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
            "5. Steam Deck 上谁处理电源键：",
            "• `/etc/systemd/logind.conf.d/suspendbutton.conf` 设置 `HandlePowerKey=ignore`，logind 不直接响应电源键。",
            "• udev 规则 `70-steamos-power-button.rules` 给电源键的输入设备加上 `uaccess`，让登录用户的程序可以读取它。",
            "• 游戏模式中由用户服务 `steamos-powerbuttond` 读取按键，短按发送 `steam://shortpowerpress`、长按发送 `steam://longpowerpress` 给 Steam，由 Steam 决定休眠还是弹出电源菜单；桌面模式中这个服务不运行，由 KDE 的电源管理 (PowerDevil) 按自己的设置处理。",
            "6. 休眠到内存与休眠到硬盘：`/sys/power/mem_sleep` 中带方括号的是当前的挂起方式，Deck 上为 `[deep]`（S3）。休眠到硬盘 (hibernate) 的准备工作都在：`hibernate-prepare.service` 会在休眠前用 `hibernate-swap-helper.sh` 在 `/home` 建临时交换文件，并把它的位置写入 `/sys/power/resume` 与 `resume_offset`，唤醒后再删除。但 `/usr/lib/systemd/sleep.conf.d/steamos-suspend-then-hibernate.conf` 默认设置了 `AllowHibernation=no`，所以默认不会休眠到硬盘。",
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
                "查看电源键接管与休眠配置",
                "cat /etc/systemd/logind.conf.d/suspendbutton.conf; cat /usr/lib/udev/rules.d/70-steamos-power-button.rules",
                "两个文件都很短。").Then("先输出 `[Login]` 与 `HandlePowerKey=ignore`；再输出一行 udev 规则：匹配 `ENV{STEAMOS_POWER_BUTTON}==\"1\"` 的输入设备，`MODE=\"0660\"`、`TAG+=\"uaccess\"`。"),
            Step.Read(
                "查看当前有哪些程序在阻止或推迟休眠",
                "systemd-inhibit --list --no-pager",
                "`block` 阻止，`delay` 只推迟片刻。").Then("表格：`WHO`（程序）、`UID`、`USER`、`PID`、`WHAT`（被抑制的操作）、`WHY`（原因）、`MODE`。通常能看到 NetworkManager、UPower 等的 `delay` 锁。"),
            Step.Change(
                "运行一个命令，期间禁止系统休眠",
                "systemd-inhibit --what=sleep:idle --why='备份中' <命令>",
                "例如把 rsync 备份命令放在最后。").Then("命令照常运行并输出；运行期间即使长时间不操作或按电源键，系统也不会休眠。命令结束后锁自动释放。"),
            Step.Read(
                "查看电池耗电功率与剩余时间",
                "upower -i $(upower -e | grep BAT) | grep -E 'state|percentage|energy-rate|time to'",
                "`energy-rate` 是当前功率（瓦），拔掉电源时才有意义。").Then("输出几行：`state: discharging`、`energy-rate: 9.8 W`、`percentage: 76%`、`time to empty: 3.1 hours`；插电时 state 为 `charging` 或 `pending-charge`。"),
            Step.Change(
                "一小时后自动关机",
                "systemd-run --user --on-active=1h --unit=auto-poweroff systemctl poweroff",
                "取消：`systemctl --user stop auto-poweroff.timer`。").Then("输出 `Running timer as unit: auto-poweroff.timer`，一小时后系统关机。"),
            Step.Change(
                "立即休眠",
                "systemctl suspend",
                "休眠期间仍会缓慢耗电。").Then("屏幕熄灭，Deck 进入睡眠（电源灯熄灭或闪烁），按电源键唤醒后回到原来的界面。"),
            Step.Change(
                "重启 / 关机",
                "systemctl reboot",
                "关机用 `systemctl poweroff`。").Then("所有程序被关闭，系统立即重启。"),
            Step.Read(
                "查看挂起方式与休眠到硬盘的设置",
                "cat /sys/power/mem_sleep /sys/power/resume /sys/power/resume_offset; grep -v '^#' /usr/lib/systemd/sleep.conf.d/*.conf",
                "`resume` 与 `resume_offset` 只在休眠到硬盘的过程中才会被临时写入。").Then("依次输出 `s2idle [deep]`（当前用 deep）、`0:0`、`0`（现在没有准备休眠文件），最后是 `[Sleep]`、`AllowHibernation=no`、`AllowSuspendThenHibernate=no` 等几行。"),
        ],

        Verify =
        [
            "1. 在另一个终端运行 `systemd-inhibit --what=sleep:idle --why=测试 sleep 60`，然后执行第 1 步，列表中能看到「测试」这一项。",
            "2. 设置定时关机后，`systemctl --user list-timers` 中出现 `auto-poweroff.timer` 及剩余时间。",
        ],
    };
}
