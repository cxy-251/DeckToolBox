using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Network;

public static class KdeConnectData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-kdeconnect",
        Title = "KDE Connect：与手机互联",
        Keywords = "kdeconnect-cli · 配对 · 传文件 · 剪贴板 · 找手机 · 通知",
        Level = ModuleLevel.Basic,
        Overview = "KDE Connect 让 Deck 与手机在同一局域网内互联：互传文件和链接、共享剪贴板、在 Deck 上看手机通知、用手机当触控板和键盘、让手机响铃找到它。SteamOS 自带 KDE Connect，手机端安装 KDE Connect 应用即可；kdeconnect-cli 是它的命令行界面。",

        Why =
        [
            "1. 前提：Deck 与手机连接同一个局域网，手机上安装 KDE Connect（Android 在应用商店或 F-Droid，iOS 在 App Store）。",
            "2. 配对：首次需要配对，一端发出请求，另一端确认；之后在同一网络中自动连接。配对后的通信经过加密。",
            "3. 常用功能（手机端可在应用中逐项开关）：",
            "- 分享：把文件、链接从 Deck 发到手机，或反向发送。",
            "- 剪贴板同步：在一端复制，另一端直接粘贴。",
            "- 通知同步：手机上的通知显示在 Deck 桌面。",
            "- 远程输入：手机当触控板、键盘使用，适合 Deck 接电视时。",
            "- 响铃：让手机以最大音量响铃，找手机。",
            "4. 防火墙：KDE Connect 使用 1714–1764 端口，在 SteamOS 默认放行的 1024–65535 范围内，通常无需额外设置。",
            "5. kdeconnect-cli：`-l` 列出设备，`-d 设备ID` 或 `-n 设备名` 指定设备，后接操作，如 `--ping`、`--ring`、`--share 文件`、`--pair`。",
        ],

        Consequences =
        [
            "• 配对后，对方设备可以按你开启的功能访问：发送文件、读取剪贴板、远程输入等。只与自己的设备配对。",
            "• 取消配对：`kdeconnect-cli -n 设备名 --unpair`，或在系统设置 → KDE Connect 中操作。",
        ],

        Steps =
        [
            Step.Read(
                "列出发现的设备",
                "kdeconnect-cli -l",
                "手机需打开 KDE Connect 应用并在同一网络中。").Then("每个设备一行，如 `- 我的手机: 1a2b3c... (paired and reachable)`；最后一行 `找到 N 个设备`。未配对的设备显示 `reachable`。"),
            Step.Change(
                "向手机发起配对",
                "kdeconnect-cli -n '<设备名>' --pair",
                "在手机上确认配对请求。").Then("手机弹出配对请求，确认后再执行 `kdeconnect-cli -l`，状态变为 `paired and reachable`。"),
            Step.Change(
                "测试连通",
                "kdeconnect-cli -n '<设备名>' --ping-msg '来自 Steam Deck'",
                "向手机发送一条通知。").Then("手机上收到一条内容为「来自 Steam Deck」的通知。"),
            Step.Change(
                "把文件发送到手机",
                "kdeconnect-cli -n '<设备名>' --share ~/Pictures/<文件名>",
                "也可以分享网址：`--share https://...`，手机会用浏览器打开。").Then("手机收到文件（Android 默认保存到「下载」目录），并显示完成通知。"),
            Step.Change(
                "让手机响铃",
                "kdeconnect-cli -n '<设备名>' --ring",
                "即使手机处于静音模式也会响。").Then("手机以最大音量响铃，直到在手机上点击停止。"),
        ],

        Verify =
        [
            "1. 在手机上复制一段文字，在 Deck 上按 `Ctrl+V`，能粘贴出同样的内容（需在手机端开启剪贴板同步）。",
        ],
    };
}
