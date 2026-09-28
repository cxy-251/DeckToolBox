using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class DiskHealthFirmwareData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "disk-health-firmware",
        Title = "硬盘健康、TRIM 与固件",
        Keywords = "smartctl · nvme · fstrim · fwupdmgr",
        Level = ModuleLevel.Intermediate,
        Overview = "固态硬盘会磨损，也有自己的健康记录；TRIM 帮助它长期保持性能；很多设备的固件可以在 Linux 下更新。这些都有系统自带的命令可查。",

        Why =
        [
            "1. SMART 健康数据：硬盘自己记录的运行状况。对 NVMe 固态硬盘最值得关注的几项：",
            "- `Percentage Used`：已用掉的寿命百分比（按厂商标称的写入量估算），到 100% 并不代表立刻损坏。",
            "- `Data Units Written`：累计写入量。",
            "- `Media and Data Integrity Errors`：介质错误数，应为 0。",
            "- `Critical Warning`：非 0 说明硬盘报告了严重问题，应尽快备份。",
            "• 读取 SMART 需要直接访问设备，所以要用 sudo。",
            "2. TRIM：告诉固态硬盘哪些块已经不再使用，让它提前回收，维持写入速度。普通 Linux 发行版通常启用 `fstrim.timer` 每周执行；SteamOS 由 steamos-manager 提供 `steamosctl trim-devices`。",
            "3. 固件更新：",
            "• `fwupdmgr` 从 LVFS 获取厂商发布的固件，支持很多笔记本、扩展坞、USB 接收器、固态硬盘等。",
            "• Steam Deck 自身的 BIOS 和手柄固件**不走 LVFS**，由 SteamOS 系统更新自动处理（`jupiter-biosupdate`、`jupiter-controller-update`），fwupdmgr 在 Deck 上一般显示没有可更新设备。",
        ],

        Consequences =
        [
            "• 读取 SMART、查看固件信息是只读的（需要 sudo 的只是访问权限）。",
            "• TRIM 对固态硬盘是安全的例行维护，执行期间磁盘 IO 会短暂变高。",
            "• 固件更新过程中不要断电，插上电源再操作；更新后通常需要重启。",
        ],

        Steps =
        [
            Step.Read(
                "列出硬盘型号与容量",
                "lsblk -d -o NAME,MODEL,SIZE,TRAN,ROTA",
                "`-d` 只列磁盘不列分区；`TRAN` 是接口类型；`ROTA` 为 0 表示固态存储。").Then("每个磁盘一行，如 `nvme0n1  Micron_2400_MTFDKBK512QFM  476.9G  nvme  0`、`mmcblk0  953.6G  mmc  0`。"),
            Step.Root(
                "查看 NVMe 硬盘的健康摘要",
                "sudo smartctl -H -A /dev/nvme0",
                "`-H` 总体评估，`-A` 列出各项属性。").Then("先是版本信息，然后 `SMART overall-health self-assessment test result: PASSED`，接着一张属性表：温度、`Percentage Used`、`Data Units Written`、`Power On Hours`、`Media and Data Integrity Errors` 等。"),
            Step.Root(
                "用 nvme 工具查看健康日志",
                "sudo nvme smart-log /dev/nvme0",
                "与 smartctl 的数据来源相同，字段名略有差异。").Then("一张键值表：`critical_warning`、`temperature`、`percentage_used`、`data_units_written`、`power_on_hours`、`media_errors` 等。"),
            Step.Read(
                "查看 TRIM 支持情况",
                "lsblk --discard",
                "zram 也会显示支持，它是内存设备。").Then("表格列出每个设备的 `DISC-GRAN`（TRIM 粒度）与 `DISC-MAX`（单次最大量）；非 0 表示支持 TRIM。"),
            Step.Change(
                "在 SteamOS 上执行一次 TRIM",
                "steamosctl trim-devices",
                "由 steamos-manager 执行。在其他发行版上用 `sudo fstrim -av`，会逐个显示每个挂载点回收了多少空间。").Then("通常没有输出或只输出简短结果，后台对内置硬盘和 SD 卡执行 TRIM，期间磁盘 IO 短暂升高。"),
            Step.Read(
                "查看 fwupd 识别到的设备",
                "fwupdmgr get-devices",
                "Deck 本身的 BIOS 不走 fwupd，一般标记为不可更新。").Then("一棵设备树：顶部是机型（如 `Valve Galileo`），下面列出固态硬盘、UEFI 固件等，每个设备有设备 ID、当前版本、是否可更新等字段。"),
            Step.Change(
                "检查是否有可用的固件更新",
                "fwupdmgr refresh; fwupdmgr get-updates",
                "有更新时用 `fwupdmgr update` 安装。").Then("`refresh` 显示下载元数据的进度；`get-updates` 在 Deck 上通常输出 `No updatable devices`，接上支持的扩展坞或接收器时会列出新版本。"),
        ],

        Verify =
        [
            "1. `smartctl -H` 显示 `PASSED`，`Media and Data Integrity Errors` 为 0。",
            "2. 外接的扩展坞或无线接收器若有新固件，`get-updates` 会列出它和新版本号。",
        ],
    };
}
