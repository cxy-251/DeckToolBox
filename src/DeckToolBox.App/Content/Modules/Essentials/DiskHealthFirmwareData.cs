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
                "`TRAN` 是接口类型（nvme、usb、mmc），`ROTA` 为 0 表示固态硬盘。"),
            Step.Root(
                "查看 NVMe 硬盘的健康摘要",
                "sudo smartctl -H -A /dev/nvme0",
                "`-H` 总体评估（PASSED 即正常），`-A` 列出各项属性，重点看上面提到的几项。"),
            Step.Root(
                "用 nvme 工具查看健康日志",
                "sudo nvme smart-log /dev/nvme0",
                "与 smartctl 的数据来源相同，字段名略有差异。"),
            Step.Read(
                "查看 TRIM 支持情况",
                "lsblk --discard",
                "`DISC-GRAN` 与 `DISC-MAX` 非 0 表示该设备支持 TRIM。"),
            Step.Change(
                "在 SteamOS 上执行一次 TRIM",
                "steamosctl trim-devices",
                "由 steamos-manager 对适用的设备执行 TRIM。在其他发行版上用 `sudo fstrim -av`。"),
            Step.Read(
                "查看 fwupd 识别到的设备",
                "fwupdmgr get-devices",
                "列出可由 fwupd 管理的设备及当前固件版本。"),
            Step.Change(
                "检查是否有可用的固件更新",
                "fwupdmgr refresh; fwupdmgr get-updates",
                "`refresh` 下载最新的固件目录。有更新时用 `fwupdmgr update` 安装。"),
        ],

        Verify =
        [
            "1. `smartctl -H` 显示 `PASSED`，`Media and Data Integrity Errors` 为 0。",
            "2. 外接的扩展坞或无线接收器若有新固件，`get-updates` 会列出它和新版本号。",
        ],
    };
}
