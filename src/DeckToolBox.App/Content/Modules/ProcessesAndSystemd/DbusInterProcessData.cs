using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.ProcessesAndSystemd;

public static class DbusInterProcessData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "dbus_inter_process",
        Title = "D-Bus 进程通信总线与 busctl 原生交互",
        Subtitle = "进程与系统编排",
        CategoryName = "进程与系统编排",
        Badge = "通信总线",
        Overview = "揭秘 Linux 桌面的“神经中枢”——D-Bus 消息总线，掌握三层寻址模型，使用系统原生 busctl 实时调用内核与桌面方法。",

        WhyCard = new InfoCardItem(
            "📌",
            "Linux 桌面的消息总线架构 (Desktop Bus Architecture)",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 为什么 Linux 系统必须拥有 D-Bus？",
                "• 管道和共享内存只适用于点对点的孤立进程通信。当有外设插拔、网络切换、媒体切歌或电池低电量时，操作系统需要一种统一的广播发布/订阅 (Pub/Sub) 与远程方法调用 (RPC) 总线，这就是 D-Bus。",
                "2. 系统总线 (System Bus) 与 会话总线 (Session Bus)：",
                "• System Bus (/run/dbus/system_bus_socket)：全机唯一。负责底层系统级服务（NetworkManager 联网、UPower 供电、UDisks2 磁盘热插拔）。",
                "• Session Bus (/run/user/<UID>/bus)：用户会话专属。负责桌面通知、多媒体播放控制 (MPRIS 规范)、KWin 窗口合成器交互。",
                "3. D-Bus 严密的三层寻址模型：",
                "• Service (服务名)：反向域名（如 org.freedesktop.Notifications）。",
                "• Object Path (对象路径)：类文件路径（如 /org/freedesktop/Notifications）。",
                "• Interface (接口名)：声明方法规范（如 org.freedesktop.Notifications）。",
                "• Method / Signal：可调用的方法（如 Notify）或主动广播的信号。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "理解总线对自动化与脚本控制的决定性作用",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 掌握原生操作，拒绝低效轮询：传统脚本动辄用 sleep 循环查看状态，开销巨大；而通过 D-Bus 监听信号，可以做到事件发生瞬间被动唤醒，0 CPU 占用！",
                "• 原生工具零依赖：systemd 自带的 busctl 以及 KDE 自带的 qdbus 是现成的高级武器，直接向总线发送命令即可操纵系统行为。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "查看当前用户会话总线上连接的所有软件与服务清单", "busctl --user list | head -n 25", "查看有哪些桌面组件正在监听或发送消息。"),
            new(2, "反射检查指定总线对象的所有公开接口与方法 (Introspect)", "busctl --system introspect org.freedesktop.UPower /org/freedesktop/UPower", "犹如查看一份实时的 API 接口说明书。"),
            new(3, "体验 D-Bus 原生 RPC 调用：向桌面弹射一条系统通知", "busctl --user call org.freedesktop.Notifications /org/freedesktop/Notifications org.freedesktop.Notifications Notify susssasa{sv}i \"DeckToolBox\" 0 \"\" \"原生总线通知\" \"直接通过 D-Bus 调起系统弹窗！\" \"[]\" \"{}\" 5000", "复制执行后，观察屏幕右上角是否立即弹出系统通知！")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "结果验证",
            "#238636",
            "#3FB950",
            new[]
            {
                "执行第三步指令后，系统右上角弹出横幅通知，终端回显 '(u 1)'（代表调用返回的新通知 ID），证明 D-Bus RPC 远程调用已完整走完内核 socket 握手流程！"
            }
        )
    };
}
