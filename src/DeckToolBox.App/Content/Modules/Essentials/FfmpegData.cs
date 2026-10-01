using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class FfmpegData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "ffmpeg",
        Title = "ffmpeg：音视频处理",
        Keywords = "ffmpeg · ffprobe · 剪辑 · 压缩 · VAAPI",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 自带 ffmpeg：查看媒体信息、无损剪切片段、压缩视频便于分享、提取音频、截图、做 GIF，还能调用 APU 的硬件编码器。录好的游戏视频不必传到电脑上再处理。",

        Why =
        [
            "1. 容器与编码：`.mp4`、`.mkv` 是容器，里面装着视频流（H.264、HEVC、AV1 等）和音频流（AAC、Opus 等）。`ffprobe` 显示一个文件里有哪些流、用什么编码。",
            "2. 复制还是重新编码：",
            "- `-c copy`：直接复制原始数据，几乎瞬间完成、画质无损；但剪切点只能落在关键帧上，可能比指定时间早一点开始。",
            "- 指定编码器（如 `-c:v libx264`）：重新编码，可以改分辨率、码率，耗时较长。",
            "3. 常用参数：",
            "- `-ss 开始时间`、`-t 持续时长`：截取片段，时间可写 `90` 或 `00:01:30`。",
            "- `-vf scale=-2:720`：缩放到 720 像素高，`-2` 表示按比例计算宽度并保证为偶数。",
            "- `-crf`：画质参数，数值越大文件越小、画质越低，x264 常用 18~28。",
            "- `-vn`：去掉视频（只要音频）；`-an`：去掉音频。",
            "4. 硬件编码：Deck 的 APU 支持 VAAPI，`h264_vaapi`、`hevc_vaapi`、`av1_vaapi` 编码器用 GPU 编码，速度快、CPU 占用低，但同等体积下画质通常略逊于软件编码。",
        ],

        Consequences =
        [
            "• ffmpeg 不修改输入文件，总是写出新文件；输出文件已存在时会询问是否覆盖（加 `-y` 则直接覆盖）。",
            "• 重新编码会占满 CPU，耗电且发热，长视频可以插上电源、或在 tmux 中运行。",
            "• 转换有版权保护的内容不在本模块讨论范围内。",
        ],

        Steps =
        [
            Step.Read(
                "查看媒体文件信息",
                "ffprobe -v error -show_entries format=duration,size,bit_rate:stream=codec_type,codec_name,width,height -of compact <文件>",
                "`<文件>` 替换为音视频绝对路径；`-v error` 抑制冗余输出，`-of compact` 每个流输出一行紧凑键值。").Then("每个流一行，如 `stream|codec_name=h264|codec_type=video|width=1280|height=800`、`stream|codec_name=aac|codec_type=audio`，最后一行 `format|duration=...|size=...|bit_rate=...`。"),
            Step.Change(
                "无损截取一段",
                "ffmpeg -ss <开始时间> -i <输入> -t <时长> -c copy <输出>.mp4",
                "`<开始时间>` 支持 `00:01:30` 或纯秒数 `90`；`<时长>` 为秒数；`<输入>`/`<输出>` 带空格需引号；`-c copy` 流拷贝不重编码。").Then("几乎瞬间完成，输出文件大小大致与片段长度成正比。开头可能比指定时间早一点（从最近的关键帧开始）。"),
            Step.Change(
                "压缩为 720p 便于分享",
                "ffmpeg -i <输入> -vf scale=-2:720 -c:v libx264 -crf 26 -preset medium -c:a aac -b:a 128k <输出>.mp4",
                "`<输入>` 与 `<输出>` 填入对应路径；`-crf 26` 控制质量（数值越大体积越小）；`-preset medium` 平衡压制速度与体积。").Then("终端持续刷新一行进度：`frame=  speed=`，`speed` 表示相对实时的倍速。完成后得到体积小得多的 720p 文件。"),
            Step.Change(
                "用 GPU 硬件编码为 HEVC",
                "ffmpeg -vaapi_device /dev/dri/renderD128 -i <输入> -vf 'format=nv12,hwupload' -c:v hevc_vaapi -qp 26 -c:a copy <输出>.mp4",
                "`<输入>`、`<输出>` 填文件路径。编码由 APU 中的 VCN 视频单元完成，CPU 占用低。`-qp` 越大文件越小；把 `hevc_vaapi` 换成 `h264_vaapi` 兼容性更好。").Then("进度行的 `speed` 通常明显高于软件编码，CPU 占用较低；输出 HEVC 编码的 mp4。"),
            Step.Change(
                "提取音频",
                "ffmpeg -i <输入> -vn -c:a libopus -b:a 128k <输出>.opus",
                "`<输入>`、`<输出>` 填文件路径；`-vn` 去掉视频，`-c:a libopus` 编码为 Opus。需要 mp3 时改为 `-c:a libmp3lame -q:a 2 <输出>.mp3`。").Then("很快完成，得到只含音频的 `.opus` 文件。"),
            Step.Change(
                "截取某一帧为图片",
                "ffmpeg -ss <时间> -i <输入> -frames:v 1 <输出>.png",
                "`<时间>` 填入目标时间点（如 `00:00:15`）；`<输入>` 与 `<输出>` 填入对应路径；`-frames:v 1` 仅解码导出单一视频帧。").Then("瞬间完成，得到一张与视频分辨率相同的 png 图片。"),
            Step.Change(
                "把一段视频做成 GIF",
                "ffmpeg -ss <开始时间> -t 3 -i <输入> -vf 'fps=12,scale=480:-1:flags=lanczos' <输出>.gif",
                "`<开始时间>` 为起点，`-t 3` 截 3 秒；`fps=12` 与 `scale=480:-1` 控制帧率和宽度（`lanczos` 是缩放算法）。GIF 体积增长很快，时长、帧率、宽度都不宜大。").Then("得到一个 3 秒、每秒 12 帧、宽 480 像素的 GIF，可以直接发到聊天软件。"),
            Step.Read(
                "列出可用的硬件编码器",
                "ffmpeg -hide_banner -encoders | grep vaapi",
                "Deck 的 APU 支持 VAAPI 硬件编码。").Then("几行输出，如 ` V....D h264_vaapi  H.264/AVC (VAAPI)`、`hevc_vaapi`、`av1_vaapi`。"),
        ],

        Verify =
        [
            "1. 用 `ffprobe` 查看输出文件：分辨率、编码、时长与预期一致。",
            "2. 硬件编码时在另一个终端运行 `btop`，CPU 占用明显低于软件编码。",
        ],
    };
}
