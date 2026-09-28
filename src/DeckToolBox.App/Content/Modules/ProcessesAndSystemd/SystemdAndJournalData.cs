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
            "1. systemd 的职责：作为 1 号进程启动整个系统，按依赖关系并行启动各项服务，监控它们，在服务退出时按策略重启。每个服务运行在独立的 cgroup 中，停止服务时它派生的所有子进程都会被一并结束。",
            "2. 单元 (unit) 类型：",
            "- `.service`：服务，定义怎样启动一个程序、失败时是否重启。",
            "- `.timer`：定时器，按时间触发某个 service（见「定时任务：systemd timer」）。",
            "- `.target`：一组单元的同步点，例如 `graphical.target` 表示图形界面所需的服务都已就绪。",
            "- `.mount`、`.socket`、`.path` 等：挂载点、套接字激活、路径监视。",
            "3. 系统实例与用户实例：`systemctl` 管理系统服务（需要 root 才能修改）；`systemctl --user` 管理你自己的用户服务，单元文件放在 `~/.config/systemd/user/`，无需 root，随你登录启动、注销停止。",
            "4. journald：收集内核、服务和用户程序的输出，以带索引的二进制格式保存在 `/var/log/journal`，用 `journalctl` 按时间、服务、级别过滤查询。",
        ],

        Consequences =
        [
            "• `systemctl status`、`list-units`、`journalctl` 都是只读的。",
            "• 用户服务以你的身份运行，不能修改系统文件，也不会影响只读根分区。",
            "• 新增或修改单元文件后必须执行 `systemctl --user daemon-reload`（系统服务用 `sudo systemctl daemon-reload`），否则 systemd 仍使用旧配置。",
            "• `enable` 让服务开机（或登录时）自动启动，`start` 只启动这一次；`enable --now` 两者都做。",
        ],

        Steps =
        [
            Step.Read(
                "列出启动失败的系统单元",
                "systemctl --failed",
                "列出所有处于 failed 状态的系统单元。").Then("正常时显示 `0 loaded units listed.`；有失败时每行列出单元名与 `failed` 状态。"),
            Step.Read(
                "列出正在运行的用户服务",
                "systemctl --user list-units --type=service --state=running",
                "`--user` 查看用户级服务，`--state=running` 只显示运行中的。").Then("表格列出正在运行的用户服务，如 `pipewire.service`、`wireplumber.service`、`xdg-desktop-portal.service`，末尾统计总数。"),
            Step.Change(
                "编写一个用户级服务",
                "mkdir -p ~/.config/systemd/user\ncat <<'EOF' > ~/.config/systemd/user/my-bg-tool.service\n[Unit]\nDescription=我的自定义后台工具\nAfter=network.target\n\n[Service]\nType=simple\nExecStart=/home/deck/Applications/MyTool/start.sh\nRestart=on-failure\nRestartSec=5s\n\n[Install]\nWantedBy=default.target\nEOF\nsystemctl --user daemon-reload",
                "`Restart=on-failure` 让程序崩溃后 5 秒自动重启。把 `ExecStart` 换成你自己程序的绝对路径。").Then("没有输出。生成 `~/.config/systemd/user/my-bg-tool.service` 并让 systemd 重新加载配置，但服务尚未启动；执行 `systemctl --user enable --now my-bg-tool` 后才会启动并开机自启。"),
            Step.Read(
                "查看本次开机的错误日志",
                "journalctl -p err -b 0 | head -n 30",
                "`-p err` 只看错误及更严重的级别，`-b 0` 表示本次开机。").Then("每行一条错误日志：时间、主机名、来源进程[PID]、消息内容，显示前 30 条。"),
        ],

        Verify =
        [
            "1. 启用示例服务后，`systemctl --user status my-bg-tool.service` 显示 `active (running)`，下方有它最近的输出日志。",
            "2. `journalctl --user -u my-bg-tool.service -f` 能实时看到该服务的输出。",
        ],
    };
}
