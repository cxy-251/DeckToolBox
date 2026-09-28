using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class RgData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-rg",
        Title = "rg 与 rga：快速全文搜索",
        Keywords = "ripgrep · rg -i · -t · --hidden · rga · PDF · 压缩包",
        Level = ModuleLevel.Basic,
        Overview = "rg（ripgrep）是更快的递归版 grep，SteamOS 自带。rga（ripgrep-all）在它的基础上还能搜索 PDF、电子书、压缩包、Office 文档内部的文字。找「哪个文件里写过这句话」时，用它们最省事。",

        Why =
        [
            "1. rg 与 `grep -r` 的区别：",
            "- 默认递归当前目录，并行搜索，速度快得多。",
            "- 默认跳过隐藏文件、二进制文件和 `.gitignore` 中列出的内容；`--hidden` 包括隐藏文件，`-uuu` 完全不跳过。",
            "- 输出按文件分组，带行号与颜色。",
            "2. 常用选项（与 grep 大体一致）：",
            "- `-i` 忽略大小写（`-S` 智能大小写）、`-w` 整词、`-v` 反向、`-c` 计数、`-l` 只列文件名。",
            "- `-t py`、`-t sh`：只搜某类文件（`rg --type-list` 查看全部类型）；`-g '*.conf'`：按文件名模式筛选。",
            "- `-C 2`：显示上下文；`-F`：按普通文字而非正则匹配。",
            "3. rga：用法与 rg 相同，但会先把 PDF、epub、docx、zip、tar 等格式中的文字提取出来再搜索；首次处理某个文件较慢，结果会缓存在 `~/.cache/ripgrep-all`。",
            "4. 普通 rg 遇到压缩包等二进制文件时，只提示 `binary file matches` 而不显示内容，这时改用 rga。",
        ],

        Consequences =
        [
            "• rg、rga 只读，不修改被搜索的文件。",
            "• rga 会在 `~/.cache/ripgrep-all` 中写入缓存，可以随时删除。",
            "• 因为默认跳过隐藏文件，在 `~/.config` 这类目录中搜索时记得加 `--hidden`，或者直接指定该目录。",
        ],

        Steps =
        [
            Step.Read(
                "在一个目录中递归搜索",
                "rg -i steam ~/.config/autostart ~/Desktop 2>/dev/null",
                "不写目录时从当前目录开始。").Then("按文件分组输出：先是紫色的文件路径，下面是「行号:内容」，匹配文字高亮；没有匹配时没有输出。"),
            Step.Read(
                "包括隐藏文件搜索配置",
                "rg --hidden -l 'PROXY' ~ --max-depth 1",
                "`--hidden` 包括以 `.` 开头的文件，`-l` 只列文件名。").Then("列出家目录第一层中含 PROXY 的文件，如 `/home/deck/.bashrc`；不加 `--hidden` 则找不到。"),
            Step.Read(
                "只在指定的一组文件中搜索",
                "rg -l 'steamosctl' /usr/bin/steamos-*",
                "由 shell 展开 `steamos-*` 得到文件列表。`-t sh` 这类按类型筛选依赖扩展名，对 `/usr/bin` 中没有扩展名的脚本无效。").Then("列出调用了 steamosctl 的 SteamOS 脚本，如 `/usr/bin/steamos-session-select`。"),
            Step.Change(
                "准备一个压缩包，比较 rg 与 rga",
                "mkdir -p /tmp/rga-demo && cd /tmp/rga-demo && echo 'secret keyword inside' > inner.txt && zip -q pack.zip inner.txt && rm inner.txt",
                "把一个文本文件压进 zip。").Then("没有输出，目录中只剩 `pack.zip`。"),
            Step.Read(
                "用 rg 搜索压缩包",
                "rg keyword pack.zip",
                "rg 不解压。").Then("只提示 `binary file matches (found \"\\0\" byte ...)`，看不到具体内容。"),
            Step.Read(
                "用 rga 搜索压缩包内部",
                "rga keyword pack.zip",
                "rga 会先提取压缩包里的文字。").Then("输出 `inner.txt: secret keyword inside`：直接显示压缩包内哪个文件的哪一行匹配。"),
            Step.Read(
                "在 PDF 中搜索文字",
                "rga -i 'classified' /usr/share/cups/data/*.pdf",
                "系统自带的打印测试 PDF 可以用来练习；换成你自己的 PDF 目录同样适用。").Then("输出匹配的 PDF 文件名及 `Page N:` 开头的匹配行；首次搜索较慢，再次搜索使用缓存会很快。"),
        ],

        Verify =
        [
            "1. `rg --version` 与 `rga --version` 都能输出版本号。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/rga-demo` 清理。",
        ],
    };
}
