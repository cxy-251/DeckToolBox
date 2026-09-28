using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class KdeCliData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "kde-cli",
        Title = "用命令行控制 KDE 桌面",
        Keywords = "kscreen-doctor · spectacle · kreadconfig6 · loginctl",
        Level = ModuleLevel.Intermediate,
        Overview = "KDE Plasma 的大部分设置都能在终端里完成：显示器分辨率与缩放、截图录屏、读写配置、锁屏、发通知、打开文件。适合写成脚本，或在只有 SSH 的时候远程操作桌面。",

        Why =
        [
            "1. 显示器：`kscreen-doctor` 直接调整输出设备。",
            "- `kscreen-doctor -o` 列出所有输出（名称如 `eDP-1` 内置屏、`DP-1` 外接屏）及其模式、缩放、位置。",
            "- 设置写成 `output.<名称>.<属性>.<值>`，例如 `output.DP-1.mode.1920x1080@60`、`output.eDP-1.scale.1.25`、`output.eDP-1.disable`。",
            "2. 截图与录屏：`spectacle` 在命令行下 `-b` 后台运行、`-n` 不弹通知、`-o` 指定文件；`-f` 全屏、`-a` 当前窗口、`-r` 框选区域。",
            "3. 配置文件：KDE 的设置保存在 `~/.config/` 下的 ini 格式文件中（如 `kdeglobals`、`kwinrc`）。`kreadconfig6` / `kwriteconfig6` 按「文件 → 组 → 键」读写，比手动编辑更不容易出错。部分设置需要重新登录才生效。",
            "4. 其他：",
            "- `loginctl lock-session`：锁屏。",
            "- `notify-send '标题' '内容'`：发送桌面通知，脚本完成时提醒自己。",
            "- `kde-open <文件或网址>`：用默认程序打开。",
            "- `plasma-apply-colorscheme`、`plasma-apply-wallpaperimage`：切换配色方案、设置壁纸。",
            "5. 这些命令需要在 KDE 会话中运行；通过 SSH 执行时，需要先设置 `WAYLAND_DISPLAY=wayland-0` 等环境变量，或借助 `systemd-run --user` 在会话环境中运行。",
        ],

        Consequences =
        [
            "• 查询类命令（`kscreen-doctor -o`、`kreadconfig6`）只读。",
            "• `kscreen-doctor` 不会像设置界面那样弹出「保留更改」倒计时，改动立即生效且不会自动还原。禁用唯一的屏幕会导致黑屏，只能插上外接屏或通过 SSH 执行 `kscreen-doctor output.eDP-1.enable` 恢复。",
            "• `kwriteconfig6` 直接修改配置文件，修改前可用 `kreadconfig6` 记下原值。",
        ],

        Steps =
        [
            Step.Read(
                "列出显示器及其模式、缩放",
                "kscreen-doctor -o",
                "Deck 的内置屏显示为 `eDP-1`，模式 `800x1280@90`（OLED 版），因为面板物理上是竖屏。"),
            Step.Change(
                "设置外接显示器的分辨率与刷新率",
                "kscreen-doctor output.<输出名>.mode.<宽>x<高>@<刷新率>",
                "例如 `kscreen-doctor output.DP-1.mode.1920x1080@60`。可用模式见上一步的 `Modes` 列表。"),
            Step.Change(
                "设置缩放比例",
                "kscreen-doctor output.<输出名>.scale.<比例>",
                "例如 `output.eDP-1.scale.1.25`。"),
            Step.Change(
                "关闭屏幕（挂机下载时省电），按任意键或触摸唤醒",
                "sleep 2; kscreen-doctor --dpms off",
                "`sleep 2` 留出松开按键的时间，否则回车键本身会立即把屏幕唤醒。与休眠不同，系统和网络继续运行。"),
            Step.Change(
                "后台全屏截图并保存",
                "spectacle -b -n -f -o ~/Pictures/screen-$(date +%F-%H%M%S).png",
                "`-f` 换成 `-r` 可以框选区域。"),
            Step.Read(
                "读取一项 KDE 配置",
                "kreadconfig6 --file kdeglobals --group General --key ColorScheme",
                "SteamOS 默认配色方案名为 `Vapor`。"),
            Step.Change(
                "切换配色方案",
                "plasma-apply-colorscheme --list-schemes; plasma-apply-colorscheme <方案名>",
                "先列出可用方案，再应用。换回默认：`plasma-apply-colorscheme Vapor`。"),
            Step.Change(
                "脚本结束时发送桌面通知",
                "<命令>; notify-send '任务完成' \"退出码 $?\"",
                "`$?` 是上一条命令的退出码，0 表示成功。"),
            Step.Change(
                "锁屏",
                "loginctl lock-session",
                "所有使用 systemd-logind 的桌面都支持。"),
        ],

        Verify =
        [
            "1. 截图命令执行后，`ls ~/Pictures` 中出现新的 png 文件。",
            "2. 应用配色方案后窗口颜色立即变化，`kreadconfig6` 读到的 `ColorScheme` 为新方案名。",
        ],
    };
}
