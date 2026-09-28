using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class GrepData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-grep",
        Title = "grep：按内容搜索文本",
        Keywords = "grep -i · -n · -r · -v · -c · -E · -w · -A/-B",
        Level = ModuleLevel.Basic,
        Overview = "grep 在文件或其他命令的输出中查找包含指定文字的行。它是管道中最常见的一环：先用某个命令产生大量输出，再用 grep 只留下关心的那几行。",

        Why =
        [
            "1. 写法：`grep 模式 文件...`，或 `命令 | grep 模式`。模式默认是基本正则表达式；只找普通文字时加 `-F` 更安全（不把 `.`、`*` 当作特殊字符）。",
            "2. 常用选项：",
            "- `-i`：忽略大小写。",
            "- `-n`：显示行号。",
            "- `-v`：反向，只显示**不**匹配的行。",
            "- `-c`：只输出匹配的行数。",
            "- `-w`：整词匹配（`two` 不会匹配 `network`）。",
            "- `-r`：递归搜索目录；`-l` 只列出含匹配的文件名。",
            "- `-E`：扩展正则，可以用 `a|b` 表示「或」、`+`、`?`、`()` 等。",
            "- `-A 2` / `-B 2` / `-C 2`：同时显示匹配行之后 / 之前 / 前后的 2 行上下文。",
            "- `-o`：只输出匹配到的部分，而不是整行。",
            "3. 常用正则：`^` 行首、`$` 行尾、`.` 任意一个字符、`.*` 任意长度、`[0-9]` 一个数字、`[0-9]+`（需 `-E`）一个或多个数字。",
            "4. 退出码：找到为 0，没找到为 1，出错为 2。脚本中可写 `if grep -q 模式 文件; then ...`（`-q` 不输出，只看退出码）。",
        ],

        Consequences =
        [
            "• grep 只读取，不修改文件。",
            "• 模式中含空格、`|`、`*`、`$` 时要用单引号括起来，否则会被 shell 先处理。",
            "• 递归搜索整个家目录可能很慢；在大目录中搜索可改用 `rg`（见「rg 与 rga」）。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习文件",
                "mkdir -p /tmp/grep-demo && cd /tmp/grep-demo && printf 'Line one\\nline two error\\nERROR three\\nfour\\n' > log.txt",
                "一个四行的小文件。").Then("没有输出，当前目录变为 `/tmp/grep-demo`。"),
            Step.Read(
                "查找并显示行号",
                "grep -n error log.txt",
                "默认区分大小写。").Then("只输出 `2:line two error`，`ERROR three` 因大小写不同未被匹配。"),
            Step.Read(
                "忽略大小写",
                "grep -in error log.txt",
                "`-i` 忽略大小写。").Then("输出两行：`2:line two error` 与 `3:ERROR three`，匹配的文字高亮显示。"),
            Step.Read(
                "统计匹配行数 / 反向筛选",
                "grep -ic error log.txt; grep -iv error log.txt",
                "`-c` 计数，`-v` 取不匹配的行。").Then("先输出 `2`；再输出不含 error 的两行 `Line one` 与 `four`。"),
            Step.Read(
                "多个关键词之一",
                "grep -E 'one|four' log.txt",
                "`-E` 启用扩展正则，`|` 表示或。").Then("输出 `Line one` 与 `four`。"),
            Step.Read(
                "显示上下文",
                "grep -i -A1 three log.txt",
                "`-A1` 同时显示匹配行之后的 1 行。").Then("输出 `ERROR three` 和紧接着的 `four`。"),
            Step.Read(
                "在管道中过滤其他命令的输出",
                "ps -eo pid,comm | grep -i steam",
                "grep 最常见的用法。").Then("只留下进程名中含 steam 的行，如 `steam`、`steamwebhelper`，每行带 PID。"),
            Step.Read(
                "递归搜索目录，只列出文件名",
                "grep -rl 'PATH' ~/.bashrc ~/.bash_profile ~/.config/environment.d 2>/dev/null",
                "`-r` 递归，`-l` 只列文件名。").Then("列出内容中含 PATH 的文件路径，如 `/home/deck/.bashrc`。"),
            Step.Read(
                "只提取匹配的部分",
                "ip -4 addr show wlan0 | grep -oE 'inet [0-9.]+'",
                "`-o` 只输出匹配到的文字。").Then("输出一行 `inet 192.168.x.x`，而不是整行。"),
        ],

        Verify =
        [
            "1. `grep -q steam <(ps -e) && echo 找到` 在 Steam 运行时输出「找到」。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/grep-demo` 清理。",
        ],
    };
}
