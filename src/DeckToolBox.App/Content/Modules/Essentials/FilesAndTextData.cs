using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class FilesAndTextData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "files-and-text",
        Title = "文件、搜索与管道",
        Keywords = "cp · mv · rm · find · grep · | · >",
        Level = ModuleLevel.Basic,
        Overview = "复制、移动、删除文件，按名称找文件，在文件内容里搜索文字，以及用管道把命令串起来——这几样组合起来，能解决大部分日常问题。",

        Why =
        [
            "1. 文件操作四件套：",
            "- `cp 源 目标`：复制；复制目录要加 `-r`。",
            "- `mv 源 目标`：移动，也用于重命名。",
            "- `mkdir -p 路径`：创建目录，`-p` 自动创建中间目录。",
            "- `rm 文件`：删除；删除目录要 `-r`。**不经过回收站，无法恢复。**",
            "2. 通配符：`*` 匹配任意多个字符，`?` 匹配一个字符。`*.png` 表示当前目录所有 png 文件。通配符由 shell 展开，先用 `ls *.png` 看看会匹配哪些文件，再交给 `rm`。",
            "3. 找文件与找内容：",
            "- `find`：按名称、大小、时间找文件，任何 Linux 上都有。",
            "- `grep`：在文件内容里搜索文字。",
            "- SteamOS 还自带更快更好用的 `fd`（找文件）和 `rg`（ripgrep，搜内容），默认跳过隐藏文件和 `.gitignore` 中的内容。",
            "4. 管道与重定向：",
            "- `命令1 | 命令2`：把前一个命令的输出交给后一个处理，例如 `ls | wc -l` 统计文件数。",
            "- `> 文件` 把输出写入文件（**会覆盖**），`>> 文件` 追加到末尾，`2>/dev/null` 丢弃错误信息。",
            "5. 查看文件：`cat` 直接输出全部，`less` 分页浏览（`/关键词` 搜索、`q` 退出），`head`/`tail` 看开头/结尾，`tail -f` 持续跟踪新增内容（看日志常用）。编辑文本可以用 `nano`（新手友好，底部有快捷键提示）或 `vim`。",
        ],

        Consequences =
        [
            "• `cp`、`mv` 遇到同名目标会**直接覆盖**。加 `-i` 会在覆盖前询问，加 `-n` 从不覆盖。",
            "• `rm -r` 会删除整个目录树。用通配符删除前，先用同样的通配符执行 `ls` 确认。",
            "• `>` 会清空目标文件再写入，把 `>>` 误写成 `>` 会丢失原内容。",
        ],

        Steps =
        [
            Step.Change(
                "建一个练习目录并创建几个文件",
                "mkdir -p ~/linux-lab/sub && cd ~/linux-lab && echo 'hello linux' > a.txt && echo 'hello deck' > sub/b.txt && ls -R",
                "所有练习都在 `~/linux-lab` 中进行，最后一步会删除它。"),
            Step.Change(
                "复制、重命名文件",
                "cd ~/linux-lab && cp -i a.txt c.txt && mv -i c.txt renamed.txt && ls",
                "`-i` 在覆盖已存在文件前询问。"),
            Step.Read(
                "按名称查找文件",
                "find ~/linux-lab -name '*.txt'",
                "等价的 `fd` 写法：`fd -e txt . ~/linux-lab`。"),
            Step.Read(
                "查找大于 1 GB 的文件",
                "find ~ -xdev -type f -size +1G -printf '%s\\t%p\\n' 2>/dev/null | sort -rn | head -n 20 | awk -F'\\t' '{printf \"%.1f GB  %s\\n\", $1/1073741824, $2}'",
                "`-xdev` 不进入其他文件系统（例如不去扫 SD 卡）；`-printf` 输出字节数与路径，文件名含空格也不会被截断。"),
            Step.Read(
                "在文件内容中搜索文字",
                "grep -rn 'hello' ~/linux-lab",
                "`-r` 递归搜索目录，`-n` 显示行号，`-i` 忽略大小写。`rg hello ~/linux-lab` 效果相同且更快。"),
            Step.Read(
                "用管道组合命令：统计与排序",
                "ls /usr/bin | wc -l; ps -eo comm | sort | uniq -c | sort -rn | head",
                "第一条统计系统命令的数量；第二条列出运行实例最多的程序。"),
            Step.Change(
                "把输出保存到文件，再追加一行",
                "cd ~/linux-lab && date > log.txt && echo '第二行' >> log.txt && cat log.txt",
                "`>` 新建或覆盖，`>>` 追加。"),
            Step.Danger(
                "删除练习目录",
                "rm -r ~/linux-lab",
                "删除前可先执行 `ls -R ~/linux-lab` 看看会删掉什么。"),
        ],

        Verify =
        [
            "1. `find` 与 `grep` 都能在练习目录中找到 `a.txt` 和 `sub/b.txt`。",
            "2. `log.txt` 中有两行：日期和「第二行」。",
            "3. 最后一步之后，`ls ~/linux-lab` 提示「没有那个文件或目录」。",
        ],
    };
}
