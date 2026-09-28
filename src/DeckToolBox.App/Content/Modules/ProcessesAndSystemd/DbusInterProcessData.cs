using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.ProcessesAndSystemd;

public static class DbusInterProcessData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "dbus",
        Title = "D-Bus 消息总线",
        Keywords = "busctl · session/system bus",
        Level = ModuleLevel.Advanced,
        Overview = "揭秘 Linux 桌面的“神经中枢”——D-Bus 消息总线，掌握三层寻址模型，使用系统原生 busctl 实时调用内核与桌面方法。",

        Why =
        [
            "1. 为什么 Linux 系统必须拥有 D-Bus？",
            "• 管道和共享内存只适用于点对点的孤立进程通信。当有外设插拔、网络切换、媒体切歌或电池低电量时，操作系统需要一种统一的广播发布/订阅 (Pub/Sub) 与远程方法调用 (RPC) 总线，这就是 D-Bus。",
            "2. 系统总线 (System Bus) 与 会话总线 (Session Bus)：",
            "• System Bus (/run/dbus/system_bus_socket)：全机唯一。负责底层系统级服务（NetworkManager 联网、UPower 供电、UDisks2 磁盘热插拔）。",
            "• Session Bus (/run/user/<UID>/bus)：用户会话专属。负责桌面通知、多媒体播放控制 (MPRIS 规范)、KWin 窗口合成器交互。",
            "3. D-Bus 严密的三层寻址模型：",
            "• Service (服务名)：反向域名（如 org.freedesktop.Notifications）。",
            "• Object Path (对象路径)：类文件路径（如 /org/freedesktop/Notifications）。",
            "• Interface (接口名)：声明方法规范（如 org.freedesktop.Notifications）。",
            "• Method / Signal：可调用的方法（如 Notify）或主动广播的信号。",
        ],

        Consequences =
        [
            "• 掌握原生操作，拒绝低效轮询：传统脚本动辄用 sleep 循环查看状态，开销巨大；而通过 D-Bus 监听信号，可以做到事件发生瞬间被动唤醒，0 CPU 占用！",
            "• 原生工具零依赖：systemd 自带的 busctl 以及 KDE 自带的 qdbus 是现成的高级武器，直接向总线发送命令即可操纵系统行为。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前用户会话总线上连接的所有软件与服务清单",
                "busctl --user list --no-pager | sed -n '1,25p'",
                "会话总线上是桌面组件与应用。").Then("表格列出前 25 个连接：总线名（`:1.23` 或 `org.kde.xxx`）、PID、进程名、用户等。"),
            Step.Read(
                "反射检查指定总线对象的所有公开接口与方法 (Introspect)",
                "busctl --system introspect org.freedesktop.UPower /org/freedesktop/UPower",
                "`introspect` 让服务报告自己提供了哪些方法、属性与信号。").Then("按接口分组列出 `.EnumerateDevices`（方法）、`.OnBattery`（属性，带当前值）、`.DeviceAdded`（信号）等成员。"),
            Step.Change(
                "直接调用 D-Bus 方法：向桌面发一条通知",
                "busctl --user call org.freedesktop.Notifications /org/freedesktop/Notifications org.freedesktop.Notifications Notify susssasa{sv}i DeckToolBox 0 '' 'D-Bus 测试' '这条通知是直接调用总线方法发出的' 0 0 5000",
                "签名 `susssasa{sv}i` 依次对应：应用名、替换 ID、图标、标题、正文、动作数组（`0` 表示空数组）、提示字典（`0` 表示空）、超时毫秒。`notify-send` 做的就是同一次调用。").Then("桌面右下角弹出一条标题为「D-Bus 测试」的通知，5 秒后消失；终端输出 `u 17` 这样的返回值（通知 ID）。"),
        ],

        Verify =
        [
            "执行最后一步后桌面弹出通知，终端输出形如 `u 17`：返回值类型为 u（无符号整数），数值是新通知的 ID。",
        ],
    };
}
