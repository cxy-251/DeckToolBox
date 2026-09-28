using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class TroubleshootingData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "troubleshooting",
        Title = "排查问题的通用流程",
        Keywords = "journalctl · systemctl --failed · coredumpctl · 日志",
        Level = ModuleLevel.Basic,
        Overview = "不论是服务启动失败、程序闪退还是硬件没反应，排查的套路都一样：先看系统层有没有报错，再看具体程序的日志，最后看崩溃记录。学会这几条命令，搜索和提问时也能给出有用的信息。",

        Why =
        [
            "1. 系统日志集中在 journald：内核、系统服务、用户服务的输出都在这里，用 `journalctl` 查询。",
            "- `-b`：本次开机；`-b -1`：上一次开机（排查「上次关机前出了什么事」）。",
            "- `-p err`：只看错误及更严重的级别；`-k`：只看内核消息。",
            "- `-u <服务>`：某个系统服务；`--user -u <服务>`：某个用户服务。",
            "- `-f`：持续跟踪新日志，一边复现问题一边观察。",
            "2. 服务状态：`systemctl --failed` 列出启动失败的系统服务，`systemctl status <服务>` 同时显示状态和最近几行日志。",
            "3. 程序崩溃：程序因段错误等原因崩溃时，systemd-coredump 会记录崩溃信息，用 `coredumpctl` 查看是哪个程序、什么信号、何时崩溃。",
            "4. 程序自己的日志：",
            "- Steam：`~/.local/share/Steam/logs/`。",
            "- Proton 游戏：在启动选项加 `PROTON_LOG=1`，日志在 `~/steam-<AppID>.log`（见「Wine 与 Proton 原理」）。",
            "- 图形程序：从终端启动它，终端里的输出往往直接说明了问题。",
            "5. 提问时附上：发行版与版本（`cat /etc/os-release`）、内核版本（`uname -r`）、完整的报错文字和相关日志片段。",
        ],

        Consequences =
        [
            "• 本模块的命令全部只读。",
            "• 普通用户可能看不到部分系统服务的日志，必要时加 `sudo`。",
            "• 公开粘贴日志前检查一遍，去掉用户名、IP、序列号等隐私信息。",
        ],

        Steps =
        [
            Step.Read(
                "列出启动失败的服务",
                "systemctl --failed --no-pager; systemctl --user --failed --no-pager",
                "分别检查系统服务和用户服务。`0 loaded units listed` 表示没有失败的服务。"),
            Step.Read(
                "查看本次开机以来的错误日志",
                "journalctl -b -p err --no-pager | tail -n 40",
                "只显示 err 及以上级别，最后 40 条。"),
            Step.Read(
                "查看上一次开机的最后日志",
                "journalctl -b -1 -n 50 --no-pager",
                "排查死机、意外重启时，看关机前最后发生了什么。提示「No journal boot entry found」时，先用 `journalctl --list-boots` 看保留了哪些开机记录；系统日志需加 `sudo` 才能完整查看。"),
            Step.Read(
                "一边复现问题一边观察日志",
                "journalctl -f",
                "执行后去操作出问题的程序或插拔设备，新日志会实时滚动出现。`Ctrl+C` 退出。"),
            Step.Read(
                "查看最近的程序崩溃记录",
                "coredumpctl list --no-pager | tail -n 10",
                "`SIG` 列为导致崩溃的信号（常见 SIGSEGV 段错误、SIGABRT 主动中止）。查看某次详情：`coredumpctl info <PID>`。"),
            Step.Read(
                "查看 Steam 客户端的日志",
                "ls -lt ~/.local/share/Steam/logs/ | head; tail -n 50 ~/.local/share/Steam/logs/console_log.txt",
                "`console_log.txt` 记录了 Steam 客户端的主要活动与报错。"),
            Step.Read(
                "收集提问时需要的系统信息",
                "grep PRETTY_NAME /etc/os-release; grep -E '^(VERSION_ID|BUILD_ID)=' /etc/os-release; uname -r",
                "把输出连同报错一起附上。"),
        ],

        Verify =
        [
            "1. `systemctl --failed` 显示 `0 loaded units listed`，说明所有服务都正常。",
            "2. 你能根据日志中的时间戳，把错误与自己操作的时间点对应起来。",
        ],
    };
}
