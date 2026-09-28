using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.ProcessesAndSystemd;

public static class DbusInterProcessData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "dbus",
        Title = "D-Bus 消息总线",
        Keywords = "busctl · session/system bus",
        Level = ModuleLevel.Advanced,
        Overview = "D-Bus 是 Linux 桌面上进程之间通信的消息总线：网络状态变化、U 盘插入、媒体播放控制、桌面通知都经由它传递。用 busctl 可以查看总线上有哪些服务、它们提供什么接口，并直接调用。",

        Why =
        [
            "1. 为什么需要消息总线：管道、套接字适合两个进程之间点对点通信；而「电池电量变化」「网络断开」这类事件需要通知多个关心它的程序，「让播放器暂停」需要一种统一的调用方式。D-Bus 提供了广播（信号）和远程调用（方法）两种机制。",
            "2. 两条总线：",
            "- 系统总线：全机一条，系统服务在这里提供接口，如 NetworkManager、UPower、UDisks2、steamos-manager。",
            "- 会话总线：每个登录用户一条，桌面组件在这里通信，如通知服务、KWin、媒体播放器（MPRIS 接口）。",
            "3. 寻址方式：",
            "- 服务名：反向域名，如 `org.freedesktop.Notifications`。",
            "- 对象路径：类似文件路径，如 `/org/freedesktop/Notifications`。",
            "- 接口与成员：接口名下有方法（Method，可调用）、属性（Property，可读写）和信号（Signal，由服务主动发出）。",
            "4. 工具：systemd 自带 `busctl`；KDE 自带 `qdbus6`，写法更简短。",
        ],

        Consequences =
        [
            "• `list`、`introspect`、读取属性都是只读的。",
            "• 调用方法等同于让服务执行对应操作，效果与在界面上点按钮相同。调用系统服务上会修改状态的方法时，通常会触发 polkit 授权确认。",
            "• 脚本中监听 D-Bus 信号（`busctl monitor`、`dbus-monitor`）可以在事件发生时立即响应，比定时轮询更及时、更省资源。",
        ],

        Steps =
        [
            Step.Read(
                "列出会话总线上的连接",
                "busctl --user list --no-pager | sed -n '1,25p'",
                "会话总线上是桌面组件与应用。").Then("表格列出前 25 个连接：总线名（`:1.23` 或 `org.kde.xxx`）、PID、进程名、用户等。"),
            Step.Read(
                "查看一个服务对象提供的接口",
                "busctl --system introspect org.freedesktop.UPower /org/freedesktop/UPower",
                "`introspect` 让服务报告自己提供了哪些方法、属性与信号。").Then("按接口分组列出 `.EnumerateDevices`（方法）、`.OnBattery`（属性，带当前值）、`.DeviceAdded`（信号）等成员。"),
            Step.Change(
                "直接调用 D-Bus 方法：向桌面发一条通知",
                "busctl --user call org.freedesktop.Notifications /org/freedesktop/Notifications org.freedesktop.Notifications Notify susssasa{sv}i DeckToolBox 0 '' 'D-Bus 测试' '这条通知是直接调用总线方法发出的' 0 0 5000",
                "签名 `susssasa{sv}i` 依次对应：应用名、替换 ID、图标、标题、正文、动作数组（`0` 表示空数组）、提示字典（`0` 表示空）、超时毫秒。`notify-send` 做的就是同一次调用。").Then("桌面右下角弹出一条标题为「D-Bus 测试」的通知，5 秒后消失；终端输出 `u 17` 这样的返回值（通知 ID）。"),
        ],

        Verify =
        [
            "1. 执行通知示例后，桌面弹出通知，终端输出 `u <数字>`（新通知的 ID）。",
            "2. `busctl --system get-property org.freedesktop.UPower /org/freedesktop/UPower org.freedesktop.UPower OnBattery` 输出 `b false`（插着电源）或 `b true`（使用电池）。",
        ],
    };
}
