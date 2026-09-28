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
            "• 桌面模式中系统的只读保护依然生效，安装软件请用便携应用（`~/Applications`）或 Distrobox 容器，见「不用 Flatpak：清理与替代」。",
        ],

        Steps =
        [
            Step.Read(
                "确认当前处于哪种会话",
                "echo \"$XDG_SESSION_TYPE  $XDG_CURRENT_DESKTOP\"",
                "`$XDG_SESSION_TYPE` 是会话类型，`$XDG_CURRENT_DESKTOP` 是桌面环境名。").Then("输出一行文字。桌面模式下是 `wayland  KDE`；若为空，说明你是通过 SSH 等非图形会话登录的。"),
            Step.Read(
                "查看系统版本与硬件型号",
                "grep -E '^(PRETTY_NAME|VERSION_ID|BUILD_ID)=' /etc/os-release; cat /sys/devices/virtual/dmi/id/product_name",
                "`grep -E` 只挑出这三个字段；`product_name` 是主板固件报告的机型代号。").Then("先输出三行 `名称=值`（系统名、版本号、构建日期），最后一行是机型：`Jupiter` 表示 LCD 版，`Galileo` 表示 OLED 版。"),
            Step.Change(
                "设置 deck 用户密码（仅需一次）",
                "passwd",
                "为当前用户设置登录密码。").Then("依次提示「新的密码」「重新输入新的密码」（已有密码时先要求输入当前密码）。输入时屏幕上**不显示任何字符**，这是正常的。成功后显示 `passwd：已成功更新密码`，此后 sudo、SSH 登录都使用这个密码。"),
            Step.Read(
                "查找你的截图",
                "ls -t ~/.local/share/Steam/userdata/*/760/remote/*/screenshots/*.jpg 2>/dev/null | head",
                "`ls -t` 按修改时间排序，`2>/dev/null` 隐藏「找不到文件」的报错，`head` 只取前 10 行。").Then("按时间从新到旧列出最多 10 个截图文件的完整路径；没有任何截图时没有输出。"),
            Step.Read(
                "查看本机的 IP 地址（SSH、局域网传文件时要用）",
                "ip -4 -br addr show wlan0",
                "`-4` 只看 IPv4，`-br` 简洁格式，`show wlan0` 只看无线网卡。").Then("输出一行，例如 `wlan0  UP  192.168.0.21/24`：`UP` 表示网卡已连接，斜杠前的 `192.168.0.21` 就是 Deck 在局域网里的地址。"),
        ],

        Verify =
        [
            "1. 能在 Konsole 里用屏幕键盘或外接键盘输入命令并执行。",
            "2. `passwd -S` 的第二列为 `P`，表示已设置密码。",
        ],
    };
}
