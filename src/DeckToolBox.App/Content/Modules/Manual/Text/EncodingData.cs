using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class EncodingData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-encoding",
        Title = "编码、换行与繁简转换",
        Keywords = "iconv · GBK · dos2unix · CRLF · opencc · 繁简",
        Level = ModuleLevel.Basic,
        Overview = "从 Windows 或旧网站拿到的文本，打开时出现乱码或多出 ^M，通常是编码或换行格式不同。iconv 转换编码，dos2unix 转换换行，opencc 做简繁中文转换（包括词汇差异，如「软件 → 軟體」）。三者都是 SteamOS 自带的。",

        Why =
        [
            "1. 字符编码：",
            "• Linux 默认使用 UTF-8。许多旧的中文 Windows 文件（txt 小说、字幕、游戏存档中的文本）使用 GBK（或 GB18030），用 UTF-8 打开就会显示为乱码。",
            "• `file -i 文件` 可以查看推测的编码；GBK 文件常被识别为 `charset=iso-8859-1` 或 `unknown-8bit`。",
            "• `iconv -f 源编码 -t 目标编码`：转换编码，结果输出到终端，用 `-o` 或重定向保存。",
            "2. 换行格式：",
            "• Linux 用 LF（`\\n`），Windows 用 CRLF（`\\r\\n`）。Windows 文本在 Linux 下常在行尾显示 `^M`；Windows 格式的 shell 脚本执行时会报 `$'\\r': 未找到命令`。",
            "• `dos2unix 文件`：把 CRLF 改为 LF（直接修改文件）；`unix2dos` 反向转换。",
            "3. opencc：开放中文转换，按词汇而不只是逐字转换。常用配置：",
            "- `s2t` 简→繁、`t2s` 繁→简。",
            "- `s2tw` / `tw2s`：简体与台湾正体互转；`s2twp` / `tw2sp` 同时转换台湾常用词汇（软件 ↔ 軟體、鼠标 ↔ 滑鼠）。",
            "- `s2hk` / `hk2s`：简体与香港繁体互转。",
        ],

        Consequences =
        [
            "• `iconv` 与 `opencc` 默认只输出结果，不修改原文件。",
            "• `dos2unix` 直接修改文件本身（保留原修改时间可加 `-k`）；重要文件先复制一份。",
            "• 源编码写错时 iconv 会报 `无法转换` 或产生新的乱码，换一个编码（GBK、GB18030、BIG5、SHIFT_JIS）再试。",
        ],

        Steps =
        [
            Step.Change(
                "准备一个 GBK 编码、Windows 换行的文件",
                "mkdir -p /tmp/enc-demo && cd /tmp/enc-demo && printf '中文测试\\r\\n第二行\\r\\n' | iconv -f UTF-8 -t GBK > old.txt && file old.txt",
                "模拟从旧 Windows 系统拿到的文本。").Then("`file` 显示类似 `ISO-8859 text, with CRLF line terminators`：编码未被识别为 UTF-8，且是 Windows 换行。").Practice(),
            Step.Read(
                "直接查看：乱码",
                "cat old.txt",
                "终端按 UTF-8 解读 GBK 字节。").Then("显示为无法辨认的乱码字符。"),
            Step.Change(
                "把 GBK 转为 UTF-8",
                "iconv -f GBK -t UTF-8 old.txt -o new.txt && cat new.txt",
                "`-f` 源编码、`-t` 目标编码、`-o` 输出文件。").Then("正常显示「中文测试」「第二行」。").Practice(),
            Step.Change(
                "把 Windows 换行改为 Linux 换行",
                "file new.txt; dos2unix new.txt; file new.txt",
                "dos2unix 直接修改文件。").Then("第一次 `file` 显示 `with CRLF line terminators`；dos2unix 输出 `dos2unix: 正在转换文件 new.txt 为Unix格式...`；第二次 `file` 只显示 `UTF-8 text`。").Practice(),
            Step.Read(
                "简体转繁体",
                "echo '软件开发需要内存和鼠标' | opencc -c s2t",
                "调用 OpenCC 字符映射表；`-c s2t` 仅做简繁单字字形转换，保留原始专业术语用词。").Then("输出 `軟件開發需要內存和鼠標`：字形变为繁体，但用词仍是大陆习惯。"),
            Step.Read(
                "简体转台湾正体并转换用词",
                "echo '软件开发需要内存和鼠标' | opencc -c s2twp",
                "`s2twp` 同时转换台湾常用词汇。").Then("输出 `軟體開發需要記憶體和滑鼠`。"),
            Step.Change(
                "把一个繁体文本文件转为简体",
                "echo '這是一段繁體文字' > tw.txt && opencc -c t2s -i tw.txt -o cn.txt && cat cn.txt",
                "`-i` 输入文件、`-o` 输出文件。").Then("输出 `这是一段繁体文字`，原文件 `tw.txt` 不变。").Practice(),
        ],

        Verify =
        [
            "1. `file -i new.txt` 显示 `charset=utf-8`。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/enc-demo` 清理。",
        ],
    };
}
