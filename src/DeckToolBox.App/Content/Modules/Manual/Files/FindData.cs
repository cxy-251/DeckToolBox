using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class FindData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-find",
        Title = "find：按条件查找文件",
        Keywords = "find -name · -type · -size · -mtime · -exec · -delete",
        Level = ModuleLevel.Intermediate,
        Overview = "find 从指定目录开始逐层遍历，按名称、类型、大小、修改时间等条件筛选文件，还能对找到的每个文件执行命令。它在所有 Linux 上都有，是写脚本时最可靠的文件查找工具。",

        Why =
        [
            "1. 写法：`find 起始目录 条件... 动作`。不写动作时默认 `-print`，即打印路径。",
            "2. 常用条件：",
            "- `-name '模式'`：按文件名匹配，区分大小写；`-iname` 不区分。模式必须加引号，防止 shell 提前展开。",
            "- `-type f` 普通文件、`-type d` 目录、`-type l` 符号链接。",
            "- `-size +100M` 大于 100 MB、`-size -1k` 小于 1 KB；`-empty` 空文件或空目录。",
            "- `-mtime -7` 7 天内修改过、`-mtime +30` 30 天以前修改的；`-mmin -60` 60 分钟内。",
            "- `-newer 文件` 比某个文件更新。",
            "- `-maxdepth N` 最多向下 N 层；`-xdev` 不进入其他文件系统。",
            "- 条件之间默认是「并且」；`-o` 表示「或者」，`!` 表示「非」。",
            "3. 动作：",
            "- `-exec 命令 {} +`：把找到的文件作为参数交给命令，`{}` 代表文件列表。",
            "- `-exec 命令 {} \\;`：对每个文件单独执行一次命令。",
            "- `-delete`：删除找到的文件。",
            "- `-printf`：自定义输出格式，如 `%s` 大小、`%p` 路径、`%TY-%Tm-%Td` 日期。",
        ],

        Consequences =
        [
            "• 只打印结果的 find 是只读的；从 `/` 或家目录开始遍历，文件很多时需要较长时间。",
            "• 遇到无权访问的目录会输出 `权限不够`，加 `2>/dev/null` 可以隐藏这些提示。",
            "• **`-delete` 和 `-exec rm` 会直接删除，没有确认。**先不带删除动作运行一遍，确认列表无误后再加上。`-delete` 必须写在所有条件之后。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习文件",
                "rm -rf /tmp/find-demo && mkdir -p /tmp/find-demo/sub && cd /tmp/find-demo && echo hi > a.txt && echo hi > sub/b.TXT && touch -d '40 days ago' old.log && touch empty.tmp && head -c 2M /dev/zero > big.bin",
                "先删除上次练习留下的目录，重复练习时结果才与下面的说明一致。`head -c 2M /dev/zero` 生成一个 2 MB 的文件。").Then("没有输出，当前目录变为 `/tmp/find-demo`。").Practice(),
            Step.Read(
                "按名称查找（忽略大小写）",
                "find . -iname '*.txt'",
                "`.` 表示从当前目录开始。").Then("输出 `./a.txt` 与 `./sub/b.TXT`。用 `-name` 则只找到 `./a.txt`。"),
            Step.Read(
                "只找目录",
                "find . -type d",
                "`-type d` 表示目录。").Then("输出 `.` 与 `./sub`。"),
            Step.Read(
                "按大小查找",
                "find . -type f -size +1M",
                "`+1M` 表示大于 1 MB。").Then("输出 `./big.bin`。"),
            Step.Read(
                "按修改时间查找",
                "find . -type f -mtime +30",
                "`+30` 表示 30 天以前修改的。").Then("输出 `./old.log`。"),
            Step.Read(
                "查找空文件",
                "find . -type f -empty",
                "大小为 0 的文件。").Then("输出 `./empty.tmp` 与 `./old.log`。"),
            Step.Read(
                "对找到的文件执行命令",
                "find . -type f -name '*.txt' -exec wc -c {} +",
                "`{}` 代表找到的文件，`+` 表示一次性传入全部。").Then("每个文件一行「字节数 文件名」，这里只匹配到 `3 ./a.txt`（`-name` 区分大小写，`b.TXT` 不匹配）；匹配到多个文件时最后一行是 `总计`。"),
            Step.Read(
                "自定义输出：大小与路径",
                "find . -type f -printf '%s\\t%p\\n' | sort -n",
                "`%s` 字节数、`%p` 路径、`\\t` 制表符。").Then("每行「字节数<Tab>路径」，从小到大排列，`big.bin` 在最后（2097152 字节）。"),
            Step.Danger(
                "删除找到的文件",
                "find . -name '*.tmp' -print -delete",
                "`-print` 先显示再删除。先去掉 `-delete` 运行一次确认列表。").Then("输出 `./empty.tmp`，该文件被永久删除。").Practice(),
        ],

        Verify =
        [
            "1. 再执行 `find . -name '*.tmp'`，没有输出。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/find-demo` 清理。",
        ],
    };
}
