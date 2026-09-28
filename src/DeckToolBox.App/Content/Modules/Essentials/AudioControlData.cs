using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class AudioControlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "audio-control",
        Title = "音量与音频设备：wpctl",
        Keywords = "wpctl · 默认设备 · 静音 · 麦克风测试",
        Level = ModuleLevel.Basic,
        Overview = "wpctl 是 WirePlumber 的命令行工具，能查看所有音频设备、切换默认输出、调节音量和静音，还能快速录一段麦克风试听。音频栈的原理见系统原理专区的「PipeWire 音频栈」。",

        Why =
        [
            "1. 设备 ID：`wpctl status` 列出所有设备，每个前面有一个数字 ID，带 `*` 的是当前默认设备。ID 在重启或插拔后可能变化，所以脚本里更适合用下面的别名。",
            "2. 别名：",
            "- `@DEFAULT_AUDIO_SINK@`：默认输出（扬声器 / 耳机）。",
            "- `@DEFAULT_AUDIO_SOURCE@`：默认输入（麦克风）。",
            "3. 音量写法：",
            "- `0.5` 或 `50%`：设为指定值。",
            "- `5%+` / `5%-`：相对增减。",
            "- `-l 1.0`：限制最大值，防止增减时超过 100% 导致失真。",
            "4. Sink 与 Source：Sink 是声音的去处（扬声器、耳机、HDMI），Source 是声音的来源（麦克风）。「没声音」时先确认默认 Sink 选对了。",
        ],

        Consequences =
        [
            "• `status`、`get-volume`、`inspect` 只读。",
            "• 设置音量、默认设备会立即生效，并被 WirePlumber 记住。",
            "• 录音测试会录下周围的声音，测试文件放在 `/tmp`，重启后自动清除。",
        ],

        Steps =
        [
            Step.Read(
                "查看所有音频设备与当前默认设备",
                "wpctl status",
                "`*` 表示默认设备。").Then("树状输出：`Audio` 下的 `Devices`、`Sinks`（输出）、`Sources`（输入）、`Streams`（正在发声的程序），每项前面是数字 ID，默认设备前有 `*`，后面是 `[vol: 0.12]` 这样的音量。"),
            Step.Read(
                "查看默认输出的音量与名称",
                "wpctl get-volume @DEFAULT_AUDIO_SINK@; wpctl inspect @DEFAULT_AUDIO_SINK@ | grep node.description",
                "`@DEFAULT_AUDIO_SINK@` 表示默认输出，不需要知道设备 ID。").Then("第一行 `Volume: 0.12`（即 12%，静音时后面有 `[MUTED]`），第二行 `node.description = \"ACP/ACP3X/ACP6x Audio Coprocessor Speaker\"`，即当前的默认输出设备。"),
            Step.Change(
                "调节音量",
                "wpctl set-volume -l 1.0 @DEFAULT_AUDIO_SINK@ 5%+",
                "`-l 1.0` 限制最大值为 100%；`5%-` 降低；`0.4` 直接设为 40%。").Then("没有输出。音量增加 5%，屏幕上弹出音量提示，但不会超过 100%。"),
            Step.Change(
                "切换静音",
                "wpctl set-mute @DEFAULT_AUDIO_SINK@ toggle",
                "`toggle` 表示切换；也可以写 `1`（静音）或 `0`（取消静音）。麦克风把 `SINK` 换成 `SOURCE`。").Then("没有输出。原来有声音则变为静音（托盘音量图标显示静音），再执行一次恢复。"),
            Step.Change(
                "切换默认输出设备",
                "wpctl set-default <设备ID>",
                "`<设备ID>` 取自 `wpctl status` 中 Sinks 列表前的数字。").Then("没有输出。声音立即改从新设备播放，`wpctl status` 中 `*` 移到该设备前。"),
            Step.Change(
                "录 5 秒麦克风并回放",
                "timeout 5 pw-record /tmp/mic-test.wav; pw-play /tmp/mic-test.wav",
                "`timeout 5` 让录音 5 秒后自动停止。").Then("终端静止 5 秒（正在录音），然后通过默认输出播放刚才录下的声音。`/tmp/mic-test.wav` 保留到重启。"),
        ],

        Verify =
        [
            "1. 调节音量后，`wpctl get-volume @DEFAULT_AUDIO_SINK@` 的数值随之变化，桌面右下角的音量图标同步变化。",
            "2. 回放录音时能听到自己的声音，说明麦克风与默认输入设置正确。",
        ],
    };
}
