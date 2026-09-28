using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class DesktopModeBasicsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "desktop-mode",
        Title = "桌面模式上手",
        Keywords = "切换模式 · 屏幕键盘 · Konsole · 设置密码",
        Level = ModuleLevel.Basic,
        Overview = "Steam Deck 平时运行在游戏模式；切到桌面模式，它就是一台运行 KDE Plasma 的完整 Linux 电脑。本应用里几乎所有操作都在桌面模式的终端中完成，先把这几件基础的事情弄顺手。",

        Why =
        [
            "1. 两种模式：",
            "- 游戏模式：Steam 界面 + Gamescope 合成器，为手柄和游戏优化。",
            "- 桌面模式：KDE Plasma 桌面，可以使用文件管理器 Dolphin、终端 Konsole、浏览器等。",
            "• 切到桌面：按 `Steam` 键 → 电源 → 切换到桌面。切回：双击桌面上的「返回游戏模式」图标。",
            "2. 没有键盘时怎么打字：按 `Steam + X` 调出屏幕键盘。长时间使用终端，建议接一个 USB 或蓝牙键盘，或者从电脑 SSH 登录（见「SSH 远程登录与传文件」）。",
            "3. 手柄在桌面模式的默认操作：右触控板移动鼠标，`R2` 左键，`L2` 右键，右摇杆滚动。可在 Steam（桌面模式下的 Steam 窗口）里修改「桌面配置」。",
            "4. 打开终端：应用菜单（左下角）→ 系统 → Konsole。可以把它固定到任务栏。",
            "5. 使用 sudo 前先设置密码：SteamOS 出厂时 deck 用户没有密码，执行 `passwd` 设置一次即可（见「sudo、密码与用户」）。",
            "6. 你的文件在哪：家目录 `/home/deck`。截图在 `~/.local/share/Steam/userdata/<用户ID>/760/remote/<AppID>/screenshots/`；SD 卡在 `/run/media/deck/<卷标>`。",
        ],

        Consequences =
        [
            "• 在桌面模式装的软件、改的设置，切回游戏模式后依然保留。",
            "• 桌面模式的 Steam 与游戏模式是同一个客户端，在其中添加的非 Steam 游戏两边都能看到。",
            "• 桌面模式中系统的只读保护依然生效，安装软件请用 Discover (Flatpak) 或其他不修改系统分区的方式。",
        ],

        Steps =
        [
            Step.Read(
                "确认当前处于哪种会话",
                "echo \"$XDG_SESSION_TYPE  $XDG_CURRENT_DESKTOP\"",
                "桌面模式输出 `wayland  KDE`。"),
            Step.Read(
                "查看系统版本与硬件型号",
                "grep -E '^(PRETTY_NAME|VERSION_ID|BUILD_ID)=' /etc/os-release; cat /sys/devices/virtual/dmi/id/product_name",
                "`Jupiter` 是 LCD 版 Deck，`Galileo` 是 OLED 版。"),
            Step.Change(
                "设置 deck 用户密码（仅需一次）",
                "passwd",
                "输入时屏幕不显示字符，属正常。"),
            Step.Read(
                "查找你的截图",
                "ls -t ~/.local/share/Steam/userdata/*/760/remote/*/screenshots/*.jpg 2>/dev/null | head",
                "按时间从新到旧列出游戏截图。"),
            Step.Read(
                "查看本机的 IP 地址（SSH、局域网传文件时要用）",
                "ip -4 -br addr show wlan0",
                "斜杠前的部分就是 IP。"),
        ],

        Verify =
        [
            "1. 能在 Konsole 里用屏幕键盘或外接键盘输入命令并执行。",
            "2. `passwd -S` 的第二列为 `P`，表示已设置密码。",
        ],
    };
}
