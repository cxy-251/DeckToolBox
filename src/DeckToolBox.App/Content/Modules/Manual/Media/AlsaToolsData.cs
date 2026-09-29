using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Media;

public static class AlsaToolsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-alsa-tools",
        Title = "ALSA 工具：声卡、混音器与测试",
        Keywords = "aplay -l · arecord · amixer · alsamixer · speaker-test",
        Level = ModuleLevel.Intermediate,
        Overview = "ALSA 是内核层的声卡驱动接口。日常调音量用 wpctl（见「音量与音频设备：wpctl」），但排查「系统完全没声音」「某个声卡通道被静音」时，需要直接查看 ALSA 层：有哪些声卡、硬件混音器的每个通道是什么状态、扬声器左右声道是否都能发声。",

        Why =
        [
            "1. 声卡与设备编号：`aplay -l`（播放）、`arecord -l`（录音）列出硬件，格式为 `card N: 名称, device M`。Steam Deck 上：",
            "- `card 0`：`HD-Audio Generic`，APU 的 HDMI/DP 音频输出（接显示器或底座时使用）。",
            "- `card 1`：`sof-nau8821-max`，内置扬声器、耳机孔与麦克风。",
            "2. 设备名：`hw:1,0` 表示 card 1 的 device 0，直接访问硬件；`default`、`pipewire` 经过 PipeWire，与其他程序共享声卡。日常测试应使用 `default`。",
            "3. 混音器 (mixer)：声卡芯片内部的音量与开关。PipeWire 的音量之外，芯片上还有各个通道自己的增益和静音开关。",
            "- `amixer -c 1 scontrols`：列出 card 1 的所有控制项。",
            "- `amixer -c 1 sget '名称'`：查看某项的值；`sset '名称' 80%`、`sset '名称' unmute` 修改。",
            "- `alsamixer`：终端图形界面，`F6` 选择声卡，方向键调节，`M` 切换静音（`MM` 表示静音，`OO` 表示开启），`Esc` 退出。",
            "4. `speaker-test`：播放测试音，`-c 2 -t wav` 依次从左右声道念出「Front Left」「Front Right」，用于确认两个扬声器都正常。",
        ],

        Consequences =
        [
            "• 查看是只读的；修改混音器立即影响声音，WirePlumber 可能在设备切换时重新设置部分值。",
            "• 使用 `hw:` 设备直接播放时，如果 PipeWire 正占用该声卡，会报「设备或资源忙」。",
            "• speaker-test 默认循环播放，按 `Ctrl+C` 停止；先把音量调低。",
        ],

        Steps =
        [
            Step.Read(
                "列出播放与录音硬件",
                "aplay -l; arecord -l",
                "列出所有声卡与设备。").Then("播放部分列出 `card 0: Generic [HD-Audio Generic]` 的若干 HDMI 设备，以及 `card 1: sofnau8821max`；录音部分列出 `card 1` 的麦克风设备。"),
            Step.Read(
                "列出可用的设备名",
                "aplay -L | grep -E '^(default|pipewire|sysdefault|hw:)' | head",
                "`-L` 列出可在 `-D` 参数中使用的设备名。").Then("输出 `pipewire`、`default`、`sysdefault:CARD=sofnau8821max` 等名称。"),
            Step.Read(
                "查看内置声卡的混音器控制项",
                "amixer -c 1 scontrols | head -n 15",
                "`-c 1` 指定 card 1。").Then("每行一个控制项，如 `Simple mixer control 'Headphone',0`、`'Frontend PGA',0`。"),
            Step.Read(
                "查看总音量",
                "amixer sget Master",
                "未指定 `-c` 时使用默认设备（经 PipeWire）。").Then("最后两行为左右声道，如 `Front Left: Playback 26870 [41%] [on]`：数值、百分比、是否开启。"),
            Step.Read(
                "用终端界面调整混音器",
                "alsamixer -c 1",
                "方向键选择与调节，`M` 切换静音，`Esc` 退出。").Then("全屏显示一排竖条，每条是一个控制项，底部为名称，条上方显示数值；`MM` 表示该通道被静音。"),
            Step.Read(
                "测试左右声道",
                "speaker-test -D default -c 2 -t wav -l 1",
                "`-l 1` 只播放一轮后退出；先把音量调低。").Then("扬声器依次念出「Front Left」「Front Right」，终端同时打印当前播放的声道；播放完一轮自动结束。"),
            Step.Change(
                "录 3 秒并回放（ALSA 层）",
                "arecord -D default -d 3 -f cd /tmp/alsa-test.wav && aplay /tmp/alsa-test.wav",
                "`-d 3` 录 3 秒，`-f cd` 为 44.1 kHz 16 位立体声。").Then("先显示 `正在录音 WAVE '/tmp/alsa-test.wav'`，3 秒后显示 `正在播放 WAVE ...` 并放出刚才的录音。"),
        ],

        Verify =
        [
            "1. speaker-test 时两个声道都能听到对应的语音，说明左右扬声器正常。",
        ],
    };
}
