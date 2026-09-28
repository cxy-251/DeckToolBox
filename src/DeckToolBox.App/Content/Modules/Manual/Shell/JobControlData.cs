using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class JobControlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-job-control",
        Title = "后台运行与作业控制",
        Keywords = "& · Ctrl+Z · jobs · fg · bg · nohup · setsid · disown",
        Level = ModuleLevel.Intermediate,
        Overview = "在终端里启动一个耗时的命令后，终端就被占住了。bash 的作业控制可以把命令放到后台、暂停和恢复，nohup 与 setsid 则让命令在关闭终端后继续运行。需要长期保持的任务，更可靠的做法是 tmux 或 systemd-run。",

        Why =
        [
            "1. 前台与后台：",
            "- `命令 &`：在后台启动，终端立即可以继续输入；bash 显示作业号和 PID，如 `[1] 12345`。",
            "- `Ctrl+Z`：暂停当前前台命令（进程停止运行，但没有退出）。",
            "- `jobs`：列出当前终端的作业；`fg %1` 把 1 号作业调回前台；`bg %1` 让暂停的作业在后台继续运行。",
            "2. 关闭终端会发生什么：终端关闭时，shell 向它的作业发送 SIGHUP 信号，大多数程序收到后退出，所以普通的 `&` 后台任务会随终端一起结束。",
            "3. 让任务脱离终端：",
            "- `nohup 命令 &`：忽略 SIGHUP，输出写入 `nohup.out`。",
            "- `setsid 命令`：在新的会话中运行，与当前终端完全脱离。",
            "- `disown %1`：让 shell 不再管理这个作业，关闭终端时不向它发送 SIGHUP。",
            "4. 更好的选择：需要随时查看输出、以后接回来的任务用 tmux（见「tmux：会话保持与分屏」）；需要日志和资源限制的用 `systemd-run --user`。",
            "5. 从终端启动图形程序：直接运行会占住终端，程序的日志也会输出到终端；想启动后立即释放终端可用 `setsid 程序 >/dev/null 2>&1`。",
        ],

        Consequences =
        [
            "• 后台任务仍然会把输出打印到终端，可能打乱你正在输入的内容；把输出重定向到文件可避免。",
            "• 被 `Ctrl+Z` 暂停的程序不会自己恢复，忘记处理会一直占用内存；退出 shell 时 bash 会提示「有停止的作业」。",
            "• nohup、setsid 启动的程序不会随终端结束，需要时用 `pkill` 或 `kill` 手动结束。",
        ],

        Steps =
        [
            Step.Read(
                "在后台启动一个命令",
                "sleep 60 &",
                "`&` 放在命令末尾。").Then("立即输出 `[1] 12345`（作业号与 PID），然后返回提示符，可以继续输入其他命令。"),
            Step.Read(
                "查看当前终端的作业",
                "jobs -l",
                "`-l` 同时显示 PID。").Then("输出 `[1]+ 12345 运行中  sleep 60 &`。"),
            Step.Read(
                "暂停、后台继续、调回前台",
                "sleep 100",
                "运行后按 `Ctrl+Z` 暂停；再输入 `bg` 让它在后台继续；输入 `fg` 调回前台；最后按 `Ctrl+C` 结束。").Then("按 `Ctrl+Z` 后显示 `[2]+ 已停止  sleep 100`；`bg` 显示 `[2]+ sleep 100 &`；`fg` 后终端再次被占住，`Ctrl+C` 结束它。"),
            Step.Change(
                "关闭终端后仍继续运行：nohup",
                "nohup sh -c 'for i in 1 2 3; do date; sleep 10; done' > /tmp/nohup-demo.log 2>&1 &",
                "输出重定向到文件，否则写入当前目录的 `nohup.out`。").Then("显示作业号与 PID。此时关闭 Konsole 再重新打开，`cat /tmp/nohup-demo.log` 能看到每 10 秒写入的时间，说明任务没有随终端结束。"),
            Step.Change(
                "启动图形程序并立即释放终端",
                "setsid kate >/dev/null 2>&1 &",
                "以 Kate 编辑器为例。").Then("Kate 窗口打开，终端立即回到提示符，关闭这个终端也不会影响 Kate。"),
            Step.Change(
                "结束后台作业",
                "sleep 300 & jobs; kill %1; sleep 0.2; jobs",
                "`%1` 指当前终端的 1 号作业，只影响这个终端启动的任务；不要用 `pkill sleep`，它会结束你所有的 sleep 进程。").Then("第一次 `jobs` 显示 `[1]+ 运行中  sleep 300 &`；`kill %1` 后第二次 `jobs` 显示 `[1]+ 已终止  sleep 300`。"),
        ],

        Verify =
        [
            "1. 执行 `sleep 30 &` 后立即执行 `jobs`，能看到该作业；30 秒后再按一次回车，bash 提示 `[1]+ 已完成`。",
        ],
    };
}
