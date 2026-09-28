using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class LsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-ls",
        Title = "ls：列出目录内容",
        Keywords = "ls -l · -a · -h · -t · -S · -R",
        Level = ModuleLevel.Basic,
        Overview = "ls 是最常用的命令：列出一个目录里有哪些文件。加上不同选项，可以看到权限、大小、修改时间，按时间或大小排序，显示隐藏文件。",

        Why =
        [
            "1. 基本用法：`ls` 列出当前目录；`ls <目录>` 列出指定目录；`ls <文件>` 只显示这个文件。",
            "2. 常用选项（可以合并写，如 `ls -lah`）：",
            "- `-l`：长格式，每行一个文件，显示权限、硬链接数、所有者、组、大小、修改时间。",
            "- `-a`：包括以 `.` 开头的隐藏文件（配置文件大多是隐藏的，如 `.bashrc`）。",
            "- `-h`：大小以 K、M、G 显示，需与 `-l` 一起用。",
            "- `-t`：按修改时间排序，最新的在前；加 `-r` 反转顺序。",
            "- `-S`：按大小排序，最大的在前。",
            "- `-R`：递归列出所有子目录。",
            "- `-d`：列出目录本身而不是其内容，常与 `-l` 一起查看目录的权限。",
            "- `-F`：在名称后加类型标记：`/` 目录、`*` 可执行文件、`@` 符号链接。",
            "3. 读懂 `-l` 的第一列：第 1 个字符是类型（`-` 文件、`d` 目录、`l` 链接），后 9 个字符分三组，是所有者、组、其他人的读 (r)、写 (w)、执行 (x) 权限。",
            "4. SteamOS 的 bash 中 `ls` 是 `ls --color=auto` 的别名，所以输出带颜色：蓝色是目录、绿色是可执行文件、青色是链接。",
        ],

        Consequences =
        [
            "• ls 只读取目录信息，不会修改任何东西。",
            "• 目录中文件极多时（上万个），`ls -l` 需要读取每个文件的属性，会慢一些；只需数量时用 `ls | wc -l`。",
            "• 不要在脚本中解析 `ls` 的输出来处理文件名（文件名可能含空格、换行），应使用通配符或 `find`。",
        ],

        Steps =
        [
            Step.Read(
                "列出家目录",
                "ls ~",
                "`~` 表示家目录 `/home/deck`。").Then("在一行或几行中列出 `Desktop`、`Documents`、`Downloads`、`Pictures` 等目录名，目录显示为蓝色。隐藏文件不显示。"),
            Step.Read(
                "长格式列出，包括隐藏文件，大小易读",
                "ls -lah ~",
                "`-l` 长格式、`-a` 含隐藏文件、`-h` 易读大小。").Then("第一行 `总计 ...`，之后每行一个条目，例如 `-rw-r--r-- 1 deck deck 675 9月28日 17:09 .bashrc`；`.` 表示当前目录本身，`..` 表示上一级目录。"),
            Step.Read(
                "按修改时间排序，看最近下载了什么",
                "ls -lt ~/Downloads | head",
                "`-t` 按时间排序，最新的在最上面；`head` 只看前 10 行。").Then("最近修改或下载的文件排在第一行（第一行是 `总计`）。"),
            Step.Read(
                "按大小排序，找出最大的文件",
                "ls -lhS ~/Downloads | head",
                "`-S` 按大小从大到小。").Then("最大的文件排在最前面，大小列以 M、G 显示。"),
            Step.Read(
                "查看目录本身的权限",
                "ls -ld ~/.ssh ~/Downloads 2>/dev/null",
                "`-d` 显示目录本身；不存在的目录会报错，`2>/dev/null` 把报错隐藏。").Then("每个目录一行，如 `drwx------ ... /home/deck/.ssh`（只有自己能访问）。"),
            Step.Read(
                "标记出文件类型",
                "ls -F /usr/bin | head -n 5; ls -F ~",
                "`-F` 在名称后加 `/`、`*`、`@` 等标记。").Then("家目录中的目录名后面带 `/`；可执行文件后带 `*`，符号链接后带 `@`。"),
        ],

        Verify =
        [
            "1. `ls -a ~` 能看到 `.bashrc`、`.config`、`.local` 等隐藏条目，而 `ls ~` 看不到。",
            "2. 新建一个文件后执行 `ls -lt`，它排在第一位。",
        ],
    };
}
