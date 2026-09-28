using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class BashScriptData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-bash-script",
        Title = "bash 脚本：条件、循环与退出码",
        Keywords = "#!/bin/bash · if · [[ ]] · for · while · $? · && || · set -euo pipefail",
        Level = ModuleLevel.Intermediate,
        Overview = "把一串命令写进文件，加上条件判断和循环，就是一个 shell 脚本。本模块从退出码讲起，写一个能备份指定目录、会检查错误的小脚本，涵盖日常自动化需要的全部基本语法。",

        Why =
        [
            "1. 退出码：每条命令结束时返回一个 0~255 的数字，0 表示成功，非 0 表示失败。`$?` 保存上一条命令的退出码。",
            "2. 按结果连接命令：",
            "- `命令1 && 命令2`：1 成功才执行 2。",
            "- `命令1 || 命令2`：1 失败才执行 2。",
            "- `命令1 ; 命令2`：无论成败都依次执行。",
            "3. 条件判断：`if 命令; then ...; elif ...; else ...; fi`，根据命令的退出码选择分支。`[[ ]]` 是专门的测试命令：",
            "- 文件：`-f` 普通文件存在、`-d` 目录存在、`-e` 任意存在、`-x` 可执行、`-s` 非空。",
            "- 字符串：`==`、`!=`、`-z`（为空）、`-n`（非空）；`==` 右侧可用通配符，如 `[[ $f == *.txt ]]`。",
            "- 数字：`-eq` `-ne` `-lt` `-le` `-gt` `-ge`（或在 `(( ))` 中直接用 `<`、`>`）。",
            "4. 循环：",
            "- `for f in *.txt; do ...; done`：遍历文件或列表。",
            "- `for i in {1..5}` 或 `for ((i=0; i<5; i++))`：计数循环。",
            "- `while 条件; do ...; done`；`while read -r line; do ...; done < 文件`：逐行读取文件。",
            "5. 脚本文件：",
            "- 第一行 `#!/bin/bash`（shebang）指定解释器。",
            "- `chmod +x 脚本` 后用 `./脚本` 运行，或直接 `bash 脚本`。",
            "- `$1`、`$2` 是传入的参数，`$#` 是参数个数，`$@` 是全部参数。",
            "- `set -euo pipefail` 放在开头：任一命令失败即退出 (`-e`)、使用未定义变量报错 (`-u`)、管道中任一环节失败都算失败 (`pipefail`)，能避免脚本在出错后继续执行造成更大问题。",
        ],

        Consequences =
        [
            "• 脚本以你的身份运行，能做你在终端里能做的一切，包括删除文件。运行别人给的脚本前先阅读它。",
            "• 在 Windows 上编辑过的脚本会带有 CRLF 换行，运行时报 `$'\\r': 未找到命令`，用 `dos2unix` 转换即可。",
            "• 脚本中的 `cd` 只影响脚本自己的进程，脚本结束后你的终端仍在原目录。",
        ],

        Steps =
        [
            Step.Read(
                "观察退出码与 && ||",
                "ls ~ > /dev/null; echo \"成功时: $?\"; ls /不存在 2>/dev/null; echo \"失败时: $?\"; ls /不存在 2>/dev/null && echo 执行了 || echo 前一条失败了",
                "`$?` 只保存紧挨着的上一条命令的退出码。").Then("输出 `成功时: 0`、`失败时: 2`、`前一条失败了`。"),
            Step.Read(
                "条件判断",
                "f=~/.bashrc; if [[ -f $f ]]; then echo \"$f 存在，大小 $(stat -c %s \"$f\") 字节\"; else echo 不存在; fi",
                "`-f` 判断普通文件是否存在。").Then("输出 `/home/deck/.bashrc 存在，大小 695 字节`（数字随文件而变）。"),
            Step.Read(
                "for 循环遍历文件",
                "files=(/etc/*.conf); for f in \"${files[@]:0:5}\"; do echo \"$(basename \"$f\"): $(wc -l < \"$f\") 行\"; done",
                "`files=( )` 把通配符结果存成数组，`${files[@]:0:5}` 取前 5 个；`\"$f\"` 加引号，文件名含空格也安全。").Then("输出 5 行，如 `pacman.conf: 102 行`。"),
            Step.Read(
                "逐行读取文件",
                "while IFS=: read -r user _ uid _; do (( uid >= 1000 && uid < 60000 )) && echo \"普通用户: $user\"; done < /etc/passwd",
                "`IFS=:` 以冒号切分每行，`_` 表示忽略该字段。").Then("输出 `普通用户: deck`。"),
            Step.Change(
                "写一个带参数和错误检查的备份脚本",
                "mkdir -p ~/bin && cat > ~/bin/backup-dir.sh <<'EOF'\n#!/bin/bash\nset -euo pipefail\n\nif [[ $# -ne 1 ]]; then\n    echo \"用法: $0 要备份的目录\" >&2\n    exit 1\nfi\n\nsrc=\"$1\"\nif [[ ! -d $src ]]; then\n    echo \"错误: $src 不是目录\" >&2\n    exit 2\nfi\n\nout=\"$HOME/backup-$(basename \"$src\")-$(date +%F).tar.zst\"\ntar -caf \"$out\" -C \"$(dirname \"$src\")\" \"$(basename \"$src\")\"\necho \"已备份到 $out ($(du -h \"$out\" | cut -f1))\"\nEOF\nchmod +x ~/bin/backup-dir.sh",
                "`>&2` 把提示写到标准错误；`exit 1` 以失败状态结束。").Then("没有输出，生成可执行的 `~/bin/backup-dir.sh`。"),
            Step.Read(
                "测试脚本的错误处理",
                "~/bin/backup-dir.sh; echo \"退出码 $?\"; ~/bin/backup-dir.sh /不存在; echo \"退出码 $?\"",
                "分别测试「没有参数」和「目录不存在」。").Then("先输出用法提示与 `退出码 1`，再输出 `错误: /不存在 不是目录` 与 `退出码 2`。"),
            Step.Change(
                "正常运行脚本",
                "~/bin/backup-dir.sh ~/Documents",
                "备份 Documents 目录。").Then("输出 `已备份到 /home/deck/backup-Documents-<日期>.tar.zst (大小)`，家目录中出现该压缩包。"),
        ],

        Verify =
        [
            "1. `tar -tf ~/backup-Documents-*.tar.zst | head` 能列出 `Documents/` 下的文件。",
            "2. 不需要时删除：`rm ~/bin/backup-dir.sh ~/backup-Documents-*.tar.zst`。",
        ],
    };
}
