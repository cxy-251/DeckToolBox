using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.KernelVfs;

public static class DeviceNodesUdevData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "udev",
        Title = "/dev 设备节点与 udev",
        Keywords = "/dev · udevadm · 规则",
        Level = ModuleLevel.Advanced,
        Overview = "掌握字符与块设备本质，解析 udev 动态管理机制，彻底理解手柄控制器、触摸板与外设免 root 访问权限原理。",

        Why =
        [
            "1. 字符设备 vs 块设备（一切皆文件物理抽象）：",
            "• 字符设备 (c)：按字节流顺序读写且无系统级缓冲（如 /dev/input/event* 输入设备、/dev/tty 虚拟终端、/dev/null 黑洞）。",
            "• 块设备 (b)：按固定大小扇区随机读写并自带高速缓冲（如 /dev/nvme0n1p* 固态硬盘分区、/dev/mmcblk0 SD 卡）。",
            "2. 主次设备号 (Major & Minor Numbers)：",
            "• Major（主设备号）：标识内核中绑定哪一个驱动程序模块（如 13 代表 input 输入子系统）。",
            "• Minor（次设备号）：标识该驱动程序下所管理的具体物理实例（如 event0, event1）。",
            "3. udev 动态设备管理引擎：",
            "• 当你插入 USB 手柄或蓝牙控制器时，Linux 内核抛出 uevent 事件。后台的 systemd-udevd 守护进程依据规则文件 (/etc/udev/rules.d/) 动态创建 /dev 节点。",
            "• 核心标签 TAG+=\"uaccess\"：给节点动态赋予当前登录桌面用户的 ACL 读写权限，使普通用户无需 sudo 即可操控手柄与输入设备。",
        ],

        Consequences =
        [
            "• 排查外设失灵的根本依据：很多用户连接第三方手柄、手写板或特殊摇杆后游戏无法识别，根本原因往往并非硬件损坏，而是缺少对应的 udev 规则导致 /dev/input 节点权限被拒绝！",
            "• 编写规范规则：只需在 /etc/udev/rules.d/99-gamepad.rules 添加对应的 VID/PID 权限规则，即可彻底免 root 解锁设备。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统已识别的所有输入设备节点 (事件流)",
                "ls -l /dev/input/",
                "`/dev/input` 下每个 `eventN` 对应一个输入设备。").Then("列出 `event0`、`event1`… 以及 `by-id/`、`by-path/` 两个目录；每行开头的 `c` 表示字符设备，`13, 64` 这样的两个数字是主、次设备号。"),
            Step.Read(
                "列出当前掌机全部手柄与传感器硬件名称与事件映射",
                "cat /proc/bus/input/devices | grep -E \"(Name|Handlers)\"",
                "`/proc/bus/input/devices` 是内核维护的输入设备清单。").Then("成对输出 `N: Name=\"...\"` 与 `H: Handlers=... eventN`，可以据此知道某个设备对应哪个 `eventN`。"),
            Step.Read(
                "实时动态监控硬件热插拔事件 (拔插 USB 即刻捕获)",
                "udevadm monitor --environment --udev",
                "`--environment` 显示每个事件的全部属性，`--udev` 只看 udev 处理后的事件。").Then("开始后静止等待；插拔 U 盘、手柄时，每个事件打印一段 `ACTION=add`/`remove`、`DEVNAME=`、`ID_VENDOR_ID=` 等键值。按 `Ctrl+C` 退出。"),
            Step.Read(
                "查询指定输入设备节点的完整 udev 属性树 (规则编写必备)",
                "udevadm info -a -n /dev/input/event0 | head -n 30",
                "`-a` 遍历整条父设备链，`-n` 指定设备节点；把 `event0` 换成你要查的设备。").Then("从设备本身开始逐级向上打印父设备的属性，如 `ATTRS{name}==\"Power Button\"`；写 udev 规则时就是从这里抄匹配条件。"),
        ],

        Verify =
        [
            "执行 `grep -E '^(N|H):' /proc/bus/input/devices`，能看到 `Valve Software Steam Controller` 及其 `Handlers` 中的 `eventN`；插拔 USB 设备时 `udevadm monitor` 会实时打印事件。",
        ],
    };
}
