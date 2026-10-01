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
            "4. 蓝牙音频编码：连接耳机时，PipeWire 与耳机协商一种双方都支持的编码。SteamOS 的 PipeWire 带有 SBC（含 SBC-XQ）、AAC、aptX / aptX HD、LDAC、FastStream、Opus 与 LE Audio 的 LC3 编码插件（`/usr/lib/spa-0.2/bluez5/`）。音质高的编码（如 LDAC）缓冲也更大，游戏中觉得声音明显滞后时，可在桌面模式「系统设置 → 音频」里给耳机选一个延迟更低的配置。",
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
                "`Powered: yes` 表示蓝牙已开启。").Then("输出几行：`Name: <主机名>`、`Powered: yes`、`Discoverable: no`、`Pairable: yes`。"),
            Step.Read(
                "列出已配对的设备与当前已连接的设备",
                "bluetoothctl devices Paired; echo; bluetoothctl devices Connected",
                "MAC 地址用于后续命令。").Then("两组列表，每行 `Device D0:FA:3F:BC:EA:20 Logi M750` 这样的「MAC 地址 名称」；上一组是已配对的，空行后是此刻已连接的。"),
            Step.Read(
                "查看某个设备的详细信息与电量",
                "bluetoothctl info <MAC地址>",
                "`<MAC地址>` 可由第 2 步列表查得；输出括号内是十进制电量百分比。").Then("列出该设备的名称、类型图标、`Paired`、`Trusted`、`Connected`、支持的服务 UUID，支持时还有 `Battery Percentage: 0x50 (80)`。"),
            Step.Read(
                "查看蓝牙耳机实时音频编码",
                "pactl list sinks | grep -E 'Name: bluez|api.bluez5.(codec|profile)'",
                "需要先连上一副蓝牙耳机。只挑出蓝牙输出设备的名称、编码和配置。").Then("连着蓝牙耳机时输出几行，如 `Name: bluez_output.XX_XX_XX_XX_XX_XX.1`、`api.bluez5.codec = \"ldac\"`、`api.bluez5.profile = \"a2dp-sink\"`；没有蓝牙耳机时没有输出。"),
            Step.Change(
                "扫描附近的设备",
                "bluetoothctl --timeout 15 scan on",
                "先让设备进入配对模式（通常长按配对键直到指示灯快闪）。").Then("15 秒内滚动出现 `[NEW] Device xx:xx:... 名称` 行，找到处于配对模式的目标设备后记下它的地址；时间到自动停止。"),
            Step.Change(
                "配对、信任并连接一个新设备",
                "bluetoothctl pair <MAC地址> && bluetoothctl trust <MAC地址> && bluetoothctl connect <MAC地址>",
                "`<MAC地址>` 可由上步扫描结果取得；键盘配对时可能要求在键盘上盲打数字并回车。").Then("依次提示 `Pairing successful`、`Changing ... trust succeeded`、`Connection successful`。设备的指示灯变为常亮或熄灭，随即可以使用。"),
            Step.Change(
                "删除一个设备的配对",
                "bluetoothctl remove <MAC地址>",
                "`<MAC地址>` 可由 `bluetoothctl devices Paired` 查得；用于解决配对异常，删后需设备重新进配对模式。").Then("提示 `Device has been removed`，设备断开并从已配对列表中消失。"),
            Step.Change(
                "重启蓝牙适配器",
                "bluetoothctl power off && bluetoothctl power on",
                "蓝牙整体异常时使用。").Then("依次提示 `Changing power off succeeded` 与 `Changing power on succeeded`。所有蓝牙设备先断开，几秒后已信任的设备自动重连。"),
        ],

        Verify =
        [
            "1. `bluetoothctl info <MAC地址>` 显示 `Paired: yes`、`Trusted: yes`、`Connected: yes`。",
            "2. 重启 Deck 后，已信任的手柄或键鼠按任意键即可自动重连。",
        ],
    };
}
