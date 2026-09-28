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
                "busctl --user list | head -n 25",
                "查看有哪些桌面组件正在监听或发送消息。"),
            Step.Read(
                "反射检查指定总线对象的所有公开接口与方法 (Introspect)",
                "busctl --system introspect org.freedesktop.UPower /org/freedesktop/UPower",
                "犹如查看一份实时的 API 接口说明书。"),
            Step.Read(
                "体验 D-Bus 原生 RPC 调用：向桌面弹射一条系统通知",
                "busctl --user call org.freedesktop.Notifications /org/freedesktop/Notifications org.freedesktop.Notifications Notify susssasa{sv}i \"DeckToolBox\" 0 \"\" \"原生总线通知\" \"直接通过 D-Bus 调起系统弹窗！\" \"[]\" \"{}\" 5000",
                "复制执行后，观察屏幕右上角是否立即弹出系统通知！"),
        ],

        Verify =
        [
            "执行第三步指令后，系统右上角弹出横幅通知，终端回显 '(u 1)'（代表调用返回的新通知 ID），证明 D-Bus RPC 远程调用已完整走完内核 socket 握手流程！",
        ],
    };
}
