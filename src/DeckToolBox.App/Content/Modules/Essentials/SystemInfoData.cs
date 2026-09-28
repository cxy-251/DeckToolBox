using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class SystemInfoData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "system-info",
        Title = "查看硬件与系统信息",
        Keywords = "hostnamectl · lscpu · lspci · lsusb · timedatectl",
        Level = ModuleLevel.Basic,
        Overview = "排查问题、求助、判断能不能运行某个程序时，第一步总是弄清楚「这是什么系统、什么硬件」。下面这些命令在几乎所有 systemd 发行版上都能用，输出也是最标准的求助附件。",

        Why =
        [
            "1. 系统身份：",
            "- `hostnamectl`：主机名、系统、内核、硬件厂商与型号、固件（BIOS）版本，一条命令覆盖大部分信息。",
            "- `uname -r`：内核版本；`cat /etc/os-release`：发行版与版本。",
            "2. 硬件清单：",
            "- `lscpu`：CPU 型号、核心/线程数、频率范围、缓存。Deck 的 APU 显示为 `AMD Custom APU 0405`（LCD 版）或 `0932`（OLED 版）。",
            "- `lspci`：PCI 设备（显卡、网卡、声卡、NVMe 控制器），`-k` 显示每个设备使用的内核驱动。",
            "- `lsusb`：USB 设备，`ID 厂商:产品` 就是驱动匹配、udev 规则中使用的编号（内置手柄为 `28de:1205`）。",
            "- `lsblk`：存储设备；`free -h`：内存；`sensors`：温度与风扇。",
            "3. 时间与语言：",
            "- `timedatectl`：本地时间、时区、是否已与网络时间同步。游戏联机、证书校验出错时，先检查时间是否正确。",
            "- `localectl`：系统语言与键盘布局。",
            "4. 电源设备：`upower -d` 列出电池、电源适配器，以及蓝牙鼠标、键盘、手柄等外设的电量。",
        ],

        Consequences =
        [
            "• 本模块的查询命令全部只读。",
            "• `timedatectl set-timezone` 修改系统时区，需要 sudo，立即生效。",
            "• `hostnamectl` 的输出包含 Machine ID，`lsusb -v`、`dmidecode` 可能包含序列号，公开分享前删除这些字段。",
        ],

        Steps =
        [
            Step.Read(
                "系统概况",
                "hostnamectl",
                "`Hardware Model: Galileo` 表示 OLED 版 Deck，`Jupiter` 为 LCD 版。"),
            Step.Read(
                "CPU 信息",
                "lscpu | head -n 20",
                "关注型号名称、CPU(s)、每个核的线程数、最大/最小频率。"),
            Step.Read(
                "PCI 设备及其驱动",
                "lspci -k | grep -A4 -E 'VGA|Display|Network|Audio'",
                "`Kernel driver in use` 一行是正在使用的驱动，如 `amdgpu`、`ath11k_pci`。"),
            Step.Read(
                "USB 设备",
                "lsusb",
                "插入新设备前后各执行一次，对比即可找到它的 `ID`。"),
            Step.Read(
                "时间、时区与同步状态",
                "timedatectl",
                "`System clock synchronized: yes` 表示时间已与网络同步。"),
            Step.Root(
                "修改时区",
                "sudo timedatectl set-timezone <时区>",
                "例如 `Asia/Shanghai`。可用 `timedatectl list-timezones | grep Asia` 查找。"),
            Step.Read(
                "电池与外设电量",
                "upower -d | grep -E 'native-path|model|percentage|state' | sed 's/^ *//'",
                "能同时看到 Deck 电池和已连接的蓝牙鼠标、键盘等设备的电量。"),
            Step.Read(
                "一次性输出求助用的基本信息",
                "{ hostnamectl; echo; lscpu | grep -iE 'model name|型号'; free -h; lsblk -o NAME,SIZE,FSTYPE,MOUNTPOINTS; } 2>&1",
                "`{ ...; }` 把多条命令的输出合在一起，方便整体复制或 `> 文件` 保存。"),
        ],

        Verify =
        [
            "1. `hostnamectl` 中的 `Operating System` 与 `cat /etc/os-release` 的 `PRETTY_NAME` 一致。",
            "2. `lsusb` 中能看到 `28de:1205 Valve Software Steam Controller`，即 Deck 的内置手柄。",
        ],
    };
}
