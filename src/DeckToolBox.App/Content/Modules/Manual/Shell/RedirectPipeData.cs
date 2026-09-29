using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class RedirectPipeData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-redirect-pipe",
        Title = "重定向、管道与 tee",
        Keywords = "> · >> · 2> · 2>&1 · | · < · <<< · <( ) · tee",
        Level = ModuleLevel.Basic,
        Overview = "每个命令都有三个标准数据流：输入、输出、错误输出。重定向把它们接到文件，管道把一个命令的输出接到另一个命令的输入，tee 则让输出同时去往屏幕和文件。几乎所有命令组合都建立在这几个符号之上。",

        Why =
        [
            "1. 三个标准流及其编号：",
            "- 0 标准输入 (stdin)：默认来自键盘。",
            "- 1 标准输出 (stdout)：正常结果，默认显示在终端。",
            "- 2 标准错误 (stderr)：错误和提示信息，默认也显示在终端，但与标准输出是分开的两条通道。",
            "2. 输出重定向：",
            "- `> 文件`：标准输出写入文件，**文件原有内容被清空**；`>> 文件`：追加到文件末尾。",
            "- `2> 文件`：只把错误写入文件；`2>/dev/null`：丢弃错误（`/dev/null` 是一个吞掉一切写入的特殊文件）。",
            "- `> 文件 2>&1` 或 `&> 文件`：输出和错误都写入同一个文件。`2>&1` 表示「把 2 号流接到 1 号流当前指向的地方」，所以必须写在 `>` 之后。",
            "3. 输入重定向：`< 文件` 从文件读取输入；`<<< \"字符串\"` 直接把一个字符串作为输入；`<< EOF ... EOF`（here document）把多行文字作为输入，本应用中写配置文件的步骤常用它。",
            "4. 管道 `|`：把左边命令的**标准输出**接到右边命令的标准输入。错误输出不经过管道，除非写成 `2>&1 |`。",
            "5. 进程替换 `<(命令)`：把一个命令的输出当作一个临时文件名，交给需要文件参数的命令，如 `diff <(命令1) <(命令2)`。",
            "6. tee：从输入读取，同时写到屏幕和文件；`tee -a` 追加。需要以 root 身份写文件时常用 `命令 | sudo tee 文件`，因为 `sudo 命令 > 文件` 中的重定向是由当前普通用户的 shell 完成的。",
        ],

        Consequences =
        [
            "• **`>` 会立即清空目标文件**，即使后面的命令失败也一样。追加时一定写 `>>`。",
            "• 不要把一个文件同时作为输入和输出（`sort a.txt > a.txt`），文件会在读取前被清空。",
            "• `set -o noclobber` 可以让 `>` 拒绝覆盖已存在的文件，需要覆盖时写 `>|`。",
        ],

        Steps =
        [
            Step.Change(
                "把输出保存到文件",
                "mkdir -p /tmp/rd-demo && cd /tmp/rd-demo && ls / > list.txt && wc -l list.txt",
                "`>` 创建或覆盖文件，终端上不再显示 ls 的结果。").Then("终端只显示 `wc` 的结果，如 `20 list.txt`：ls 的输出全部进了文件。"),
            Step.Change(
                "追加而不是覆盖",
                "date >> list.txt && tail -n 2 list.txt",
                "`>>` 追加到末尾。").Then("最后两行是 ls 的最后一项和刚追加的日期时间。"),
            Step.Change(
                "分开保存正常输出与错误",
                "ls / /不存在 > out.txt 2> err.txt; echo \"== out\"; head -n 2 out.txt; echo \"== err\"; cat err.txt",
                "`2>` 只重定向错误输出。").Then("`out.txt` 中是根目录的列表；`err.txt` 中是 `ls: 无法访问 '/不存在': 没有那个文件或目录`。"),
            Step.Change(
                "输出和错误写入同一个文件",
                "ls / /不存在 > all.txt 2>&1; head -n 3 all.txt",
                "`2>&1` 必须写在 `> 文件` 之后。").Then("文件开头是错误提示，随后是 `/:` 和目录列表：两路输出都进了同一个文件。"),
            Step.Read(
                "丢弃错误信息",
                "find / -maxdepth 3 -name 'steam*' 2>/dev/null",
                "去掉 `2>/dev/null` 再执行一次对比：会多出几十行「权限不够」。").Then("只输出找到的路径，如 `/etc/steamos-release`、`/usr/bin/steam`，没有权限错误的干扰行。"),
            Step.Change(
                "同时显示并保存：tee",
                "ls /usr/bin | wc -l | tee count.txt; cat count.txt",
                "tee 把输入同时写到屏幕和文件。").Then("屏幕上显示数字（如 `2858`），`count.txt` 中也保存了同一个数字。"),
            Step.Read(
                "把字符串或多行文字作为输入",
                "tr a-z A-Z <<< 'hello'; cat << EOF\n第一行\n用户是 $USER\nEOF",
                "`<<<` 单个字符串；`<< EOF` 到单独一行的 `EOF` 之间是多行输入，其中的变量会被展开（写成 `<< 'EOF'` 则不展开）。").Then("先输出 `HELLO`，再输出两行：`第一行`、`用户是 deck`。"),
            Step.Read(
                "用进程替换比较两个命令的输出",
                "diff <(ls /usr/bin | sed -n 1,5p) <(ls /usr/bin | sed -n 1,6p)",
                "`<( )` 把命令输出当作文件。").Then("输出 `5a6` 和 `> ...`：第二个列表比第一个多出一行。"),
        ],

        Verify =
        [
            "1. `echo 测试 > /tmp/rd-demo/x.txt; echo 再次 > /tmp/rd-demo/x.txt; cat /tmp/rd-demo/x.txt` 只输出「再次」，证实 `>` 会覆盖。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/rd-demo` 清理。",
        ],
    };
}
