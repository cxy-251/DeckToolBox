using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class PathRenameData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-path-rename",
        Title = "路径处理与批量重命名",
        Keywords = "realpath · basename · dirname · rename · for 循环",
        Level = ModuleLevel.Basic,
        Overview = "脚本里经常需要从完整路径中取出文件名、目录名或去掉扩展名，也常常需要一次改很多文件的名字。本模块介绍 realpath、basename、dirname，以及用 rename 和 bash 循环批量重命名。",

        Why =
        [
            "1. 路径工具：",
            "- `realpath 路径`：转成绝对路径，并解析掉 `..`、`.` 和符号链接。",
            "- `basename 路径 [后缀]`：取最后一部分（文件名）；给出后缀时一并去掉，如 `basename a/b/game.zip .zip` 得到 `game`。",
            "- `dirname 路径`：取所在目录。",
            "2. bash 的参数展开（不需要调用外部命令，脚本里更常用）：",
            "- `${f##*/}`：去掉最后一个 `/` 及其之前的部分，等同 basename。",
            "- `${f%.*}`：去掉最后一个 `.` 及其后面的扩展名。",
            "- `${f##*.}`：只取扩展名。",
            "3. rename（util-linux 版本）：`rename 旧字符串 新字符串 文件...`，把每个文件名中第一次出现的旧字符串替换为新字符串。",
            "• `-n` 只演示不执行，`-v` 显示改名过程。",
            "• 注意：Debian / Ubuntu 默认的 rename 是另一个 Perl 版本，语法是 `rename 's/旧/新/' 文件`，两者不通用。",
            "4. 更复杂的改名规则用 bash 循环：`for f in *.jpg; do mv -n \"$f\" \"新名字\"; done`。变量两边加双引号，文件名含空格时才不会出错。",
        ],

        Consequences =
        [
            "• realpath、basename、dirname 只处理字符串，不访问、不修改文件（realpath 解析链接时会读取文件系统）。",
            "• 批量改名一旦出错，很难逐个改回。先用 `rename -n` 或把循环中的 `mv` 换成 `echo mv` 演示一遍。",
            "• 改名后新名字与已有文件重名时，`mv` 默认会覆盖；循环里用 `mv -n` 更安全。",
        ],

        Steps =
        [
            Step.Read(
                "从完整路径中取出各部分",
                "f=/home/deck/Downloads/game.tar.gz; basename \"$f\"; basename \"$f\" .tar.gz; dirname \"$f\"; echo \"${f##*/}\" \"${f%.*}\" \"${f##*.}\"",
                "先把路径存入变量 `f`，再用不同方式截取。").Then("依次输出 `game.tar.gz`、`game`、`/home/deck/Downloads`，最后一行 `game.tar.gz /home/deck/Downloads/game.tar gz`。"),
            Step.Read(
                "转换为绝对路径并解析链接",
                "cd ~/Downloads && realpath ../.steam/steam",
                "`..` 和符号链接都会被解析。").Then("输出 `/home/deck/.local/share/Steam`。"),
            Step.Change(
                "准备一批要改名的文件",
                "mkdir -p /tmp/ren-demo && cd /tmp/ren-demo && touch IMG_001.JPG IMG_002.JPG IMG_003.JPG && ls",
                "在 `/tmp/ren-demo` 中练习。").Then("输出 `IMG_001.JPG  IMG_002.JPG  IMG_003.JPG`。"),
            Step.Read(
                "演示改名效果（不执行）",
                "rename -nv .JPG .jpg *.JPG",
                "`-n` 只演示不执行；必须同时加 `-v` 才会显示改名预览。").Then("输出三行改名预览（把 IMG_001.JPG 改为 IMG_001.jpg 等），文件名没有真正改变。"),
            Step.Change(
                "执行批量改名",
                "rename -v .JPG .jpg *.JPG && ls",
                "把扩展名改成小写。").Then("输出三行改名记录，`ls` 显示 `IMG_001.jpg  IMG_002.jpg  IMG_003.jpg`。"),
            Step.Change(
                "用循环按规则改名：加上日期前缀",
                "for f in *.jpg; do mv -nv \"$f\" \"$(date +%F)_${f#IMG_}\"; done",
                "`${f#IMG_}` 去掉开头的 `IMG_`；`$(date +%F)` 是今天的日期。").Then("每个文件一行 `已重命名 'IMG_001.jpg' -> '2026-09-28_001.jpg'`。"),
        ],

        Verify =
        [
            "1. `ls /tmp/ren-demo` 显示改名后的文件，数量不变。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/ren-demo` 清理。",
        ],
    };
}
