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
        Overview = "掌握 Linux 统治级初始化系统 (systemd)，免 root 编写持久化用户级服务 (systemd --user)，使用 journald 毫秒排查系统故障。",

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
                "开机自检利器，快速定位有没有拉起失败的后台服务。"),
            Step.Read(
                "查看当前用户专属运行的 systemd 用户服务清单",
                "systemctl --user list-units --type=service --state=running",
                "确认 PipeWire 音频、Portal 桌面传送门等用户服务的健康状态。"),
            Step.Change(
                "编写一个标准的用户级开机自启服务模板 (~/.config/systemd/user/)",
                "mkdir -p ~/.config/systemd/user\ncat <<'EOF' > ~/.config/systemd/user/my-bg-tool.service\n[Unit]\nDescription=我的自定义后台工具\nAfter=network.target\n\n[Service]\nType=simple\nExecStart=/home/deck/Applications/MyTool/start.sh\nRestart=on-failure\nRestartSec=5s\n\n[Install]\nWantedBy=default.target\nEOF\nsystemctl --user daemon-reload",
                "标准的用户守护进程编写模版，自动崩溃重启。"),
            Step.Read(
                "使用 journalctl 查看当前开机周期内所有的错误级别日志",
                "journalctl -p err -b 0 | head -n 30",
                "直接从内存二进制日志环中精准过滤内核与服务抛出的错误。"),
        ],

        Verify =
        [
            "执行 systemctl --user status my-bg-tool.service，即可看到清晰的状态绿灯 (active running)，以及由 journald 实时收集的最近几行控制台输出。",
        ],
    };
}
