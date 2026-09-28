using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class BluetoothData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "bluetooth",
        Title = "蓝牙设备：bluetoothctl",
        Keywords = "bluetoothctl · 配对 · 信任 · 电量",
        Level = ModuleLevel.Basic,
        Overview = "蓝牙手柄、耳机、键鼠连不上或反复断线时，命令行能看到比设置界面更多的信息。bluetoothctl 是 BlueZ 自带的工具，所有使用 BlueZ 的发行版用法相同。",

        Why =
        [
            "1. 连接一个新设备的三个步骤：",
            "- **配对 (pair)**：双方交换密钥，只需一次。设备必须处于配对模式（通常长按配对键直到指示灯快闪）。",
            "- **信任 (trust)**：允许设备以后主动重连，无需确认。手柄、键鼠应当设为信任，否则开机后需要手动连接。",
            "- **连接 (connect)**：建立本次连接。",
            "2. 设备以 MAC 地址标识，如 `D0:FA:3F:BC:EA:20`。先 `scan on` 发现设备，再对它的地址执行操作。",
            "3. 常见问题：",
            "- 能配对但连不上：先 `remove` 删除该设备，再重新配对。换过电脑配对的设备尤其常见。",
            "- 耳机音质差：蓝牙耳机在通话模式 (HSP/HFP) 下音质很差，确认音频设备选择的是 A2DP 配置（见「音量与音频设备」）。",
            "- 手柄延迟或断线：远离 2.4 GHz 干扰源（USB 3 设备、路由器），或改用 5 GHz Wi-Fi。",
        ],

        Consequences =
        [
            "• 查看状态、列出设备是只读的。",
            "• `remove` 会删除配对信息，之后需要让设备重新进入配对模式才能再次连接。",
            "• `power off` 关闭蓝牙适配器，所有蓝牙设备（包括蓝牙手柄）立即断开。",
        ],

        Steps =
        [
            Step.Read(
                "查看蓝牙适配器状态",
                "bluetoothctl show | grep -E 'Name|Powered|Discoverable|Pairable'",
                "`Powered: yes` 表示蓝牙已开启。"),
            Step.Read(
                "列出已配对的设备与当前已连接的设备",
                "bluetoothctl devices Paired; echo; bluetoothctl devices Connected",
                "每行显示设备的 MAC 地址和名称。"),
            Step.Read(
                "查看某个设备的详细信息与电量",
                "bluetoothctl info <MAC地址>",
                "`Connected`、`Trusted`、`Battery Percentage` 等字段；电量只有设备支持时才显示。"),
            Step.Change(
                "扫描附近的设备",
                "bluetoothctl --timeout 15 scan on",
                "先让设备进入配对模式，15 秒后自动停止扫描，记下目标设备的 MAC 地址。"),
            Step.Change(
                "配对、信任并连接一个新设备",
                "bluetoothctl pair <MAC地址> && bluetoothctl trust <MAC地址> && bluetoothctl connect <MAC地址>",
                "键盘配对时可能要求在键盘上输入屏幕显示的数字并回车。"),
            Step.Change(
                "删除一个设备的配对",
                "bluetoothctl remove <MAC地址>",
                "用于解决「配对了却连不上」的问题，之后重新配对。"),
            Step.Change(
                "重启蓝牙适配器",
                "bluetoothctl power off && bluetoothctl power on",
                "所有蓝牙设备会断开，已信任的设备随后自动重连。"),
        ],

        Verify =
        [
            "1. `bluetoothctl info <MAC地址>` 显示 `Paired: yes`、`Trusted: yes`、`Connected: yes`。",
            "2. 重启 Deck 后，已信任的手柄或键鼠按任意键即可自动重连。",
        ],
    };
}
