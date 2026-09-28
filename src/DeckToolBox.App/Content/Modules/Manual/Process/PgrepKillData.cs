using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Process;

public static class PgrepKillData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-pgrep-kill",
        Title = "pgrep、pkill、killall 与 kill",
        Keywords = "pgrep -a -f · pkill · killall · kill -l · -STOP/-CONT",
        Level = ModuleLevel.Basic,
        Overview = "kill 按 PID 给进程发信号，pgrep 按名称查找 PID，pkill 和 killall 按名称直接发信号。除了结束进程，信号还能暂停和恢复进程，例如临时冻结一个占 CPU 的后台任务。",

        Why =
        [
            "1. pgrep：按条件查找进程，输出 PID。",
            "- 默认匹配进程名（最多 15 个字符）；`-f` 匹配完整命令行。",
            "- `-a` 同时显示命令行；`-l` 显示进程名；`-c` 只输出数量。",
            "- `-u 用户` 只找该用户的进程；`-n` 最新的一个、`-o` 最早的一个；`-x` 名称必须完全相同。",
            "2. pkill：参数与 pgrep 相同，但把信号发给所有匹配的进程，默认 SIGTERM。**先用同样的参数执行 pgrep 确认匹配结果。**",
            "3. killall：按**完整**进程名发送信号（`killall steam` 不会匹配 `steamwebhelper`），`-i` 逐个确认，`-w` 等待进程真正退出。",
            "4. kill：`kill PID`（SIGTERM）、`kill -9 PID`（SIGKILL）、`kill -STOP PID`（暂停）、`kill -CONT PID`（继续）；`kill -l` 列出所有信号。",
            "5. 暂停与继续：SIGSTOP 让进程停在原地（不占 CPU，但仍占内存），SIGCONT 让它从停下的地方继续。可以用来临时冻结一个后台编码任务，把 CPU 让给游戏。",
        ],

        Consequences =
        [
            "• 按名称发送信号可能同时命中多个进程，包括你没想到的同名程序。",
            "• SIGKILL (`-9`) 和 SIGSTOP 无法被程序拦截；被 SIGSTOP 暂停的图形程序窗口会停止响应，直到收到 SIGCONT。",
            "• 普通用户只能向自己的进程发信号。",
        ],

        Steps =
        [
            Step.Read(
                "按名称查找进程",
                "pgrep -l steam; echo \"steamwebhelper 共 $(pgrep -c steamwebhelper) 个\"",
                "`-l` 显示进程名，`-c` 计数。").Then("列出 `3969 steam`、`929 steamos-manager` 等，再输出 `steamwebhelper 共 10 个` 这样的计数。"),
            Step.Read(
                "按完整命令行查找",
                "pgrep -af 'chrome.*--type=renderer' | head -n 3 | cut -c1-120",
                "`-f` 在完整命令行中匹配，`-a` 显示命令行。").Then("列出 Chrome 的渲染进程及其（截断的）命令行；没有运行 Chrome 时无输出。"),
            Step.Change(
                "启动一个练习用的后台进程",
                "cp /usr/bin/sleep /tmp/demo-sleeper; /tmp/demo-sleeper 600 & echo \"PID=$!\"",
                "把 sleep 复制成一个专用名字，后面的 killall 只会命中它，不会误伤其他程序；`$!` 是最近一个后台进程的 PID。").Then("输出 `[1] 12345` 和 `PID=12345`。"),
            Step.Change(
                "暂停与继续",
                "kill -STOP $!; ps -o pid,stat,cmd -p $!; kill -CONT $!; ps -o pid,stat,cmd -p $!",
                "`$!` 仍指向上一步的 sleep。").Then("第一次 ps 的 STAT 为 `T`（已暂停），第二次恢复为 `S`（睡眠）。"),
            Step.Change(
                "按完整进程名结束",
                "killall -v demo-sleeper; rm -f /tmp/demo-sleeper",
                "`-v` 报告发送了信号的进程。killall 匹配完整名称。").Then("输出 `demo-sleeper(12345) 被信号 15 杀死`；终端随后提示该作业 `已终止`。"),
            Step.Read(
                "查看信号编号",
                "kill -l | head -n 4",
                "信号名可以省略 `SIG` 前缀，如 `kill -TERM`、`kill -HUP`。").Then("以表格列出 `1) SIGHUP 2) SIGINT 3) SIGQUIT ... 9) SIGKILL ... 15) SIGTERM` 等。"),
        ],

        Verify =
        [
            "1. 练习结束后 `pgrep -x demo-sleeper` 没有输出。",
        ],
    };
}
