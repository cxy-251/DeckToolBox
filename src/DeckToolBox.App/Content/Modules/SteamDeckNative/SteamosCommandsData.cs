using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamosCommandsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steamos-commands",
        Title = "SteamOS 系统命令集",
        Keywords = "steamos-* · jupiter-* · steam:// · 系统报告",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 在 /usr/bin 里自带了几十个 steamos-* 和 jupiter-* 命令，设置界面里的很多按钮就是调用它们。认识其中常用的几个：切换会话、更新系统、生成诊断报告、检查固件、把程序加进 Steam，以及用 steam:// 链接从终端控制 Steam。",

        Why =
        [
            "1. 这些命令大多是 shell 脚本，可以直接阅读：`less /usr/bin/steamos-session-select`。想知道设置里某个按钮做了什么，读脚本是最直接的办法。",
            "2. 常用命令一览：",
            "- `steamos-session-select`：切换会话。`plasma` 进入桌面模式，`gamescope` 回到游戏模式（桌面上「返回游戏模式」图标调用的就是它）。",
            "- `steamos-update` / `steamos-select-branch`：检查与安装系统更新、切换更新通道（见「只读系统与 A/B 更新」）。",
            "- `steamos-systemreport`：收集系统版本、硬件、驱动、音频、日志等诊断信息，输出很长，适合保存为文件附在求助帖中。",
            "- `steamos-add-to-steam <路径>`：把一个程序添加为非 Steam 游戏，相当于 Dolphin 右键菜单中的「添加到 Steam」。",
            "- `jupiter-controller-update --check`：检查内置手柄固件是否需要更新。",
            "- `steamos-readonly`：只读保护开关。",
            "- `steamos-factory-reset`：恢复出厂设置，**会清除所有用户数据**，请只通过「设置 → 系统 → 恢复出厂设置」操作。",
            "3. `steam://` 链接：Steam 运行时，执行 `steam steam://<命令>` 会把命令交给正在运行的客户端。常用的有：",
            "- `steam://rungameid/<AppID>`：启动游戏。",
            "- `steam://validate/<AppID>`：验证游戏文件完整性。",
            "- `steam://open/downloads`、`steam://open/screenshots`：打开下载页、截图管理器。",
            "- `steam://open/console`：打开 Steam 控制台（高级调试用）。",
        ],

        Consequences =
        [
            "• `--check`、查看报告、阅读脚本都是只读的。",
            "• `steamos-session-select` 会立即结束当前会话，先保存所有文件。",
            "• 不要直接运行 `jupiter-biosupdate`、`steamos-factory-reset`、`steamos-partition` 等底层命令：它们用于刷写固件、重置和分区，误用可能导致无法开机。系统会在需要时自动调用它们。",
            "• `steamos-systemreport` 的输出包含设备序列号、用户名、网络信息等，公开分享前检查并删除敏感内容。",
        ],

        Steps =
        [
            Step.Read(
                "列出所有 SteamOS 专有命令",
                "ls /usr/bin | grep -E '^(steamos-|jupiter-|galileo-)'",
                "`galileo-*` 只在 OLED 版上起作用。"),
            Step.Read(
                "查看某个命令的实现",
                "less /usr/bin/steamos-session-select",
                "按 `q` 退出。`file /usr/bin/<命令>` 可以先判断它是脚本还是二进制程序。"),
            Step.Change(
                "从终端进入桌面模式 / 回到游戏模式",
                "steamos-session-select gamescope",
                "立即结束当前会话。进入桌面模式用 `steamos-session-select plasma`。"),
            Step.Change(
                "生成系统诊断报告",
                "steamos-systemreport > ~/steamos-report-$(date +%F).txt 2>&1; ls -lh ~/steamos-report-*.txt",
                "需要十几秒。报告开头会显示只读保护状态，以及系统是否曾被以读写方式挂载过。"),
            Step.Read(
                "检查手柄固件是否需要更新",
                "jupiter-controller-update --check",
                "最后一行 `needs_update=0` 表示已是最新。"),
            Step.Change(
                "把一个程序添加到 Steam 库",
                "steamos-add-to-steam ~/Applications/<程序>",
                "需要 Steam 正在运行。添加后在「库 → 非 Steam」中出现。"),
            Step.Change(
                "从终端启动一个游戏",
                "steam steam://rungameid/<AppID>",
                "AppID 的查法见「Wine 与 Proton 原理」。需要 Steam 正在运行。"),
            Step.Change(
                "验证游戏文件完整性",
                "steam steam://validate/<AppID>",
                "与游戏属性 → 已安装文件 → 验证完整性相同，会重新下载损坏或缺失的文件。"),
        ],

        Verify =
        [
            "1. `head -n 8 ~/steamos-report-*.txt` 能看到只读保护状态与报告生成时间。",
            "2. `steam steam://open/downloads` 执行后，Steam 窗口切换到下载页面。",
        ],
    };
}
