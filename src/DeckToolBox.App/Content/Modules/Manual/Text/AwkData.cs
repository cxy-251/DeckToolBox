using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class AwkData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-awk",
        Title = "awk：按列处理与统计",
        Keywords = "awk '{print $1}' · -F · NR · 条件 · END · 数组求和",
        Level = ModuleLevel.Intermediate,
        Overview = "awk 把每一行自动切分成列，可以按条件筛选、重新排列列、做加法和分组统计。像 ps、df、ls -l 这类表格式的输出，用 awk 处理最方便。",

        Why =
        [
            "1. 基本结构：`awk '条件 { 动作 }' 文件`。对每一行，条件成立时执行动作；省略条件表示每行都执行，省略动作表示打印整行。",
            "2. 内置变量：",
            "- `$1`、`$2`…：第 1、2… 列；`$0`：整行；`$NF`：最后一列。",
            "- `NR`：当前行号；`NF`：当前行的列数。",
            "- `-F ':'`：指定分隔符，默认按空白（空格和制表符）切分。",
            "3. 条件写法：`$3 > 100`、`$1 == \"root\"`、`/正则/`、`NR > 1`（跳过表头），可用 `&&`、`||` 组合。",
            "4. `BEGIN { }` 在读入任何行之前执行（如打印表头），`END { }` 在全部读完后执行（如输出合计）。",
            "5. 数组：`m[$2] += $1` 以第 2 列为键累加第 1 列，常用于分组求和；`for (k in m)` 遍历。",
            "6. `printf`：格式化输出，`%s` 字符串、`%d` 整数、`%.1f` 保留 1 位小数、`%-10s` 左对齐宽 10。",
        ],

        Consequences =
        [
            "• awk 只读取输入并输出结果，不会修改文件。",
            "• 规则要放在单引号中；awk 内部的 `$1` 等是 awk 的列，不是 shell 变量。",
            "• 按空白切分时，含空格的字段（如文件名「My Games」）会被拆成两列；此时换用其他分隔符或工具。",
        ],

        Steps =
        [
            Step.Read(
                "打印指定列",
                "df -h | awk '{print $6, $4}'",
                "df 的第 6 列是挂载点，第 4 列是可用空间。").Then("每行两个值，如 `/home 33G`；第一行是表头 `挂载点 可用`。"),
            Step.Read(
                "按条件筛选行",
                "df -h | awk 'NR > 1 && $5+0 > 50 {print $6, $5}'",
                "`NR > 1` 跳过表头，`$5+0` 把「82%」转成数字。").Then("只输出使用率超过 50% 的分区，如 `/ 82%`、`/home 93%`。"),
            Step.Read(
                "指定分隔符",
                "awk -F: '$3 >= 1000 && $3 < 60000 {print $1, $6}' /etc/passwd",
                "`/etc/passwd` 以冒号分隔，第 3 列是 UID，第 6 列是家目录。").Then("输出普通用户账户，如 `deck /home/deck`。"),
            Step.Read(
                "求和：统计文件总字节数",
                "ls -l /usr/bin | awk 'NR > 1 {sum += $5} END {printf \"%.1f MB\\n\", sum/1024/1024}'",
                "`END` 在处理完所有行后执行。").Then("输出一个总大小，如 `612.3 MB`（`/usr/bin` 中所有文件大小之和）。"),
            Step.Read(
                "分组统计：每个程序占用的总内存",
                "ps -eo rss,comm --no-headers | awk '{m[$2] += $1} END {for (k in m) printf \"%8.1f MB  %s\\n\", m[k]/1024, k}' | sort -rn | head",
                "以进程名为键累加 RSS（KB）。多进程程序（如浏览器）会被合并计算。").Then("10 行「内存 程序名」，按占用从大到小，如 `2399.2 MB  chrome`。"),
            Step.Read(
                "格式化成对齐的表格",
                "awk -F: 'BEGIN {printf \"%-16s %s\\n\", \"用户\", \"Shell\"} $7 !~ /nologin/ {printf \"%-16s %s\\n\", $1, $7}' /etc/passwd",
                "`BEGIN` 打印表头，`!~` 表示不匹配正则。").Then("第一行是表头，下面是能登录的账户及其 shell，两列左对齐。"),
        ],

        Verify =
        [
            "1. `echo 'a b c' | awk '{print $NF, NF}'` 输出 `c 3`：最后一列与列数。",
        ],
    };
}
