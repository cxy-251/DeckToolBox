using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.DisplayAndDesktop;

public static class DisplayServerStackData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "display-stack",
        Title = "Wayland、X11 与合成器",
        Keywords = "Wayland · XWayland · KWin",
        Level = ModuleLevel.Intermediate,
        Overview = "全面剖析 Linux 图形显示栈演进、合成器渲染管线与 SteamOS 独特的掌机双会话架构。",

        Why =
        [
            "1. X11 (传统视窗系统协议)：采用经典客户端-服务端模型。应用向 X Server 发送绘图请求，由窗口管理器和合成器多次搬运缓冲。缺陷是架构臃肿、任意窗口可偷窥全局按键事件、多显示器混频易撕裂。",
            "2. Wayland (现代合成器协议)：彻底重构显示管线。合成器 (如 KWin-Wayland) 直接掌管一切。应用将画面渲染到专属显存缓冲中直接递交给合成器，合成器通过 Linux 内核 DRM/KMS 直接交送显卡点亮屏幕，实现 0 画面撕裂与极致低输入延迟。",
            "3. XWayland (无缝向下兼容层)：许多 Windows 游戏通过 Wine 运行时默认仍走 X11 API。Wayland 合成器会在后台悄然运行一个内置的 XWayland X Server，老软件绘制到 XWayland，XWayland 再无缝映射为 Wayland 表面，完全不影响用户体验。",
            "4. Gamescope（Valve 的合成器）：在 Deck 游戏模式中它是唯一的合成器，直接驱动屏幕；在桌面模式或其他电脑上，它也可以作为嵌套合成器在一个窗口里运行游戏。提供分辨率缩放 (FSR/NIS)、帧率限制等功能（见「Gamescope 合成器与缩放」）。",
        ],

        Consequences =
        [
            "• 桌面模式 (Desktop Mode)：运行在标准的 KDE Plasma on Wayland 会话 (kwin_wayland) 中，DISPLAY=:0 作为 XWayland 服务，WAYLAND_DISPLAY=wayland-0 作为主通道。",
            "• 游戏模式 (Game Mode / Gamescope Session)：不启动任何 KDE 桌面组件，直接由独立 systemd 服务拉起 gamescope 作为根合成器，独占整个 GPU 与输入设备，将所有系统开销压至绝对最低。",
        ],

        Steps =
        [
            Step.Read(
                "确认当前会话使用的视窗协议 (Wayland / X11)",
                "echo \"Session: $XDG_SESSION_TYPE | Wayland: $WAYLAND_DISPLAY | X11: $DISPLAY\"",
                "三个环境变量分别表示会话类型、Wayland 套接字名、X11 显示编号。").Then("输出一行，桌面模式下为 `Session: wayland | Wayland: wayland-0 | X11: :0`：会话是 Wayland，同时运行着供旧程序使用的 XWayland (`:0`)。"),
            Step.Read(
                "查看当前正在前台运行的合成器进程 (KWin / Gamescope)",
                "ps aux | grep -E \"(kwin_wayland|gamescope)\" | grep -v grep",
                "`grep -v grep` 去掉 grep 自己这一行。").Then("桌面模式下列出 `kwin_wayland` 进程（用户、PID、CPU%、内存、启动参数）；游戏模式下列出 `gamescope`。"),
            Step.Read(
                "查看内核 DRM 显示节点连接状态",
                "tail -n +1 /sys/class/drm/card*-*/status 2>/dev/null",
                "`tail -n +1` 用于一次显示多个文件并带上文件名标题。").Then("按文件分段输出，每段标题为 `==> .../card0-eDP-1/status <==`，下面一行 `connected` 或 `disconnected`。"),
        ],

        Verify =
        [
            "如果在桌面模式下，你将看到 XDG_SESSION_TYPE=wayland，kwin_wayland 正在主导屏幕；如果在掌机游戏模式，gamescope 会直接接管 DRM 呈现管线。",
        ],
    };
}
