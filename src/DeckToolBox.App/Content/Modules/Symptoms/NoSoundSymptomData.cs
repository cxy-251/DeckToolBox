using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class NoSoundSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-no-sound",
        Title = "没有声音或声音从错误的设备出来",
        Keywords = "没声音 · 静音 · 耳机 · HDMI · 扩展坞 · 输出设备 · PipeWire",
        Level = ModuleLevel.Basic,
        Overview = "扬声器没声音，插了耳机声音还从扬声器出来，或者接扩展坞、显示器后声音跑到了显示器上。先看声音被送到了哪个输出设备、音量是多少，再检查音频服务是否正常。",

        Why =
        [
            "1. **默认输出设备不对**：接上 HDMI/DP 显示器或蓝牙耳机后，系统会自动切换到新设备；拔掉后有时没有切回来。",
            "2. **音量为 0 或被静音**：包括系统音量和单个程序的音量。",
            "3. **音频服务异常**：PipeWire 或 WirePlumber 崩溃、卡住，所有程序都没有声音。重启这几个用户服务即可，不必重启整机。",
        ],

        Consequences =
        [
            "• 第 1、2 步只读。",
            "• 第 3 步重启音频服务，声音会中断约一秒。",
        ],

        Steps =
        [
            Step.Read(
                "声音现在送到哪个设备、音量多少",
                "wpctl inspect @DEFAULT_AUDIO_SINK@ | grep node.description; wpctl get-volume @DEFAULT_AUDIO_SINK@",
                "`@DEFAULT_AUDIO_SINK@` 代表当前默认输出设备。").Then("如 `node.description = \"ACP/ACP3X/ACP6x Audio Coprocessor Speaker\"`（内置扬声器）与 `Volume: 0.44`。**设备不是你想要的**（如显示器名称）→ 用 `wpctl status` 找到正确设备的编号，再 `wpctl set-default <编号>`，或在快捷菜单中切换。**音量为 0 或后面有 `[MUTED]`** → 调高音量或取消静音。"),
            Step.Read(
                "音频服务是否都在运行",
                "systemctl --user is-active pipewire pipewire-pulse wireplumber",
                "三个用户服务分别是音频核心、PulseAudio 兼容层和会话管理器。").Then("正常时输出三行 `active`。**任何一个不是 `active`** → 做第 3 步。"),
            Step.Change(
                "重启音频服务",
                "systemctl --user restart pipewire pipewire-pulse wireplumber",
                "所有程序都没声音、或设备列表异常时使用。").Then("没有输出。声音中断约一秒后恢复，正在播放的程序通常会自动重连。"),
        ],

        Verify =
        [
            "• `pw-play /usr/share/sounds/freedesktop/stereo/bell.oga` 能从预期的设备听到提示音。",
        ],

        Related = ["audio-control", "audio-pipewire", "man-alsa-tools", "sym-bt-audio"],
    };
}
