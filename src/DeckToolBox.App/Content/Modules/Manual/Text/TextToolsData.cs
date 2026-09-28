using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class TextToolsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-text-tools",
        Title = "sort、uniq、cut、tr、wc 等小工具",
        Keywords = "sort -n -h -k · uniq -c · cut -d -f · tr · wc · column · paste",
        Level = ModuleLevel.Basic,
        Overview = "coreutils 提供了一组各做一件小事的文本工具：排序、去重计数、取列、字符替换、统计行数、对齐成表格、按列拼接。它们单独用很简单，用管道组合起来就能完成大部分文本统计工作。",

        Why =
        [
            "1. sort：",
            "- 默认按字符排序；`-n` 按数字、`-h` 按 K/M/G 大小、`-r` 倒序。",
            "- `-k 2` 按第 2 列排序；`-t ,` 指定分隔符；`-u` 排序并去重。",
            "2. uniq：合并**相邻**的重复行，所以通常先 sort。`-c` 在每行前加出现次数，`-d` 只显示重复的行。",
            "3. cut：按分隔符取列，`-d ',' -f 1,3` 取第 1、3 列；`-c 1-10` 取每行前 10 个字符。",
            "4. tr：逐字符转换，只读标准输入。`tr a-z A-Z` 转大写、`tr -d ' '` 删除空格、`tr -s ' '` 压缩连续空格、`tr ':' '\\n'` 冒号换成换行。",
            "5. wc：`-l` 行数、`-w` 词数、`-c` 字节数、`-m` 字符数（中文一个字 3 字节、1 个字符）。",
            "6. column -t：把参差不齐的列对齐；`-s ','` 指定分隔符。",
            "7. paste：把多个文件按行并排拼接，`-d` 指定连接符。",
        ],

        Consequences =
        [
            "• 这些命令都只输出结果，不修改输入文件。",
            "• `sort 文件 > 文件` 会先清空文件导致内容丢失；要原地排序用 `sort -o 文件 文件`。",
        ],

        Steps =
        [
            Step.Change(
                "准备一个 CSV 文件",
                "mkdir -p /tmp/tt-demo && cd /tmp/tt-demo && printf 'name,game,hours\\ntom,Terraria,120\\namy,Celeste,35\\ntom,Celeste,8\\nbob,Terraria,300\\n' > play.csv",
                "一个记录游戏时长的小表格。").Then("没有输出，当前目录变为 `/tmp/tt-demo`。"),
            Step.Read(
                "对齐成表格查看",
                "column -t -s, play.csv",
                "`-s,` 以逗号为分隔符。").Then("五行对齐的三列表格，表头为 `name game hours`。"),
            Step.Read(
                "取出某一列",
                "cut -d, -f2 play.csv",
                "`-d,` 分隔符，`-f2` 第 2 列。").Then("输出 `game`、`Terraria`、`Celeste`、`Celeste`、`Terraria`。"),
            Step.Read(
                "按数字列倒序排序",
                "tail -n +2 play.csv | sort -t, -k3 -n -r",
                "`tail -n +2` 去掉表头；`-k3 -n -r` 按第 3 列数字倒序。").Then("第一行是 `bob,Terraria,300`，最后一行是 `tom,Celeste,8`。"),
            Step.Read(
                "统计每个游戏出现的次数",
                "tail -n +2 play.csv | cut -d, -f2 | sort | uniq -c | sort -rn",
                "经典组合：取列 → 排序 → 计数 → 按次数排序。").Then("输出 `2 Terraria` 与 `2 Celeste` 两行。"),
            Step.Read(
                "字符转换",
                "head -n 1 play.csv | tr a-z A-Z | tr ',' '\\t'",
                "先转大写，再把逗号换成制表符。").Then("输出 `NAME	GAME	HOURS`，以制表符分隔。"),
            Step.Read(
                "统计行数、词数、字符数",
                "wc -l play.csv; echo '中文ab' | wc -c -m",
                "`-c` 字节数，`-m` 字符数。").Then("第一行 `5 play.csv`；第二行 `5 9`（左边是字符数 5：两个汉字、两个字母、一个换行；右边是字节数 9，每个汉字占 3 字节）。"),
            Step.Read(
                "按列拼接两个列表",
                "paste -d: <(cut -d, -f1 play.csv) <(cut -d, -f3 play.csv)",
                "`<( )` 把命令输出当作文件使用。").Then("输出 `name:hours`、`tom:120` 等五行。"),
        ],

        Verify =
        [
            "1. `awk -F, 'NR>1{s+=$3} END{print s}' play.csv` 输出总时长 `463`，与四个数字相加的结果一致。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/tt-demo` 清理。",
        ],
    };
}
