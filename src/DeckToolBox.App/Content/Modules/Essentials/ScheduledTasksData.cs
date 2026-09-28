using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class ScheduledTasksData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "scheduled-tasks",
        Title = "定时任务：systemd timer",
        Keywords = "systemctl --user · .timer · OnCalendar",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 没有安装 cron（crontab 命令不存在）。定时执行脚本的标准做法是 systemd 用户定时器：一个 .service 描述做什么，一个 .timer 描述什么时候做，无需 root。",

        Why =
        [
            "1. 两个文件，放在 `~/.config/systemd/user/`：",
            "- `名称.service`：要执行的命令（`ExecStart=`）。定时任务通常用 `Type=oneshot`，执行完即结束。",
            "- `名称.timer`：触发时间，启用的是 timer 而不是 service。",
            "2. 常用的时间写法：",
            "- `OnCalendar=daily`：每天 0 点；`weekly`、`hourly` 同理。",
            "- `OnCalendar=*-*-* 03:30:00`：每天 3:30；`OnCalendar=Mon *-*-* 09:00`：每周一 9 点。",
            "- `OnBootSec=10min`：开机 10 分钟后；`OnUnitActiveSec=6h`：上次执行后每隔 6 小时。",
            "- `Persistent=true`：错过的执行（例如 Deck 当时在休眠）会在下次开机后补上。",
            "3. 与 cron 相比：日志自动进入 journald，可以用 `systemctl --user status` 查看上次执行结果，也能手动立即触发一次来测试。",
            "4. 用户服务在你登录桌面或游戏模式时运行；注销后停止。",
        ],

        Consequences =
        [
            "• 用户定时器只能做你自己有权限做的事，不会影响系统。",
            "• 修改 `.service` / `.timer` 文件后必须执行 `systemctl --user daemon-reload`，否则改动不生效。",
            "• 脚本里要使用绝对路径；定时器环境的 `PATH` 可能与你的终端不同。",
            "• 删除定时任务：先 `disable --now` 定时器，再删除两个文件并 `daemon-reload`。",
        ],

        Steps =
        [
            Step.Read(
                "查看现有的定时器",
                "systemctl --user list-timers --all --no-pager; systemctl list-timers --no-pager | head",
                "第一条是你的用户定时器，第二条是系统定时器（可以参考它们的写法）。"),
            Step.Change(
                "创建一个示例任务：每天备份文档目录",
                "mkdir -p ~/.config/systemd/user && cat > ~/.config/systemd/user/backup-docs.service <<'EOF'\n[Unit]\nDescription=每日备份 Documents\n\n[Service]\nType=oneshot\nExecStart=/usr/bin/rsync -a %h/Documents/ %h/Backups/Documents/\nEOF",
                "`%h` 是 systemd 对家目录的写法。可以把 `ExecStart` 换成你自己脚本的绝对路径。"),
            Step.Change(
                "创建对应的定时器",
                "cat > ~/.config/systemd/user/backup-docs.timer <<'EOF'\n[Unit]\nDescription=每天 21:00 备份 Documents\n\n[Timer]\nOnCalendar=*-*-* 21:00:00\nPersistent=true\n\n[Install]\nWantedBy=timers.target\nEOF",
                "文件名主体必须与 .service 相同，timer 才知道要触发哪个服务。"),
            Step.Read(
                "检查时间表达式的含义",
                "systemd-analyze calendar '*-*-* 21:00:00'",
                "显示规范化写法和下一次触发时间，写复杂表达式时很有用。"),
            Step.Change(
                "加载并启用定时器",
                "mkdir -p ~/Backups/Documents && systemctl --user daemon-reload && systemctl --user enable --now backup-docs.timer",
                "先创建备份目标目录。"),
            Step.Change(
                "立即手动执行一次任务来测试",
                "systemctl --user start backup-docs.service && systemctl --user status backup-docs.service --no-pager",
                "`status` 中显示 `status=0/SUCCESS` 表示成功。"),
            Step.Change(
                "停用并删除这个定时任务",
                "systemctl --user disable --now backup-docs.timer && rm ~/.config/systemd/user/backup-docs.{service,timer} && systemctl --user daemon-reload",
                "备份出来的 `~/Backups` 目录不会被删除。"),
        ],

        Verify =
        [
            "1. `systemctl --user list-timers` 中出现 `backup-docs.timer`，`NEXT` 列显示下一次执行时间。",
            "2. 手动执行后 `ls ~/Backups/Documents` 能看到文件；`journalctl --user -u backup-docs.service` 有执行记录。",
        ],
    };
}
