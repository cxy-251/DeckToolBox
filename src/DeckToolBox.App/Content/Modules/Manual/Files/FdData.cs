using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class FdData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-fd",
        Title = "fd：更快更简单的查找",
        Keywords = "fd · -e · -t · -H · -I · -x",
        Level = ModuleLevel.Basic,
        Overview = "fd 是 find 的现代替代品，SteamOS 自带。语法更短：直接写要找的名字片段即可；默认忽略隐藏文件和 .gitignore 中的内容，输出带颜色，速度也更快。日常交互式查找用 fd，写需要到处运行的脚本时用 find。",

        Why =
        [
            "1. 写法：`fd [模式] [目录]`。模式是正则表达式，匹配文件名的任意部分；不写目录时从当前目录开始。",
            "2. 与 find 的默认行为不同：",
            "- 忽略隐藏文件和目录（以 `.` 开头的），`-H` 包括它们。",
            "- 忽略 `.gitignore` 中列出的内容，`-I` 不忽略。",
            "- 大小写智能匹配：模式全是小写时不区分大小写，含大写字母时区分。",
            "3. 常用选项：",
            "- `-e txt`：按扩展名。",
            "- `-t f` 文件、`-t d` 目录、`-t l` 链接。",
            "- `-d 2`：最多向下 2 层。",
            "- `-S +100m`：大于 100 MB。",
            "- `--changed-within 2d`：两天内修改过。",
            "- `-x 命令`：对每个结果执行命令，`-X 命令` 把所有结果一次性交给命令。",
            "4. 名字：在 Debian / Ubuntu 上这个程序叫 `fdfind`，其他发行版和 SteamOS 上叫 `fd`。",
        ],

        Consequences =
        [
            "• 只查找时是只读的。",
            "• 因为默认忽略隐藏文件，找配置文件（如 `~/.config` 下的）时记得加 `-H`，否则可能以为文件不存在。",
            "• `-x rm` 之类的组合会对每个结果执行删除，先不带 `-x` 运行一遍确认结果。",
        ],

        Steps =
        [
            Step.Read(
                "在下载目录中按名称片段查找",
                "fd zip ~/Downloads",
                "匹配文件名中任意位置出现的 zip。").Then("列出路径中包含 zip 的文件，如压缩包，匹配的部分高亮显示；没有结果时没有输出。"),
            Step.Read(
                "按扩展名查找图片",
                "fd -e png -e jpg . ~/Pictures",
                "`.` 表示匹配任意名字，再用 `-e` 限定扩展名。").Then("列出 Pictures 下所有 png 和 jpg 文件的路径。"),
            Step.Read(
                "查找隐藏的配置文件",
                "fd -H -d 1 bash ~",
                "`-H` 包括隐藏文件，`-d 1` 只看第一层。").Then("列出 `.bashrc`、`.bash_profile`、`.bash_history` 等；不加 `-H` 则什么也找不到。"),
            Step.Read(
                "查找大文件",
                "fd -t f -S +500m . ~",
                "`-S +500m` 大于 500 MB。").Then("列出家目录（不含隐藏目录）中大于 500 MB 的文件，通常是游戏、视频或压缩包。"),
            Step.Read(
                "查找最近两天修改过的文件",
                "fd -t f --changed-within 2d . ~/Documents",
                "时间单位可以是 `h`、`d`、`w`。").Then("列出 Documents 中两天内修改过的文件。"),
            Step.Read(
                "对每个结果执行命令",
                "fd -e sh . ~ -d 2 -x ls -l",
                "`-x` 对每个结果执行后面的命令，结果路径作为最后一个参数传入。").Then("对找到的每个 `.sh` 文件输出一行 `ls -l` 的结果。"),
        ],

        Verify =
        [
            "1. `fd -H bashrc ~ -d 1` 与 `find ~ -maxdepth 1 -name '*bashrc*'` 找到的文件相同。",
        ],
    };
}
