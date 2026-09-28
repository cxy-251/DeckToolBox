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
        Overview = "/dev 下的设备文件是程序访问硬件的入口。它们由 udev 在设备出现时动态创建，并按规则设置名称和权限。弄清这套机制，就能解释「手柄插上却没有反应」「普通用户为什么能读取某个设备」这类问题。",

        Why =
        [
            "1. 设备文件的两种类型：",
            "- 字符设备（`ls -l` 首字母 `c`）：按字节流读写，如输入设备 `/dev/input/event*`、终端 `/dev/tty*`、`/dev/null`。",
            "- 块设备（首字母 `b`）：按块随机读写，经过内核缓存，如硬盘 `/dev/nvme0n1`、SD 卡 `/dev/mmcblk0`。",
            "2. 主、次设备号：`ls -l` 中大小的位置显示两个数字。主设备号对应内核中的驱动，次设备号区分同一驱动管理的多个设备。",
            "3. udev 的工作流程：",
            "- 设备插入时，内核发出 uevent 事件，带有厂商 ID、产品 ID、设备类型等属性。",
            "- `systemd-udevd` 按 `/usr/lib/udev/rules.d/`（系统）和 `/etc/udev/rules.d/`（本机）中的规则匹配这些属性，创建设备节点、设置权限、加载驱动或运行程序。",
            "- `TAG+=\"uaccess\"`：让当前登录桌面的用户获得该设备的读写权限（通过 ACL 实现），手柄、声卡等设备因此无需 root 就能使用。",
        ],

        Consequences =
        [
            "• `ls /dev`、`udevadm info`、`udevadm monitor` 都是只读的。",
            "• 第三方手柄、特殊外设无法识别或无法使用时，常见原因之一是缺少对应的 udev 规则，导致设备节点没有授权给当前用户。",
            "• 在 SteamOS 上可以在 `/etc/udev/rules.d/` 添加自定义规则（需要 root），保存在 `/etc` 覆盖层中，系统更新后保留。修改后执行 `sudo udevadm control --reload` 并重新插拔设备生效。",
        ],

        Steps =
        [
            Step.Read(
                "查看输入设备节点",
                "ls -l /dev/input/",
                "`/dev/input` 下每个 `eventN` 对应一个输入设备。").Then("列出 `event0`、`event1`… 以及 `by-id/`、`by-path/` 两个目录；每行开头的 `c` 表示字符设备，`13, 64` 这样的两个数字是主、次设备号。"),
            Step.Read(
                "查看每个输入设备对应的事件节点",
                "cat /proc/bus/input/devices | grep -E \"(Name|Handlers)\"",
                "`/proc/bus/input/devices` 是内核维护的输入设备清单。").Then("成对输出 `N: Name=\"...\"` 与 `H: Handlers=... eventN`，可以据此知道某个设备对应哪个 `eventN`。"),
            Step.Read(
                "实时观察设备插拔事件",
                "udevadm monitor --environment --udev",
                "`--environment` 显示每个事件的全部属性，`--udev` 只看 udev 处理后的事件。").Then("开始后静止等待；插拔 U 盘、手柄时，每个事件打印一段 `ACTION=add`/`remove`、`DEVNAME=`、`ID_VENDOR_ID=` 等键值。按 `Ctrl+C` 退出。"),
            Step.Read(
                "查看一个设备的 udev 属性（编写规则时使用）",
                "udevadm info -a -n /dev/input/event0 | head -n 30",
                "`-a` 遍历整条父设备链，`-n` 指定设备节点；把 `event0` 换成你要查的设备。").Then("从设备本身开始逐级向上打印父设备的属性，如 `ATTRS{name}==\"Power Button\"`；写 udev 规则时就是从这里抄匹配条件。"),
        ],

        Verify =
        [
            "1. 插入 U 盘时，`udevadm monitor` 中出现 `add` 事件，随后 `lsblk` 中出现新设备。",
            "2. `grep -E '^(N|H):' /proc/bus/input/devices` 中能找到 `Valve Software Steam Controller` 及其对应的 `eventN`。",
        ],
    };
}
