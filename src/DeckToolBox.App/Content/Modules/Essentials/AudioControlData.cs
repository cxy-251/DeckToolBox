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
                "看 `Audio` 部分的 `Sinks`（输出）与 `Sources`（输入），`*` 表示默认。"),
            Step.Read(
                "查看默认输出的音量与名称",
                "wpctl get-volume @DEFAULT_AUDIO_SINK@; wpctl inspect @DEFAULT_AUDIO_SINK@ | grep node.description",
                "静音时音量后会显示 `[MUTED]`。"),
            Step.Change(
                "调节音量",
                "wpctl set-volume -l 1.0 @DEFAULT_AUDIO_SINK@ 5%+",
                "`5%-` 降低；`0.4` 直接设为 40%。"),
            Step.Change(
                "切换静音",
                "wpctl set-mute @DEFAULT_AUDIO_SINK@ toggle",
                "麦克风静音把 `SINK` 换成 `SOURCE`。"),
            Step.Change(
                "切换默认输出设备",
                "wpctl set-default <设备ID>",
                "`<设备ID>` 取自 `wpctl status` 中 Sinks 列表前的数字，例如从扬声器切到 HDMI 或蓝牙耳机。"),
            Step.Change(
                "录 5 秒麦克风并回放",
                "timeout 5 pw-record /tmp/mic-test.wav; pw-play /tmp/mic-test.wav",
                "对着麦克风说话，5 秒后自动停止并播放。"),
        ],

        Verify =
        [
            "1. 调节音量后，`wpctl get-volume @DEFAULT_AUDIO_SINK@` 的数值随之变化，桌面右下角的音量图标同步变化。",
            "2. 回放录音时能听到自己的声音，说明麦克风与默认输入设置正确。",
        ],
    };
}
