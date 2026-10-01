using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class BluetoothAudioSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-bt-audio",
        Title = "蓝牙设备连不上或音质差",
        Keywords = "蓝牙 · 耳机 · 手柄 · 连不上 · 配对 · 音质差 · 延迟",
        Level = ModuleLevel.Basic,
        Overview = "蓝牙耳机、手柄、键鼠配对失败、连上又断，或者耳机声音发闷、延迟大。先确认蓝牙开着、设备的配对状态，再根据情况重新配对或调整耳机的配置。",

        Why =
        [
            "1. **蓝牙被关闭或屏蔽**：飞行模式或快捷设置中关掉了蓝牙。",
            "2. **配对信息失效**：设备和别的电脑或手机配对过后，Deck 保存的配对信息不再有效，表现为「能看到、连不上」。删除后重新配对即可。",
            "3. **耳机进入了通话模式**：使用麦克风时耳机切换到 HSP/HFP 通话配置，音质会明显变差。",
            "4. **干扰**：2.4 GHz 的 Wi-Fi、USB 3 设备与蓝牙使用相近频段，靠得太近会断连或卡顿。",
        ],

        Consequences =
        [
            "• 第 1、2 步只读。",
            "• 删除配对后，设备需要重新进入配对模式再配对一次。",
        ],

        Steps =
        [
            Step.Read(
                "蓝牙是否打开、是否被屏蔽",
                "bluetoothctl show | grep -E 'Powered|Pairable'; rfkill list bluetooth",
                "`rfkill` 显示蓝牙是否被软件或硬件开关屏蔽。").Then("正常时 `Powered: yes`，`Soft blocked: no`、`Hard blocked: no`。**`Powered: no`** → 执行 `bluetoothctl power on`。**`Soft blocked: yes`** → 执行 `rfkill unblock bluetooth`。"),
            Step.Read(
                "设备的配对与连接状态",
                "bluetoothctl devices Paired; echo ---; bluetoothctl devices Connected",
                "上半部分是已配对的设备，下半部分是当前已连接的设备。可能夹杂几行 `[NEW] Media ...` 之类的提示，可以忽略。").Then("每个设备一行 `Device <MAC地址> <名称>`。**设备在已配对列表里却始终连不上** → 执行 `bluetoothctl remove <MAC地址>` 删除配对，让设备重新进入配对模式后再配对（见「蓝牙设备：bluetoothctl」）。"),
            Step.Read(
                "耳机当前的配置与编码",
                "pactl list sinks | grep -E 'Name: bluez|api.bluez5.(codec|profile)'",
                "只在连着蓝牙耳机时有输出。").Then("如 `api.bluez5.profile = \"a2dp-sink\"` 与 `api.bluez5.codec = \"aac\"`。**profile 是 `headset-head-unit`** → 耳机处于通话模式，音质差是正常的；不用麦克风时在「系统设置 → 音频」中把它切回 A2DP 配置。"),
        ],

        Verify =
        [
            "• 设备出现在第 2 步的已连接列表中，能正常使用；耳机的 profile 为 `a2dp-sink`。",
        ],

        Related = ["bluetooth", "audio-control", "sym-no-sound"],
    };
}
