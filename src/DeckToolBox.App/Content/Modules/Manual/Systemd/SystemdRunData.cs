using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Systemd;

public static class SystemdRunData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-systemd-run",
        Title = "systemd-run：临时服务与一次性定时",
        Keywords = "--user · --unit · --scope · --on-active · --on-calendar · -p",
        Level = ModuleLevel.Intermediate,
        Overview = "systemd-run 不需要写单元文件，就能把任意命令作为一个临时服务运行：它在后台运行、日志进入 journal、可以用 systemctl 查看和停止，还能加上资源限制或定时触发。常用来代替 nohup 和 at。",

        Why =
        [
            "1. 两种运行方式：",
            "- 默认（transient service）：命令在后台作为服务运行，终端立即返回；输出进入 journal，用 `journalctl --user -u 单元名` 查看。",
            "- `--scope`：命令仍在前台运行、输出到终端，但被放进独立的 cgroup，可以施加资源限制（见「cgroups v2 资源控制」）。",
            "2. 常用选项：",
            "- `--user`：作为你的用户服务运行（不需要 root）。",
            "- `--unit=名称`：指定单元名，方便之后 `systemctl --user status 名称`。",
            "- `-p 属性=值`：设置单元属性，如 `MemoryMax=2G`、`CPUQuota=50%`、`Nice=10`。",
            "- `--on-active=30min`：30 分钟后运行一次；`--on-calendar='*-*-* 23:00'`：按日历时间运行（可重复）。",
            "- `--collect`：运行结束后即使失败也清理单元。",
            "3. 与 nohup 对比：systemd-run 启动的任务不依赖终端，关闭终端不受影响；它有名字、有日志、可以查看状态、可以统一停止，比 nohup 更可管理。",
            "4. 与 at 对比：SteamOS 没有 at 与 cron，`systemd-run --on-active` / `--on-calendar` 可以代替它们执行一次性或临时的定时任务。需要长期保留的定时任务请写成 timer 单元（见「定时任务：systemd timer」）。",
        ],

        Consequences =
        [
            "• 临时单元保存在 `/run`，重启后消失。",
            "• 用户服务在你注销后会被停止（`loginctl show-user deck -p Linger` 为 `no` 时）；在 Deck 上游戏模式与桌面模式切换属于同一登录，一般不受影响。",
            "• 定时任务执行的命令不在终端中运行，需要写完整路径，也看不到交互提示。",
        ],

        Steps =
        [
            Step.Change(
                "把一条命令作为后台服务运行",
                "systemd-run --user --unit=demo-task sh -c 'echo 来自临时服务; sleep 20'",
                "创建临时（transient）服务单元后终端立即脱离；任务交由 systemd 用户实例托管，终端退出不中断。").Then("输出 `Running as unit: demo-task.service; invocation ID: ...`，命令在后台运行。"),
            Step.Read(
                "查看它的状态与输出",
                "systemctl --user status demo-task --no-pager | head -n 5; journalctl --user -u demo-task --no-pager | tail -n 2",
                "单元名来自上一步的 `--unit`。").Then("status 显示 `Transient: yes`、`Active: active (running)`；日志中有 `来自临时服务` 这一行。"),
            Step.Change(
                "提前停止它",
                "systemctl --user stop demo-task",
                "与普通服务的停止方式相同。").Then("没有输出，任务被结束；再查 status 显示 `inactive (dead)` 或提示单元不存在。"),
            Step.Change(
                "10 分钟后执行一次",
                "systemd-run --user --on-active=10min --unit=reminder notify-send '休息一下' '已经过了 10 分钟'",
                "`--on-active=10min` 注册一次性单触发临时定时器单元，到期自动激活服务并销毁单元，代替传统 `at` 指令。").Then("输出 `Running timer as unit: reminder.timer`；10 分钟后桌面弹出通知。`systemctl --user list-timers` 中可以看到剩余时间。"),
            Step.Change(
                "每 15 分钟执行一次（直到重启）",
                "systemd-run --user --on-calendar='*:0/15' --unit=demo-timer sh -c 'date >> /tmp/every15.log'",
                "`*:0/15` 表示每小时的 0、15、30、45 分。").Then("输出定时器与服务的单元名；之后每到整 15 分钟，`/tmp/every15.log` 追加一行时间。"),
            Step.Change(
                "查看并取消临时定时器",
                "systemctl --user list-timers --no-pager | grep -E 'demo|reminder'; systemctl --user stop demo-timer.timer reminder.timer",
                "停止 timer 即取消。").Then("先列出两个定时器及下次触发时间，停止后它们从列表中消失。"),
        ],

        Verify =
        [
            "1. `systemctl --user list-units --type=service | grep run-` 可以看到所有未指定名称的临时服务（名称形如 `run-p12345-i12346.service`）。",
        ],
    };
}
