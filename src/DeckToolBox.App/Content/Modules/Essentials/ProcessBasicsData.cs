using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class ProcessBasicsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "process-basics",
        Title = "查看与结束程序",
        Keywords = "htop · btop · pgrep · kill · 强制关闭窗口",
        Level = ModuleLevel.Basic,
        Overview = "程序卡死、风扇转速一直很高、电量下降很快——先找出是哪个进程，再决定是正常关闭还是强制结束。图形界面和命令行各有一套方法。",

        Why =
        [
            "1. 进程与 PID：每个运行中的程序至少是一个进程，有唯一的数字编号 PID。`kill`、`renice` 等命令都通过 PID 指定进程。",
            "2. 看资源占用的工具（SteamOS 均自带）：",
            "- `htop`：经典的交互式进程查看器，`F6` 选择排序列，`F9` 发送信号，`q` 退出。",
            "- `btop`：更直观的图形化界面，同时显示 CPU、内存、磁盘、网络。",
            "- KDE「系统监视器」：`Ctrl+Esc` 打开。",
            "3. 结束进程的正确顺序：",
            "- 先发 `SIGTERM`（`kill <PID>` 的默认信号）：请程序自己保存状态后退出。",
            "- 等几秒仍未退出，再发 `SIGKILL`（`kill -9`）：内核立即终止进程，程序没有机会保存任何东西。",
            "4. 窗口卡死时：桌面模式按 `Meta+Ctrl+Esc`（Meta 即 Windows 键），鼠标变成骷髅后点击卡死的窗口即可强制关闭它。游戏模式中按 `Steam` 键，在游戏条目上选择「退出游戏」。",
        ],

        Consequences =
        [
            "• 查看进程只读无害。",
            "• 强制结束 (`-9`) 会丢失程序中未保存的数据；游戏在写存档时被杀，存档可能损坏。",
            "• `pkill` / `killall` 按名称匹配，可能一次结束多个进程。先用 `pgrep -a` 看看会匹配到哪些。",
            "• 普通用户只能结束自己的进程。不要结束 `systemd`、`kwin_wayland`、`plasmashell` 等系统和桌面进程，否则桌面会重启或退出。",
        ],

        Steps =
        [
            Step.Read(
                "交互式查看进程",
                "btop",
                "或者使用 `htop`，界面更简洁。").Then("终端变成全屏的监控界面：上方是 CPU 各核心、内存、网络、磁盘的图表，下方是进程列表，持续刷新。方向键选择进程，`q` 退出。"),
            Step.Read(
                "占用 CPU 最多的 10 个进程",
                "ps -eo pid,pcpu,pmem,comm --sort=-pcpu | head -n 11",
                "`pcpu` 为 CPU 占用百分比（多核时可超过 100），`pmem` 为内存占用百分比。").Then("第一行表头 `PID %CPU %MEM COMMAND`，下面 10 行按 CPU 占用从高到低排列。"),
            Step.Read(
                "按名称查找进程",
                "pgrep -a <程序名关键词>",
                "`<程序名关键词>` 填入进程名或命令行的关键字（如 `chrome` 或 `steam`）。`-a` 列出 PID 与完整命令行。").Then("每行「PID 完整命令行」，如 `550696 /home/deck/Applications/chrome/chrome --user-data-dir=...`；没有匹配时没有输出。"),
            Step.Change(
                "请求程序正常退出",
                "kill <PID>",
                "发送 SIGTERM；`<PID>` 用上一步查到的数字。").Then("没有输出。程序收到退出请求，保存数据后关闭（窗口消失）；若程序卡死不响应，它可能不会退出。"),
            Step.Danger(
                "强制结束不响应的程序",
                "kill -9 <PID>",
                "`<PID>` 为进程纯数字 ID。发送 SIGKILL 强制终止，程序无法拒绝与拦截。仅在正常退出无效时使用。").Then("没有输出，进程立即消失，窗口关闭，未保存的内容丢失。"),
            Step.Change(
                "按名称结束所有匹配的进程",
                "pkill -f <程序名关键词>",
                "`<程序名关键词>` 为进程特征词。`-f` 匹配完整命令行。先用 `pgrep -a <程序名关键词>` 确认匹配结果，避免误杀。").Then("没有输出，所有命令行中包含该关键词的进程都收到退出请求。"),
        ],

        Verify =
        [
            "1. 执行 `kill <PID>` 后，`ps -p <PID>` 不再列出该进程。",
            "2. 在桌面模式打开一个窗口，按 `Meta+Ctrl+Esc` 后点击它，窗口立即关闭。",
        ],
    };
}
