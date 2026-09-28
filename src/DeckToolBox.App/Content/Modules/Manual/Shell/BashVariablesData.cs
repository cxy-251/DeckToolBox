using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class BashVariablesData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-bash-variables",
        Title = "bash：变量、引号与展开",
        Keywords = "变量 · \"$var\" · '单引号' · $( ) · $(( )) · ${ } · 通配符 · {a,b}",
        Level = ModuleLevel.Basic,
        Overview = "在 bash 中，一条命令在执行前会先经过多种「展开」：变量被替换成值、$( ) 被替换成命令输出、* 被替换成文件列表。引号决定哪些展开会发生。很多「命令看起来对却出错」的问题，都出在引号和展开上。",

        Why =
        [
            "1. 变量：",
            "- 赋值：`name=值`，**等号两边不能有空格**。",
            "- 使用：`$name` 或 `${name}`。",
            "- `export name=值` 让子进程（之后运行的程序）也能看到它。",
            "2. 三种引号：",
            "- 双引号 `\"...\"`：展开变量和 `$( )`，但保持内容为一个整体参数，不做空格切分和通配符展开。**使用变量时几乎总应加双引号**：`\"$file\"`。",
            "- 单引号 `'...'`：原样保留，什么都不展开。",
            "- 不加引号：变量值中的空格会把它切成多个参数，`*` 会被展开成文件名。",
            "3. 替换与计算：",
            "- `$(命令)`：替换为命令的输出，如 `echo \"今天是 $(date +%F)\"`。",
            "- `$((表达式))`：整数运算，如 `$((3*4+1))`。bash 不支持小数运算，需要小数时用 `qalc` 或 `awk`。",
            "4. 参数展开 `${ }` 的常用形式：",
            "- `${#var}`：长度。",
            "- `${var:-默认}`：变量为空时使用默认值。",
            "- `${var/旧/新}`：替换第一个匹配；`${var//旧/新}` 替换全部。",
            "- `${var%.*}` 去掉扩展名、`${var##*/}` 取文件名（见「路径处理与批量重命名」）。",
            "5. 通配符（由 shell 展开成文件名）：`*` 任意长度、`?` 一个字符、`[abc]` 其中之一。没有匹配时保持原样。",
            "6. 花括号展开：`{a,b,c}` 与 `{1..5}` 生成一组词，不管文件是否存在。",
        ],

        Consequences =
        [
            "• 在终端设置的变量只存在于当前 shell，关闭终端就消失；需要长期保留的写入 `~/.bashrc`（见「环境变量与 Shell 配置」）。",
            "• 变量不加双引号是 shell 脚本中最常见的错误来源：文件名含空格时，`rm $f` 可能删除错误的文件。",
        ],

        Steps =
        [
            Step.Read(
                "设置并使用变量，比较三种引号",
                "name=\"Steam Deck\"; echo $name; echo \"$name\"; echo '$name'",
                "注意等号两边没有空格。").Then("前两行都输出 `Steam Deck`，第三行输出字面的 `$name`：单引号内不展开变量。"),
            Step.Read(
                "引号对空格的影响",
                "f=\"my file.txt\"; touch /tmp/\"$f\"; ls /tmp/$f; ls /tmp/\"$f\"",
                "第一个 ls 没有给变量加引号。").Then("第一个 ls 报两条错误（找不到 `/tmp/my` 和 `file.txt`），因为值被空格拆成了两个参数；第二个 ls 正确列出 `/tmp/my file.txt`。"),
            Step.Read(
                "命令替换与整数运算",
                "echo \"今天是 $(date +%F)，共有 $(ls /usr/bin | wc -l) 个命令，3*4+1=$((3*4+1))\"",
                "`$( )` 替换为命令输出，`$(( ))` 整数计算。").Then("输出一行，如 `今天是 2026-09-28，共有 2858 个命令，3*4+1=13`。"),
            Step.Read(
                "参数展开：长度、默认值、替换",
                "name=\"Steam Deck\"; echo ${#name}; echo \"${missing:-默认值}\"; echo \"${name/Deck/OS}\"",
                "`missing` 是一个从未设置的变量。").Then("依次输出 `10`、`默认值`、`Steam OS`。"),
            Step.Read(
                "通配符与花括号展开",
                "echo /usr/bin/steamos-s*; echo 备份-{1..3}.tar",
                "通配符匹配已存在的文件，花括号只是生成文字。").Then("第一行列出 `/usr/bin/steamos-select-branch`、`steamos-session-select` 等匹配的文件；第二行输出 `备份-1.tar 备份-2.tar 备份-3.tar`。"),
            Step.Read(
                "让子程序看到变量：export",
                "demo=1; bash -c 'echo \"子进程看到: [$demo]\"'; export demo; bash -c 'echo \"子进程看到: [$demo]\"'",
                "`bash -c` 启动一个子 shell。").Then("第一行 `子进程看到: []`（未 export 时子进程看不到），第二行 `子进程看到: [1]`。"),
        ],

        Verify =
        [
            "1. `a=1 ; echo \"$a\"` 输出 1；而 `a = 1` 会报错「未找到命令」，因为 bash 把 `a` 当成了命令名。",
        ],
    };
}
