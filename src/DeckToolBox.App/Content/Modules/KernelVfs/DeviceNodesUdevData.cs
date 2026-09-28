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
                "查看字符设备属性 (crw-rw----) 与主次设备号。"),
            Step.Read(
                "列出当前掌机全部手柄与传感器硬件名称与事件映射",
                "cat /proc/bus/input/devices | grep -E \"(Name|Handlers)\"",
                "定位当前手柄对应的 event* 与 js* 节点编号。"),
            Step.Read(
                "实时动态监控硬件热插拔事件 (拔插 USB 即刻捕获)",
                "udevadm monitor --environment --udev",
                "按下 Ctrl+C 退出。拔插任意设备时，会实时在屏幕上滚出内核 uevent 详细属性。"),
            Step.Read(
                "查询指定输入设备节点的完整 udev 属性树 (规则编写必备)",
                "udevadm info -a -n /dev/input/event0 | head -n 30",
                "提取设备的 idVendor、idProduct 等关键匹配字段。"),
        ],

        Verify =
        [
            "在终端执行 cat /proc/bus/input/devices，你会看到 Steam Controller 或 Valve Neptune Controller 的设备声明，并在 Handlers 处绑定了对应的 event 编号。",
        ],
    };
}
