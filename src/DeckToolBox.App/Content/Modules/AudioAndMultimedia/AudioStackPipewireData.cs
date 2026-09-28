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
            "1. Linux 音频演进的历史阵痛与割裂：",
            "• **ALSA (Advanced Linux Sound Architecture)**：内核级声音子系统，直接与音频芯片/声卡驱动通信，暴露 `/dev/snd/pcm*` 字符设备。缺点是默认独占声卡设备，多个应用无法原生并发混音。",
            "• **PulseAudio (2004~2020)**：在用户空间实现了桌面多通道混音与网络推流，但设计时未考虑专业低延时，音频缓冲区过大，且无法处理视频流。",
            "• **JACK (专业音频)**：支持亚毫秒级超低延时与灵活音频跳线，但配置极度复杂，与日常桌面应用格格不入。",
            "2. 终极一统：PipeWire 图处理引擎 (Graph Engine)：",
            "• 由 Red Hat 的 Wim Taymans 发起，现已成为 Fedora、Ubuntu、Arch 和 SteamOS 等主流发行版的默认音频服务。",
            "• **核心原理**：采用类似 GStreamer 的多媒体处理图 (Processing Graph)。每个音频产生者（游戏、播放器）和消费者（扬声器、蓝牙耳机）都是图上的一个节点 (Node)。",
            "• **共享内存传输**：节点之间通过 `memfd` 共享内存交换音频数据，不必在进程之间反复复制；延迟可调到与 JACK 相当的低水平，满足音乐制作的需要。",
            "• **万能兼容层**：PipeWire 启动时会创建模拟 PulseAudio 的 `/run/user/1000/pulse/native` 套接字，并提供 ALSA 插件劫持库与 JACK 仿真库，历史上的所有旧游戏与新应用无需任何代码修改即可完美并发发声。",
            "3. 会话管理器 (Session Manager)：WirePlumber：",
            "• PipeWire 本身只负责音频数据流的高效图计算，而“插拔耳机时自动切换输出设备”、“调节音量”、“蓝牙编解码器协商 (LDAC/AAC/SBC)”等业务策略决策全部由用户空间的 **WirePlumber** 守护进程统一编排。",
        ],

        Consequences =
        [
            "• 只读拓扑观测绝对安全：使用 `wpctl status`、`pw-top` 或 `pactl info` 仅实时拉取当前音频拓扑图与声卡硬件采样率，不影响任何正在播放的声音流。",
            "• 音频服务无损平滑热重启：若因蓝牙耳机异常断开导致音频无声，执行 `systemctl --user restart pipewire pipewire-pulse wireplumber` 会在 100 毫秒内重构音频图，不会影响前台游戏或桌面进程的正常运行。",
            "• 绕过 PipeWire 直接用 `aplay -D hw:0,0` 打开硬件设备时，如果 PipeWire 正占用该设备，会报「设备忙」；即使成功，也会抢走其他程序的声音。日常播放请始终经过 PipeWire。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前系统全量音频拓扑树与活动音频设备 (WirePlumber)",
                "wpctl status",
                "使用 WirePlumber 控制器查看 Audio Sinks (输出扬声器/耳机)、Sources (麦克风)、以及前台各应用程序所占用的音频节点 ID 与音量百分比。"),
            Step.Read(
                "实时观测 PipeWire 音频图处理性能与缓冲量子 (pw-top)",
                "pw-top -b -n 1",
                "类似于 top 的音频性能监视器，查看量子大小 (QUANT, 如 1024 帧)、当前硬件工作采样率 (RATE, 如 48000Hz)、以及处理耗时与 ERR 错误丢帧计数。"),
            Step.Read(
                "查看内核层物理声卡与 ALSA PCM 设备节点清单",
                "cat /proc/asound/cards && ls -l /dev/snd/",
                "查看内核识别到的声卡。Steam Deck 使用 AMD 音频协处理器 (ACP) 配合 SOF 固件，扬声器与耳机由 NAU8821 编解码器和 MAX98388 功放驱动。"),
            Step.Read(
                "排查 PulseAudio 仿真套接字健康状况与服务器版本",
                "pactl info",
                "验证 Server Name 是否明确显示为 \"PulseAudio (on PipeWire ...)\"，证实兼容层正常拦截并接管旧应用的音频请求。"),
            Step.Change(
                "无损重置用户空间音频图调度服务链（故障自愈指令）",
                "systemctl --user restart pipewire pipewire-pulse wireplumber",
                "当遇到外接 HDMI/显示器扩展坞音频丢失或蓝牙连接后无声时，执行该标准服务重启指令秒级自愈，无需重启掌机操作系统。"),
        ],

        Verify =
        [
            "1. 服务健康度核验：执行 `systemctl --user is-active pipewire wireplumber`，两行均应明确返回 `active`。",
            "2. 节点路由验证：在执行 `wpctl status` 时，带有 `*` 标识的 Sink 节点即为当前系统的默认音频主输出设备（如板载扬声器 Speaker 或外置 DAC 耳机），音量值在 0.00~1.00 之间为正常区间。",
        ],
    };
}
