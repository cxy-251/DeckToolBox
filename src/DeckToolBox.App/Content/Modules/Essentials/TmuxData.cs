using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class TmuxData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "tmux",
        Title = "tmux：会话保持与分屏",
        Keywords = "tmux · detach · attach · 窗格",
        Level = ModuleLevel.Intermediate,
        Overview = "tmux 让终端里的程序脱离终端窗口独立运行：关掉 Konsole、SSH 断线，任务照常继续，之后再接回来。它还能在一个终端里分出多个窗格。SteamOS 自带 tmux。",

        Why =
        [
            "1. 会话 (session) 与脱离 (detach)：",
            "• 普通情况下，终端窗口关闭或 SSH 断开，其中运行的程序会收到挂断信号而退出。",
            "• 在 tmux 会话里运行的程序属于 tmux 服务进程；你可以随时「脱离」会话，程序继续运行，之后用 `tmux attach` 接回。",
            "• 典型用途：通过 SSH 在 Deck 上执行耗时的备份、下载、压缩，中途断线也不受影响。",
            "2. 前缀键：tmux 的所有快捷键都先按 `Ctrl+B`，松开，再按功能键：",
            "- `Ctrl+B` `d`：脱离会话。",
            "- `Ctrl+B` `%`：左右分屏；`Ctrl+B` `\"`：上下分屏；`Ctrl+B` 方向键：在窗格间切换。",
            "- `Ctrl+B` `c`：新建窗口；`Ctrl+B` `n` / `p`：下一个 / 上一个窗口。",
            "- `Ctrl+B` `[`：进入滚动模式查看历史输出，按 `q` 退出。",
            "3. 退出：在会话中所有窗格都执行 `exit` 后，会话自动结束。",
        ],

        Consequences =
        [
            "• tmux 会话在注销或关机后消失，它不能跨重启保持任务。",
            "• 忘记还有会话在后台运行时，其中的程序会一直占用资源；定期用 `tmux ls` 检查。",
            "• `tmux kill-session` 会结束会话中所有程序，未保存的工作会丢失。",
        ],

        Steps =
        [
            Step.Change(
                "新建一个命名会话",
                "tmux new -s work",
                "进入后终端底部出现绿色状态栏。"),
            Step.Change(
                "在会话中运行一个耗时任务，然后脱离",
                "rsync -avh --info=progress2 ~/Documents/ /run/media/deck/<卷标>/backup/",
                "在 tmux 会话中执行，然后按 `Ctrl+B` `d` 脱离，任务在后台继续。").On("tmux 会话内"),
            Step.Read(
                "列出所有会话",
                "tmux ls",
                "显示会话名、窗口数和是否有终端连接着它。"),
            Step.Change(
                "接回会话",
                "tmux attach -t work",
                "从 SSH 或另一个 Konsole 窗口都可以接回。"),
            Step.Change(
                "结束会话",
                "tmux kill-session -t work",
                "会结束其中所有程序。"),
        ],

        Verify =
        [
            "1. 在会话中运行 `top`，按 `Ctrl+B` `d` 脱离后关闭 Konsole；重新打开 Konsole 执行 `tmux attach -t work`，`top` 仍在运行。",
            "2. `tmux ls` 显示 `work: 1 windows ...`。",
        ],
    };
}
