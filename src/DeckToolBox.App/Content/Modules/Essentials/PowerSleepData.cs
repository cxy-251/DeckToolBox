using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class PowerSleepData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "power-sleep",
        Title = "电源、休眠与防止休眠",
        Keywords = "systemctl suspend · systemd-inhibit · upower · 定时关机",
        Level = ModuleLevel.Basic,
        Overview = "从终端休眠、重启、关机；下载或备份时防止系统自动休眠；查看电池与耗电情况；定时关机。这些都由 systemd-logind 与 UPower 提供，所有 systemd 发行版通用。",

        Why =
        [
            "1. 电源操作：`systemctl suspend`（休眠到内存）、`reboot`、`poweroff`。普通用户在本地会话中可以直接执行，不需要 sudo。",
            "2. 抑制锁 (inhibitor)：程序可以向 logind 申请「暂时不要休眠 / 关机」。`systemd-inhibit` 让任意命令在运行期间持有这样的锁，命令结束后自动释放。长时间下载、备份、编码视频时很有用。",
            "• 锁有两种模式：`block` 阻止操作；`delay` 只是推迟一小会儿，让程序完成收尾工作（例如 NetworkManager 在休眠前断开网络）。",
            "3. UPower：汇总电池、电源适配器和蓝牙外设电量；`upower -i` 显示当前耗电功率和预计剩余时间。",
            "4. 定时操作：`systemd-run --on-active=<时长>` 可以在一段时间后执行任意命令，例如 1 小时后关机，不需要 cron 或 at（SteamOS 两者都没有）。",
        ],

        Consequences =
        [
            "• 查看抑制锁、电池信息是只读的。",
            "• `suspend` / `reboot` / `poweroff` 立即执行，未保存的内容会丢失。",
            "• `systemd-inhibit` 只在它包裹的命令运行期间生效；命令结束或被 `Ctrl+C` 中断，锁即释放。",
            "• 通过 `systemd-run --user --on-active` 创建的定时任务，可以用 `systemctl --user list-timers` 查看、`systemctl --user stop <单元名>.timer` 取消。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前有哪些程序在阻止或推迟休眠",
                "systemd-inhibit --list --no-pager",
                "`WHAT` 列是被抑制的操作，`WHY` 是原因，`MODE` 为 block 或 delay。"),
            Step.Change(
                "运行一个命令，期间禁止系统休眠",
                "systemd-inhibit --what=sleep:idle --why='备份中' <命令>",
                "例如把 rsync 备份命令放在最后。命令结束后恢复正常休眠。"),
            Step.Read(
                "查看电池耗电功率与剩余时间",
                "upower -i $(upower -e | grep BAT) | grep -E 'state|percentage|energy-rate|time to'",
                "`energy-rate` 是当前功率（瓦），拔掉电源时才有意义。"),
            Step.Change(
                "一小时后自动关机",
                "systemd-run --user --on-active=1h --unit=auto-poweroff systemctl poweroff",
                "取消：`systemctl --user stop auto-poweroff.timer`。"),
            Step.Change(
                "立即休眠",
                "systemctl suspend",
                "按电源键唤醒。"),
            Step.Change(
                "重启 / 关机",
                "systemctl reboot",
                "关机用 `systemctl poweroff`。"),
        ],

        Verify =
        [
            "1. 在另一个终端运行 `systemd-inhibit --what=sleep:idle --why=测试 sleep 60`，然后执行第 1 步，列表中能看到「测试」这一项。",
            "2. 设置定时关机后，`systemctl --user list-timers` 中出现 `auto-poweroff.timer` 及剩余时间。",
        ],
    };
}
