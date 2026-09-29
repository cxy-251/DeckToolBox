using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Media;

public static class EspeakData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-espeak",
        Title = "espeak-ng：文字转语音",
        Keywords = "espeak-ng · -v cmn · yue · -s 语速 · -w 保存",
        Level = ModuleLevel.Basic,
        Overview = "espeak-ng 是一个离线的语音合成引擎，支持普通话、粤语、英语等上百种语言，SteamOS 自带（屏幕阅读器 Orca 也使用它）。可以让脚本在完成任务时「说话」提醒，也可以把文字保存成音频文件。音色是机械合成音，胜在离线、轻量。",

        Why =
        [
            "1. 基本用法：`espeak-ng -v 语言 '文字'`，直接从默认音频设备播放。",
            "2. 常用语言代码：",
            "- `cmn`：普通话（汉字按普通话读）。",
            "- `yue`：粤语。",
            "- `en`、`en-us`：英语；`ja`：日语。",
            "- `espeak-ng --voices` 列出全部，`--voices=zh` 只列出中文相关的。",
            "3. 调节：`-s` 语速（每分钟词数，默认 175）、`-p` 音高（0~99）、`-a` 音量（0~200）。",
            "4. 输出：`-w 文件.wav` 保存为音频而不播放；`-f 文件.txt` 朗读一个文本文件；也可以从管道读取：`echo 文字 | espeak-ng -v cmn`。",
        ],

        Consequences =
        [
            "• 播放时会通过当前的默认输出设备发声；保存文件则只写入 wav，不影响系统。",
            "• 离线运行，不上传任何文字。",
        ],

        Steps =
        [
            Step.Read(
                "列出中文相关的语音",
                "espeak-ng --voices=zh",
                "表格列出可用的中文语音。").Then("列出 `cmn`（普通话）、`cmn-latn-pinyin`、`yue`（粤语）等，每行包括语言代码、性别与语音名。"),
            Step.Read(
                "用普通话朗读一句话",
                "espeak-ng -v cmn '你好，欢迎使用 Steam Deck'",
                "从默认输出设备播放。").Then("扬声器读出这句话（机械合成音），读完命令结束。"),
            Step.Read(
                "调慢语速、换成粤语",
                "espeak-ng -v yue -s 120 '今日天气好好'",
                "`-s 120` 比默认的 175 慢。").Then("以较慢的速度用粤语读出。"),
            Step.Change(
                "把文字保存成音频文件",
                "espeak-ng -v cmn -w /tmp/tts.wav '备份已经完成' && file /tmp/tts.wav",
                "`-w` 只写文件，不播放。").Then("`file` 显示 `WAVE audio, Microsoft PCM, 16 bit, mono 22050 Hz`，可以用任何播放器播放或发送给别人。"),
            Step.Read(
                "让长时间任务结束时语音提醒",
                "sleep 3; espeak-ng -v cmn '任务结束，退出码' \"$?\"",
                "把 `sleep 3` 换成你的实际命令；`$?` 是上一条命令的退出码。").Then("3 秒后读出「任务结束，退出码 0」。"),
        ],

        Verify =
        [
            "1. `aplay /tmp/tts.wav` 能播放出保存的语音。",
        ],
    };
}
