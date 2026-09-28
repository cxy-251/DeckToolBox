using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class HexStringsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-hex-strings",
        Title = "xxd、hexdump、strings：查看二进制",
        Keywords = "xxd · hexdump -C · strings · 十六进制",
        Level = ModuleLevel.Intermediate,
        Overview = "二进制文件不能用 cat 查看。xxd 和 hexdump 以十六进制逐字节显示内容，用于检查文件头、编码、隐藏字符；strings 从程序或数据文件中提取可读的文字片段，常用来快速了解一个陌生的二进制文件。",

        Why =
        [
            "1. 十六进制视图的三栏：左边是偏移量（第几个字节），中间是每个字节的十六进制值，右边是对应的可打印字符（不可打印的显示为 `.`）。",
            "2. xxd（随 vim 提供）：",
            "- `xxd 文件 | head`：查看开头。",
            "- `-l 64` 只显示前 64 字节，`-s 100` 从第 100 字节开始。",
            "- `-r` 可以把十六进制文本还原为二进制（用于简单的手工修改）。",
            "3. hexdump -C：经典的「规范」格式，每行 16 字节，右侧用 `|...|` 包住字符栏。",
            "4. 常见文件头（magic number）：`7f 45 4c 46` = ELF 程序，`50 4b 03 04` = zip，`89 50 4e 47` = png，`ef bb bf` = 带 BOM 的 UTF-8 文本。",
            "5. strings：扫描文件，输出所有长度不少于 4 的连续可打印字符序列；`-n 8` 改为至少 8 个字符。可以用来查看程序里的版本号、提示文字、配置项名称。",
        ],

        Consequences =
        [
            "• 查看是只读的。",
            "• `xxd -r` 生成的文件会覆盖同名目标，修改二进制文件前先备份。",
        ],

        Steps =
        [
            Step.Read(
                "查看一小段文字的字节",
                "printf 'Hi\\n中' | xxd",
                "英文字母各 1 字节，换行符是 `0a`，一个汉字在 UTF-8 中占 3 字节。").Then("输出一行 `00000000: 4869 0ae4 b8ad  Hi....`：`48 69` 是 Hi，`0a` 是换行，`e4 b8 ad` 是「中」。"),
            Step.Read(
                "用 hexdump 的规范格式查看",
                "printf 'Hi\\n' | hexdump -C",
                "`-C` 输出十六进制与字符对照。").Then("输出 `00000000  48 69 0a  |Hi.|` 与表示总长度的 `00000003`。"),
            Step.Read(
                "查看程序文件的文件头",
                "xxd -l 16 /usr/bin/ls",
                "`-l 16` 只看前 16 字节。").Then("以 `7f45 4c46` 开头，右侧显示 `.ELF`：这就是 file 命令识别出「ELF 可执行程序」的依据。"),
            Step.Read(
                "检查文本是否带 BOM 或 Windows 换行",
                "printf '\\xef\\xbb\\xbfA\\r\\n' | xxd",
                "模拟一个来自 Windows 记事本的文件。").Then("输出中可见开头的 `efbb bf`（BOM）以及结尾的 `0d0a`（Windows 的 CRLF 换行）。"),
            Step.Read(
                "从程序中提取可读文字",
                "strings /usr/bin/steamosctl | grep -m 5 -i tdp",
                "`grep -m 5` 最多显示 5 行。").Then("输出程序中与 TDP 相关的字符串片段，如 `get-tdp-limit-max`，可以看出它支持哪些子命令。"),
        ],

        Verify =
        [
            "1. `printf 'A' | xxd -p` 输出 `41`，即字母 A 的十六进制值。",
        ],
    };
}
