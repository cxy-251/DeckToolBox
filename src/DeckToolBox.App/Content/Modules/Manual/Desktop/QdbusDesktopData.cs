using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Desktop;

public static class QdbusDesktopData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-qdbus-desktop",
        Title = "qdbus6：剪贴板与桌面控制",
        Keywords = "qdbus6 · klipper · 剪贴板 · KWin · 显示桌面 · 屏保",
        Level = ModuleLevel.Intermediate,
        Overview = "KDE 桌面的各个组件都在会话总线上提供了接口，qdbus6 可以直接调用它们。最实用的例子：在 Wayland 下读写剪贴板（SteamOS 没有 wl-copy/xclip，但有 KDE 的剪贴板服务 Klipper）、切换显示桌面、查询屏保状态、导出 KWin 的诊断信息。",

        Why =
        [
            "1. 写法：`qdbus6 服务名 对象路径 方法名 参数...`。",
            "- 只写服务名：列出它的对象路径。",
            "- 写到对象路径：列出该对象的全部方法、属性与信号。",
            "- 不带参数运行 `qdbus6`：列出会话总线上所有服务。",
            "2. 常用服务：",
            "- `org.kde.klipper /klipper`：剪贴板。`getClipboardContents` 读取、`setClipboardContents 文字` 写入、`getClipboardHistoryItem N` 读取历史第 N 条、`clearClipboardHistory` 清空历史。",
            "- `org.kde.KWin /KWin`：窗口管理器。`showDesktop true/false` 显示桌面、`currentDesktop` 当前虚拟桌面、`supportInformation` 导出诊断信息。",
            "- `org.freedesktop.ScreenSaver /ScreenSaver`：`GetActive` 查询是否处于屏保/锁屏，`Lock` 锁屏。",
            "3. qdbus6 与 busctl：两者访问的是同一条总线（见「D-Bus 消息总线」）。qdbus6 对 KDE 的接口写法更简短，busctl 在所有 systemd 系统上都有。",
            "4. 在脚本中读写剪贴板：`qdbus6 org.kde.klipper /klipper setClipboardContents \"$(命令)\"` 把命令输出复制到剪贴板，之后可以在任何程序中粘贴。",
        ],

        Consequences =
        [
            "• 读取剪贴板会得到其中的内容，可能包含你刚复制的密码等敏感信息。",
            "• `setClipboardContents` 会覆盖当前剪贴板内容；`clearClipboardHistory` 清空全部剪贴板历史，无法恢复。",
            "• 这些命令需要在 KDE 桌面会话中运行。",
        ],

        Steps =
        [
            Step.Read(
                "列出会话总线上的 KDE 服务",
                "qdbus6 | grep -E 'org\\.kde\\.(klipper|KWin|plasmashell)|ScreenSaver'",
                "筛选出本模块用到的几个服务。").Then("输出 `org.kde.klipper`、`org.kde.plasmashell`、`org.kde.KWin`、`org.freedesktop.ScreenSaver` 等。"),
            Step.Read(
                "查看剪贴板服务提供的方法",
                "qdbus6 org.kde.klipper /klipper | grep -i clipboard",
                "写到对象路径即列出成员。").Then("列出 `method QString ...getClipboardContents()`、`method void ...setClipboardContents(QString s)`、`clearClipboardHistory()` 等。"),
            Step.Change(
                "把命令输出复制到剪贴板",
                "qdbus6 org.kde.klipper /klipper setClipboardContents \"$(ip -4 -br addr show wlan0 | awk '{print $3}')\"",
                "会覆盖剪贴板当前的内容。").Then("没有输出；此时在任意程序中按 `Ctrl+V`，粘贴出的是本机的 IP 地址。"),
            Step.Read(
                "读取剪贴板内容",
                "qdbus6 org.kde.klipper /klipper getClipboardContents",
                "输出剪贴板中的文字。").Then("输出上一步复制进去的 IP 地址（或你最近复制的内容）。"),
            Step.Change(
                "切换显示桌面",
                "qdbus6 org.kde.KWin /KWin showDesktop true; sleep 2; qdbus6 org.kde.KWin /KWin showDesktop false",
                "最小化所有窗口 2 秒后恢复。").Then("所有窗口消失、露出桌面，2 秒后窗口恢复原位。"),
            Step.Read(
                "查询屏幕是否处于锁屏/屏保",
                "qdbus6 org.freedesktop.ScreenSaver /ScreenSaver GetActive",
                "脚本可据此判断用户是否离开。").Then("正在使用时输出 `false`。"),
            Step.Change(
                "导出 KWin 诊断信息",
                "qdbus6 org.kde.KWin /KWin supportInformation > ~/kwin-info.txt && head -n 20 ~/kwin-info.txt",
                "向 KDE 报告显示或窗口问题时附上它。").Then("家目录生成 `kwin-info.txt`，开头是「KWin 技术支持信息」的说明文字，随后是 KWin 版本、编译选项、平台 (Wayland)、显示器、OpenGL 驱动与合成设置等信息。"),
        ],

        Verify =
        [
            "1. 用 `setClipboardContents` 写入一段文字，再按 `Ctrl+V` 粘贴到 Kate 中，内容一致。",
        ],
    };
}
