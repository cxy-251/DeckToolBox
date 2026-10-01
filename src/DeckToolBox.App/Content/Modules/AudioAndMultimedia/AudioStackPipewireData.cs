using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.AudioAndMultimedia;

public static class AudioStackPipewireData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "audio-pipewire",
        Title = "PipeWire 音频栈",
        Keywords = "wpctl · pw-top · WirePlumber",
        Level = ModuleLevel.Intermediate,
        Overview = "Linux 音频从 ALSA、PulseAudio、JACK 并存，统一到了 PipeWire。弄清 ALSA（内核驱动）、PipeWire（音频图引擎）与 WirePlumber（策略与设备管理）的分工，就能排查「没声音」「蓝牙耳机不工作」这类问题。",

        Why =
        [
            "1. Linux 音频的几层：",
            "- **ALSA**：内核中的声卡驱动，提供 `/dev/snd/` 设备。单独使用时，一个声卡通常同时只能被一个程序打开。",
            "- **PulseAudio**：曾经的桌面音频服务，负责多程序混音、设备切换、网络音频，但延迟较高，不适合专业音频。",
            "- **JACK**：面向音乐制作的低延迟音频服务，配置复杂，与日常桌面程序配合不佳。",
            "2. PipeWire：",
            "• 由 Red Hat 的 Wim Taymans 发起，现已成为 Fedora、Ubuntu、Arch 和 SteamOS 等主流发行版的默认音频服务。",
            "• 把所有音频（以及视频、屏幕共享）的来源和去处都看作「节点」，节点之间用「连接」组成处理图，由 PipeWire 统一调度。",
            "• **共享内存传输**：节点之间通过 `memfd` 共享内存交换音频数据，不必在进程之间反复复制；延迟可调到与 JACK 相当的低水平，满足音乐制作的需要。",
            "• 同时提供 PulseAudio 与 JACK 的兼容接口，原来为它们编写的程序无需修改即可使用。",
            "3. WirePlumber：PipeWire 的会话管理器，负责策略：插上耳机时切换输出、记住每个设备的音量、为蓝牙耳机选择编码格式等。",
            "4. Steam Deck 的麦克风处理：PipeWire 按机型读取 `/usr/share/pipewire/hardware-profiles/valve-<机型>/` 下的配置（`jupiter` 为 LCD 版，`galileo` 为 OLED 版）。其中的 filter-chain 为内置麦克风建立一条处理链：左右两路先经过 RNNoise 降噪插件（`rnnoise_ladspa.so`，配置中节点名为 `noisetorch_*`），再进入 Valve 用 Faust 编写的 `valve_deck_microphone` 处理，最后作为名为 `filter-chain-source` 的虚拟麦克风提供给程序。这些处理都在用户态的 PipeWire 中完成。",
            "5. ALSA UCM：声卡有哪些「设备」（扬声器、耳机、内置麦克风、耳麦）、各自对应哪个 PCM、靠哪个开关检测耳机插拔，写在 `/usr/share/alsa/ucm2/` 的用例配置里。WirePlumber 读取这些配置，才知道插上耳机时该切到哪个输出。",
        ],

        Consequences =
        [
            "• `wpctl status`、`pw-top`、`pactl info` 只读。",
            "• 重启音频服务 (`systemctl --user restart pipewire pipewire-pulse wireplumber`) 会让声音中断约一秒，正在播放的程序通常会自动重连。",
            "• 绕过 PipeWire 直接用 `aplay -D hw:0,0` 打开硬件设备时，如果 PipeWire 正占用该设备，会报「设备忙」；即使成功，也会抢走其他程序的声音。日常播放请始终经过 PipeWire。",
        ],

        Steps =
        [
            Step.Read(
                "查看音频设备与正在发声的程序",
                "wpctl status",
                "Sink 是输出（扬声器、耳机），Source 是输入（麦克风）。").Then("按 Audio / Video 分组的树：`Devices` 声卡、`Sinks` 输出（带 `*` 的是默认）、`Sources` 输入、`Streams` 正在播放或录音的程序，每项有数字 ID 和 `[vol: 0.12]` 这样的音量。"),
            Step.Read(
                "查看音频处理的缓冲与错误计数",
                "pw-top -b -n 1",
                "`-b` 批处理模式输出纯文本，`-n 1` 只输出一次。`ERR` 持续增长说明有爆音或掉帧。").Then("输出一次快照表格：每行一个节点，`QUANT` 缓冲帧数、`RATE` 采样率、`WAIT`/`BUSY` 耗时、`ERR` 错误数，最后一列是节点名。"),
            Step.Read(
                "查看内核识别的声卡",
                "cat /proc/asound/cards && ls -l /dev/snd/",
                "Steam Deck 使用 AMD 音频协处理器 (ACP) 配合 SOF 固件，扬声器与耳机由 NAU8821 编解码器和 MAX98388 功放驱动。").Then("先列出声卡编号与名称（`0 [Generic]` 是 HDMI/DP 输出，`1 [sofnau8821max]` 是板载声卡），再列出 `/dev/snd/` 下的 `controlC1`、`pcmC1D0p`（声卡 1 的第 0 个播放设备，`c` 结尾为录音）等设备节点。"),
            Step.Read(
                "查看本机麦克风的降噪处理链",
                "p=$(tr 'A-Z' 'a-z' < /sys/class/dmi/id/product_name); grep -nE 'name =|plugin =|output =' /usr/share/pipewire/hardware-profiles/valve-$p/filter-chain.conf.d/filter-chain.conf",
                "第一段从 DMI 读出机型名（`galileo` 或 `jupiter`），再从对应配置中只挑出节点名、插件路径和连接关系。").Then("十几行带行号的配置：两个 `noisetorch_left/right` 节点使用 `/usr/lib/ladspa/rnnoise_ladspa.so`，`valve_deck_microphone` 节点，以及 `output = \"noisetorch_left:Output\" input = \"valve_deck_microphone:in0\"` 这样的连接；末尾的 `filter.smart.target` 指明它挂在 `sof-nau8821-max` 声卡的麦克风上。"),
            Step.Read(
                "确认 PulseAudio 兼容层由 PipeWire 提供",
                "pactl info",
                "`pactl` 是 PulseAudio 的命令行工具，PipeWire 兼容它。").Then("输出十几行信息，其中 `Server Name: PulseAudio (on PipeWire 1.x.x)` 说明旧的 PulseAudio 程序实际由 PipeWire 服务；还有默认输出、输入设备名。"),
            Step.Read(
                "查看声卡在 UCM 中定义的设备",
                "alsaucm listcards; alsaucm -c hw:1 list _devices/HiFi",
                "第一条列出有 UCM 配置的声卡及编号；第二条中的 `hw:1` 换成板载声卡的编号（OLED 版为 1），`HiFi` 是 Deck 声卡唯一的用例。").Then("先输出 `0: hw:1` 与声卡描述（如 `Valve-Galileo-1`）；再列出 4 个设备：`Headphones`、`Speaker`、`Internal Mic`、`Headset`，对应「音量与音频设备」里能切换的输出与输入。"),
            Step.Change(
                "重启音频服务",
                "systemctl --user restart pipewire pipewire-pulse wireplumber",
                "HDMI 或蓝牙连接后没有声音时先试这一步，不必重启整机。").Then("没有输出。声音中断约一秒，三个音频服务重新启动；正在播放的程序通常会自动重连，个别程序需要重新播放。"),
        ],

        Verify =
        [
            "1. `systemctl --user is-active pipewire wireplumber` 输出两行 `active`。",
            "2. `wpctl status` 中 `Sinks` 下带 `*` 的是当前默认输出设备，音量值在 0.00~1.00 之间。",
        ],
    };
}
