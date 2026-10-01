using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Systemd;

public static class LoginctlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-loginctl",
        Title = "loginctl：会话、座席与 linger",
        Keywords = "list-sessions · show-session · lock-session · enable-linger",
        Level = ModuleLevel.Intermediate,
        Overview = "systemd-logind 管理用户登录会话：谁登录了、是图形还是 SSH、屏幕是否锁定、用户服务是否在注销后继续运行。loginctl 是它的命令行工具，锁屏、查看会话类型、让用户服务开机即运行都用它。",

        Why =
        [
            "1. 会话 (session)：每次登录（图形桌面、SSH、tty）都是一个会话，有编号、所属用户、类型 (`wayland`、`x11`、`tty`) 和是否活动。Deck 上通常有一个 `tty1` 的图形会话，以及一个 `manager` 类会话（用户的 systemd 实例）。",
            "2. 座席 (seat)：一组显示器、键盘、鼠标。单用户设备只有 `seat0`。设备的 `uaccess` 权限就是授予 seat0 上活动会话的用户。",
            "3. linger：",
            "• 默认情况下，用户的 systemd 实例在该用户最后一个会话结束时停止，用户服务随之停止。",
            "• `loginctl enable-linger` 让你的用户服务在开机时就启动，且注销后继续运行，适合常驻的同步、下载类用户服务。",
            "4. 常用操作：`lock-session`（锁屏）、`terminate-session 编号`（结束一个会话）、`show-user 用户`（查看用户属性）。",
        ],

        Consequences =
        [
            "• 查看类命令只读。",
            "• `terminate-session` 会结束该会话中的所有程序；对当前图形会话执行等于注销。",
            "• 开启 linger 后，你的用户服务在无人登录时也会运行，注意它们的资源占用。关闭：`loginctl disable-linger`。",
        ],

        Steps =
        [
            Step.Read(
                "列出当前所有会话",
                "loginctl list-sessions --no-pager",
                "`CLASS` 为 user 的是你实际的登录。").Then("表格列出会话编号、UID、用户、座席、TTY 等，如 `3 1000 deck seat0 3186 user tty1`，最后一行统计会话数量。"),
            Step.Read(
                "查看当前图形会话的属性",
                "loginctl show-session $(loginctl list-sessions --no-legend | awk '$6==\"user\"{print $1; exit}') -p Type -p Remote -p Active -p State",
                "先找出 CLASS 为 user 的会话编号，再读取属性。").Then("输出 `Type=wayland`（桌面模式）、`Remote=no`、`Active=yes`、`State=active`。"),
            Step.Read(
                "查看用户是否开启 linger",
                "loginctl show-user \"$USER\" -p Linger -p State",
                "查询 systemd-logind 状态；Linger 设为 yes 时允许用户注销后其后台 `user@.service` 服务保持常驻。").Then("输出 `Linger=no` 与 `State=active`。"),
            Step.Change(
                "锁屏",
                "loginctl lock-session",
                "向会话管理器发送 Lock 信号；由屏幕锁程序（如 kscreenlocker）接管输入以保护当前桌面会话。").Then("屏幕立即显示锁屏界面，输入密码解锁。"),
            Step.Change(
                "让用户服务开机即运行、注销后不停止",
                "loginctl enable-linger",
                "可能需要输入密码授权；关闭用 `loginctl disable-linger`。").Then("没有输出，`show-user` 中 `Linger` 变为 `yes`，`/var/lib/systemd/linger/deck` 文件被创建。"),
        ],

        Verify =
        [
            "1. 通过 SSH 登录后执行 `loginctl list-sessions`，会多出一个 TTY 为 `pts/N`、`Remote=yes` 的会话。",
        ],
    };
}
