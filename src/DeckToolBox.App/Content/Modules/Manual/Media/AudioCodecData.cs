using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Media;

public static class AudioCodecData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-audio-codec",
        Title = "flac、lame、mpg123：音频编码与播放",
        Keywords = "flac 无损 · lame MP3 · mpg123 · metaflac · -V2",
        Level = ModuleLevel.Basic,
        Overview = "除了万能的 ffmpeg，SteamOS 还自带几个专门的音频工具：flac 做无损压缩，lame 编码 MP3，mpg123 在终端播放或解码 MP3。它们参数简单，适合处理音乐文件。",

        Why =
        [
            "1. 无损与有损：",
            "- FLAC 是无损压缩：体积通常为 WAV 的 50%~70%，解码后与原始音频逐样本完全一致。",
            "- MP3 是有损压缩：去掉人耳不易察觉的部分，体积更小，但无法还原原始数据。",
            "2. flac：`flac 文件.wav` 编码为 `.flac`；`flac -d` 解码；`-8` 最高压缩级别（更慢，体积略小）；`-t` 校验文件完整性；`metaflac` 查看与编辑标签和流信息。",
            "3. lame：`lame -V2 输入.wav 输出.mp3` 使用可变码率，`-V0`（最高质量）到 `-V9`（最小体积），`-V2` 平均约 190 kbps，是常用的平衡点；`-b 320` 固定 320 kbps。",
            "4. mpg123：`mpg123 文件.mp3` 在终端播放，播放时 `s` 暂停、`f` 下一首、`q` 退出；`-w 输出.wav` 解码为 WAV 而不播放（选项要写在文件名之前）。",
            "5. 其他格式（AAC、Opus、OGG）以及批量转换，使用 ffmpeg（见「ffmpeg：音视频处理」与「xargs 与 parallel」）。",
        ],

        Consequences =
        [
            "• 编码生成新文件，默认不删除原文件（flac 加 `--delete-input-file` 才会删除）。",
            "• 从 MP3 转回 WAV 或 FLAC 不会恢复丢失的音质，只会让文件变大。",
        ],

        Steps =
        [
            Step.Change(
                "生成一段 3 秒的测试音频",
                "mkdir -p /tmp/audio-demo && cd /tmp/audio-demo && ffmpeg -loglevel error -f lavfi -i sine=frequency=440:duration=3 -ac 2 t.wav && ls -l t.wav",
                "440 Hz 正弦波，立体声。").Then("生成约 517 KB 的 `t.wav`。").Practice(),
            Step.Change(
                "无损压缩为 FLAC",
                "flac -s t.wav && ls -l t.wav t.flac",
                "`-s` 不显示进度。").Then("可能提示 `WARNING: skipping unknown chunk 'LIST'`（WAV 中的附加信息块，不影响音频）；生成的 `t.flac` 明显小于 `t.wav`。").Practice(),
            Step.Read(
                "证明 FLAC 是无损的",
                "ffmpeg -loglevel error -i t.wav -f md5 -; ffmpeg -loglevel error -i t.flac -f md5 -",
                "分别计算两者解码后音频数据的 MD5。").Then("两行 `MD5=...` 完全相同：解码后的音频与原始 WAV 逐样本一致。"),
            Step.Read(
                "查看 FLAC 流信息并校验",
                "metaflac --list --block-type=STREAMINFO t.flac | grep -E 'sample_rate|channels|total samples'; flac -t t.flac",
                "`-t` 解码一遍但不输出文件，用来检查损坏。").Then("输出 `sample_rate: 44100 Hz`、`channels: 2`、`total samples: 132300`，以及 `t.flac: ok`。"),
            Step.Change(
                "编码为 MP3",
                "lame --quiet -V2 t.wav t.mp3 && ls -l t.*",
                "`-V2` 可变码率，质量与体积的常用折中。").Then("生成的 `t.mp3` 约 20 KB，远小于 FLAC 与 WAV。").Practice(),
            Step.Read(
                "在终端播放 MP3",
                "mpg123 t.mp3",
                "播放时 `s` 暂停、`q` 退出。").Then("显示文件信息与播放进度，扬声器播放 3 秒的提示音后结束。"),
            Step.Change(
                "把 MP3 解码为 WAV",
                "mpg123 -q -w out.wav t.mp3 && file out.wav",
                "`-w` 必须写在文件名之前。").Then("`file` 显示 `WAVE audio, Microsoft PCM, 16 bit, stereo 44100 Hz`。").Practice(),
        ],

        Verify =
        [
            "1. `ls -lS /tmp/audio-demo` 按大小排列：WAV > FLAC > MP3。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/audio-demo` 清理。",
        ],
    };
}
