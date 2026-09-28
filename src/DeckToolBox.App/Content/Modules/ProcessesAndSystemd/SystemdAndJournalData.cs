using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.ProcessesAndSystemd;

public static class SystemdAndJournalData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "systemd-journal",
        Title = "systemd 服务与日志",
        Keywords = "systemctl · journalctl · --user",
        Level = ModuleLevel.Intermediate,
        Overview = "systemd 负责启动和管理系统里的所有服务，journald 收集所有日志。学会查看服务状态、读日志，以及不用 root 编写自己的用户级服务。",

        Why =
        [
            "1. 为什么 Systemd 能统治现代 Linux？",
            "• 强大的 cgroups 进程控制组：传统 init 脚本无法跟踪通过 double-fork 脱壳逃逸的守护进程。Systemd 将每个服务置入独立的 cgroup，服务停止时能瞬间精确收割整个进程树，绝无孤儿残留。",
            "• 并行启动与 Socket 激活：根据服务依赖图有向无环图 (DAG) 全力并行拉起，极大压缩掌机开机耗时。",
            "2. 核心单元类型 (Unit Types)：",
            "• .service：后台服务（控制启动、重启策略 Restart=on-failure）。",
            "• .timer：高精度定时器（彻底淘汰过时的 crontab，支持唤醒睡眠硬件、定时错峰执行）。",
            "• .target：逻辑同步聚合点（如 graphical.target 代表图形界面已就绪）。",
            "3. 极其重要的用户级服务体系 (systemd --user)：",
            "• 任何用户无需 root 提权，即可在 ~/.config/systemd/user/ 编写专属服务，实现自定义后端脚本开机自启、崩溃自动重启、生命周期随桌面会话绑定！",
        ],

        Consequences =
        [
            "• 用户服务隔离安全：systemd --user 严格继承普通用户权限，绝无破坏系统只读根分区的风险。",
            "• 单元文件编写规范：每次在 ~/.config/systemd/user/ 新增或修改 .service 文件后，必须执行 systemctl --user daemon-reload 重新加载内存索引，否则修改不会生效。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统当前启动失败的所有故障单元",
                "systemctl --failed",
                "列出所有处于 failed 状态的系统单元。").Then("正常时显示 `0 loaded units listed.`；有失败时每行列出单元名与 `failed` 状态。"),
            Step.Read(
                "查看当前用户专属运行的 systemd 用户服务清单",
                "systemctl --user list-units --type=service --state=running",
                "`--user` 查看用户级服务，`--state=running` 只显示运行中的。").Then("表格列出正在运行的用户服务，如 `pipewire.service`、`wireplumber.service`、`xdg-desktop-portal.service`，末尾统计总数。"),
            Step.Change(
                "编写一个标准的用户级开机自启服务模板 (~/.config/systemd/user/)",
                "mkdir -p ~/.config/systemd/user\ncat <<'EOF' > ~/.config/systemd/user/my-bg-tool.service\n[Unit]\nDescription=我的自定义后台工具\nAfter=network.target\n\n[Service]\nType=simple\nExecStart=/home/deck/Applications/MyTool/start.sh\nRestart=on-failure\nRestartSec=5s\n\n[Install]\nWantedBy=default.target\nEOF\nsystemctl --user daemon-reload",
                "`Restart=on-failure` 让程序崩溃后 5 秒自动重启。把 `ExecStart` 换成你自己程序的绝对路径。").Then("没有输出。生成 `~/.config/systemd/user/my-bg-tool.service` 并让 systemd 重新加载配置，但服务尚未启动；执行 `systemctl --user enable --now my-bg-tool` 后才会启动并开机自启。"),
            Step.Read(
                "使用 journalctl 查看当前开机周期内所有的错误级别日志",
                "journalctl -p err -b 0 | head -n 30",
                "`-p err` 只看错误及更严重的级别，`-b 0` 表示本次开机。").Then("每行一条错误日志：时间、主机名、来源进程[PID]、消息内容，显示前 30 条。"),
        ],

        Verify =
        [
            "执行 systemctl --user status my-bg-tool.service，即可看到清晰的状态绿灯 (active running)，以及由 journald 实时收集的最近几行控制台输出。",
        ],
    };
}
