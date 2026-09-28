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
        Overview = "Linux 的图形显示经历了从 X11 到 Wayland 的转变。了解合成器、XWayland 与内核 DRM/KMS 的关系，就能理解 SteamOS 桌面模式与游戏模式的区别，以及某些程序在 Wayland 下表现不同的原因。",

        Why =
        [
            "1. X11：客户端—服务器模型。程序把绘制请求发给 X 服务器，由窗口管理器和合成器负责窗口摆放与叠加。协议历史悠久、兼容性好，但设计上任何程序都能读取其他窗口的内容和全局按键，多显示器不同刷新率的支持也较差。",
            "2. Wayland：合成器本身就是显示服务器。程序把画好的缓冲区直接交给合成器（如 KWin），合成器通过内核的 DRM/KMS 接口把最终画面送到屏幕。程序之间相互隔离，截屏、全局快捷键需要经过合成器或 XDG Portal 授权。",
            "3. XWayland：仍然使用 X11 的程序（包括大量通过 Wine/Proton 运行的游戏）连接到一个由合成器启动的 X 服务器，其窗口再作为普通 Wayland 窗口显示。",
            "4. Gamescope：在 Deck 游戏模式中它是唯一的合成器，直接驱动屏幕；在桌面模式或其他电脑上，它也可以作为嵌套合成器在一个窗口里运行游戏。提供分辨率缩放 (FSR/NIS)、帧率限制等功能（见「Gamescope 合成器与缩放」）。",
        ],

        Consequences =
        [
            "• 桌面模式：KDE Plasma 运行在 Wayland 上（合成器 `kwin_wayland`），`WAYLAND_DISPLAY=wayland-0`，同时提供 XWayland（`DISPLAY=:0`）给 X11 程序。",
            "• 游戏模式：不启动 Plasma，由 systemd 启动 gamescope 会话，Steam 和游戏都在其中运行。",
            "• 只支持 X11 的工具（如 `xdotool`、`xrandr`）在 Wayland 会话中只能作用于 XWayland 窗口，无法控制整个桌面；对应功能应改用 `kscreen-doctor` 等 Wayland 下的工具。",
        ],

        Steps =
        [
            Step.Read(
                "确认当前会话类型",
                "echo \"Session: $XDG_SESSION_TYPE | Wayland: $WAYLAND_DISPLAY | X11: $DISPLAY\"",
                "三个环境变量分别表示会话类型、Wayland 套接字名、X11 显示编号。").Then("输出一行，桌面模式下为 `Session: wayland | Wayland: wayland-0 | X11: :0`：会话是 Wayland，同时运行着供旧程序使用的 XWayland (`:0`)。"),
            Step.Read(
                "查看正在运行的合成器",
                "ps aux | grep -E \"(kwin_wayland|gamescope)\" | grep -v grep",
                "`grep -v grep` 去掉 grep 自己这一行。").Then("桌面模式下列出 `kwin_wayland` 进程（用户、PID、CPU%、内存、启动参数）；游戏模式下列出 `gamescope`。"),
            Step.Read(
                "查看各显示接口的连接状态",
                "tail -n +1 /sys/class/drm/card*-*/status 2>/dev/null",
                "`tail -n +1` 用于一次显示多个文件并带上文件名标题。").Then("按文件分段输出，每段标题为 `==> .../card0-eDP-1/status <==`，下面一行 `connected` 或 `disconnected`。"),
        ],

        Verify =
        [
            "1. 桌面模式中 `echo $XDG_SESSION_TYPE` 输出 `wayland`。",
            "2. `ps aux | grep -c '[X]wayland'` 大于 0，说明 XWayland 正在为 X11 程序服务。",
        ],
    };
}
