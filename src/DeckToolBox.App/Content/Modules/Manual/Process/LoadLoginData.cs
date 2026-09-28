using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Process;

public static class LoadLoginData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-load-login",
        Title = "uptime、vmstat、w、last",
        Keywords = "负载 · 内存换页 · 登录记录",
        Level = ModuleLevel.Intermediate,
        Overview = "uptime 一行给出开机时长和平均负载；vmstat 每隔几秒报告一次 CPU、内存、交换、IO 的变化，适合观察系统是否在频繁换页或等待磁盘；w、who、last 显示谁登录了、从哪里登录，开启 SSH 后可以用来检查登录记录。",

        Why =
        [
            "1. uptime：当前时间、已运行时长、登录用户数、1/5/15 分钟平均负载；`-p` 只显示易读的运行时长。",
            "2. vmstat 的主要列（`vmstat 2 5` 每 2 秒一次，共 5 次）：",
            "- `r`：等待 CPU 的进程数；`b`：处于不可中断等待（多为磁盘 IO）的进程数。",
            "- `swpd`、`free`、`buff`、`cache`：交换已用量与内存分布（KB）。",
            "- `si`、`so`：每秒从交换空间换入、换出的量。**持续不为 0 说明内存不足**，系统在频繁换页。",
            "- `bi`、`bo`：每秒从磁盘读入、写出的块数。",
            "- `us`、`sy`、`id`、`wa`：CPU 用于用户程序、内核、空闲、等待 IO 的百分比。",
            "• 第一行是开机以来的平均值，从第二行开始才是每个间隔内的实时数据。",
            "3. w 与 who：当前登录的会话。Deck 上通常只有 `tty1`（图形会话）和若干 `pts/N`（终端窗口）；通过 SSH 登录的会话会显示来源 IP。",
            "4. last：历次登录、注销与重启记录（来自 `/var/log/wtmp`），`last -n 10` 最近 10 条，`last reboot` 只看重启。",
            "5. lastlog：每个账户最后一次登录的时间与来源。",
        ],

        Consequences =
        [
            "• 这些命令都只读。",
            "• 开启 sshd 后，定期用 `last` 检查是否有陌生 IP 的登录记录；发现异常时修改密码并检查 `~/.ssh/authorized_keys`。",
        ],

        Steps =
        [
            Step.Read(
                "查看运行时长与平均负载",
                "uptime; uptime -p",
                "`-p` 只输出运行时长。").Then("第一行如 `20:23:37 up 5 days, 3:56, 1 user, load average: 3.26, 4.71, 3.43`，第二行 `up 5 days, 3 hours, 56 minutes`。"),
            Step.Read(
                "每 2 秒采样一次系统状态",
                "vmstat 2 5",
                "共输出 5 行数据后自动结束。").Then("两行表头之后是 5 行数据。观察 `si`/`so` 是否持续非零（内存不足在换页）、`wa` 是否偏高（磁盘是瓶颈）。第一行是开机以来的平均值。"),
            Step.Read(
                "以 MB 为单位并带时间戳",
                "vmstat -S M -t 2 3",
                "`-S M` 内存单位为 MB，`-t` 每行附加时间。").Then("内存相关列以 MB 显示，每行末尾是采样时间。"),
            Step.Read(
                "查看当前登录的会话",
                "w; echo; who",
                "w 还显示每个会话在运行什么。").Then("`w` 的第一行与 uptime 相同，随后是 `deck tty1 ... /usr/bin/startplasma-wayland` 等；`who` 列出每个终端及登录时间，SSH 会话带有来源 IP。"),
            Step.Read(
                "查看最近的登录与重启记录",
                "last -n 10; last reboot | head -n 3",
                "`still logged in` 表示仍在登录。").Then("每行一条记录：用户、终端、来源、开始与结束时间、持续时长；第二部分是最近三次重启的时间。"),
        ],

        Verify =
        [
            "1. 用另一台电脑 SSH 登录 Deck 后执行 `who`，会多出一行 `pts/N` 并显示那台电脑的 IP。",
        ],
    };
}
