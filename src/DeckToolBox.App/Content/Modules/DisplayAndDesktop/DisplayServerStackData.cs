using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.DisplayAndDesktop;

public static class DisplayServerStackData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "display_stack",
        Title = "显示服务与合成器全景 (X11 / Wayland / XWayland / Gamescope)",
        Subtitle = "显示与桌面规范",
        CategoryName = "显示与桌面规范",
        Badge = "图形核心",
        Overview = "全面剖析 Linux 图形显示栈演进、合成器渲染管线与 SteamOS 独特的掌机双会话架构。",

        WhyCard = new InfoCardItem(
            "📌",
            "Linux 现代图形显示全景解析 (Architecture Breakdown)",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. X11 (传统视窗系统协议)：采用经典客户端-服务端模型。应用向 X Server 发送绘图请求，由窗口管理器和合成器多次搬运缓冲。缺陷是架构臃肿、任意窗口可偷窥全局按键事件、多显示器混频易撕裂。",
                "2. Wayland (现代合成器协议)：彻底重构显示管线。合成器 (如 KWin-Wayland) 直接掌管一切。应用将画面渲染到专属显存缓冲中直接递交给合成器，合成器通过 Linux 内核 DRM/KMS 直接交送显卡点亮屏幕，实现 0 画面撕裂与极致低输入延迟。",
                "3. XWayland (无缝向下兼容层)：许多 Windows 游戏通过 Wine 运行时默认仍走 X11 API。Wayland 合成器会在后台悄然运行一个内置的 XWayland X Server，老软件绘制到 XWayland，XWayland 再无缝映射为 Wayland 表面，完全不影响用户体验。",
                "4. Gamescope (Valve 掌机定制嵌套微合成器)：作为 Wayland 嵌套合成器，它位于游戏进程与主机显示驱动之间。专为掌机量身打造：模拟任意虚假分辨率、提供硬件级 FSR (AMD 超采样)、全局锁帧与低延迟颜色校准。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "💡",
            "SteamOS 的“双会话”运作机制与边界",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 桌面模式 (Desktop Mode)：运行在标准的 KDE Plasma on Wayland 会话 (kwin_wayland) 中，DISPLAY=:0 作为 XWayland 服务，WAYLAND_DISPLAY=wayland-0 作为主通道。",
                "• 游戏模式 (Game Mode / Gamescope Session)：不启动任何 KDE 桌面组件，直接由独立 systemd 服务拉起 gamescope 作为根合成器，独占整个 GPU 与输入设备，将所有系统开销压至绝对最低。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "确认当前会话使用的视窗协议 (Wayland / X11)", "echo \"Session: $XDG_SESSION_TYPE | Wayland: $WAYLAND_DISPLAY | X11: $DISPLAY\"", "输出当前活动的显示协议与显示套接字名称。"),
            new(2, "查看当前正在前台运行的合成器进程 (KWin / Gamescope)", "ps aux | grep -E \"(kwin_wayland|gamescope)\" | grep -v grep", "确认是由 KDE KWin 主导还是 Gamescope 主导。"),
            new(3, "查看内核 DRM 显示节点连接状态", "tail -n +1 /sys/class/drm/card*-*/status 2>/dev/null", "直接读取内核暴露的显示接口点亮状态。")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "验证与观察结论",
            "#238636",
            "#3FB950",
            new[]
            {
                "如果在桌面模式下，你将看到 XDG_SESSION_TYPE=wayland，kwin_wayland 正在主导屏幕；如果在掌机游戏模式，gamescope 会直接接管 DRM 呈现管线。"
            }
        )
    };
}
