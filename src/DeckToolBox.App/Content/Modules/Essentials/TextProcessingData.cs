using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class TextProcessingData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "text-processing",
        Title = "文本处理：sort、cut、sed、awk",
        Keywords = "sort · uniq · cut · tr · sed · awk · wc",
        Level = ModuleLevel.Intermediate,
        Overview = "Linux 的命令输出、配置文件、日志几乎都是纯文本。掌握几个小工具，用管道串起来，就能完成筛选、统计、提取、替换这类工作，不需要写程序。",

        Why =
        [
            "1. 按「行」处理的小工具：",
            "- `sort`：排序。`-n` 按数字，`-h` 按 K/M/G 大小，`-r` 倒序，`-k2` 按第 2 列。",
            "- `uniq`：合并相邻的重复行，`-c` 计数。必须先 `sort`，让相同的行相邻。",
            "- `wc -l`：统计行数；`head` / `tail`：取开头 / 结尾若干行。",
            "2. 按「列」处理：",
            "- `cut -d: -f1,7`：以 `:` 为分隔符，取第 1、7 列。适合分隔符固定的数据（如 `/etc/passwd`）。",
            "- `awk`：以空白为默认分隔符，`$1`、`$2` 表示第 1、2 列，`NR` 是行号，可以写条件和计算，例如 `awk '$5+0 > 80 {print $6}'`。",
            "- `column -t`：把参差不齐的输出对齐成表格。",
            "3. 替换与转换：",
            "- `tr`：逐字符替换，例如 `tr ':' '\\n'` 把冒号换成换行。",
            "- `sed 's/旧/新/g'`：按正则替换。`sed -n 's/.../.../p'` 只打印替换成功的行，常用来从文件中提取某个值。",
            "4. 组合的思路：先用 `grep` 缩小范围，再用 `cut`/`awk` 取列，最后 `sort | uniq -c | sort -rn` 统计排名。",
        ],

        Consequences =
        [
            "• 这些工具默认只读取输入、输出到屏幕，不会修改原文件。",
            "• `sed -i` 会**直接修改文件**，没有撤销；先不带 `-i` 运行确认结果，或用 `sed -i.bak` 自动保留备份。",
            "• awk 与 sed 的规则放在单引号里，避免 shell 先展开其中的 `$` 和 `*`。",
        ],

        Steps =
        [
            Step.Read(
                "统计：哪些程序的进程最多",
                "ps -eo comm= | sort | uniq -c | sort -rn | head",
                "经典的「计数排名」组合：`sort | uniq -c | sort -rn`。").Then("10 行「数量 程序名」，如 `17 chrome`、`10 steamwebhelper`，数量最多的在最前。"),
            Step.Read(
                "取列：列出所有账户及其登录 shell",
                "cut -d: -f1,7 /etc/passwd | column -t -s:",
                "`cut -d: -f1,7` 取第 1、7 列；`column -t -s:` 按冒号分列并对齐。").Then("两列对齐的表格：账户名和它的 shell，如 `deck /bin/bash`、`root /bin/bash`；系统账户多为 `/usr/bin/nologin`。"),
            Step.Read(
                "条件筛选：使用率超过 80% 的分区",
                "df -h | awk 'NR>1 && $5+0 > 80 {print $6, $5}'",
                "`NR>1` 跳过表头；`$5+0` 把「82%」转成数字 82。").Then("每行「挂载点 使用率」，如 `/home 92%`；没有超过 80% 的分区时没有输出。"),
            Step.Read(
                "提取：从配置文件中取出某个值",
                "sed -n 's/^PRETTY_NAME=\"\\(.*\\)\"/\\1/p' /etc/os-release",
                "`\\(...\\)` 捕获内容，`\\1` 引用它；`-n` 与末尾的 `p` 配合，只输出匹配的行。").Then("输出一行 `SteamOS`（引号已被去掉）。"),
            Step.Read(
                "日志统计：本次开机出现最多的警告",
                "journalctl -b -p warning -o cat --no-pager | cut -c1-60 | sort | uniq -c | sort -rn | sed -n '1,10p'",
                "`-o cat` 去掉时间戳等前缀，相同的消息才能被合并计数。").Then("10 行「次数 消息开头」，出现次数最多的警告排在最前，可以快速看出哪个程序在反复报错。"),
            Step.Change(
                "安全地批量替换文件内容",
                "sed -i.bak 's/<旧文本>/<新文本>/g' <文件>",
                "`s/旧/新/g` 中的 `g` 表示每行所有匹配都替换。确认无误后再删除备份。").Then("没有输出。文件中所有「旧文本」被替换为「新文本」，同目录下多出 `<文件>.bak`（修改前的原样）。"),
        ],

        Verify =
        [
            "1. 第 3 步列出的分区与 `df -h` 中「已用%」大于 80 的行一致。",
            "2. 执行替换后，`diff <文件>.bak <文件>` 只显示你想改的那些行。",
        ],
    };
}
