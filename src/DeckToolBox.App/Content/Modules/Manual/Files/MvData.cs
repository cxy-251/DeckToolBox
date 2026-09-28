using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class MvData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-mv",
        Title = "mv：移动与重命名",
        Keywords = "mv · -i · -n · -v · 重命名",
        Level = ModuleLevel.Basic,
        Overview = "mv 同时承担「移动」和「重命名」两个功能：目标是目录就移进去，否则就改名。和 cp 一样，它默认会直接覆盖同名文件。",

        Why =
        [
            "1. 基本形式：",
            "- `mv 旧名 新名`：重命名（同一目录内）。",
            "- `mv 文件... 目录/`：把一个或多个文件移进目录。",
            "- `mv 目录 新位置/`：移动整个目录，不需要 `-r`。",
            "2. 同一分区与跨分区：",
            "• 在同一个分区内移动，只是修改目录记录，无论文件多大都几乎立即完成。",
            "• 跨分区（例如从内置存储移到 SD 卡），实际是先复制再删除原文件，耗时与文件大小成正比；中途中断时原文件还在。",
            "3. 防止覆盖：`-i` 覆盖前询问，`-n` 从不覆盖；`-v` 显示每一次移动。",
            "4. 批量重命名：mv 一次只能改一个名字，批量改名见「rename：批量重命名」。",
        ],

        Consequences =
        [
            "• **目标已存在时默认直接覆盖**，被覆盖的文件无法恢复。",
            "• 移动到其他分区的过程中若空间不足，会报错并保留原文件。",
            "• 移动游戏目录、Proton 前缀等被 Steam 记录了路径的目录后，Steam 会找不到它们；游戏的位置请在 Steam 的存储管理中移动。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习文件",
                "mkdir -p /tmp/mv-demo/dir && cd /tmp/mv-demo && echo 1 > a.txt && echo 2 > b.txt",
                "在 `/tmp/mv-demo` 中练习。").Then("没有输出，当前目录变为 `/tmp/mv-demo`。"),
            Step.Change(
                "重命名一个文件",
                "mv -v a.txt first.txt",
                "`-v` 显示操作。").Then("输出 `已重命名 'a.txt' -> 'first.txt'`；`a.txt` 不再存在。"),
            Step.Change(
                "把文件移进目录",
                "mv -v first.txt b.txt dir/ && ls dir",
                "多个源文件、最后一个参数是目录。").Then("输出两行 `已重命名 ... -> 'dir/...'`，`ls dir` 显示 `b.txt  first.txt`。"),
            Step.Change(
                "移动时不覆盖已有文件",
                "echo new > b.txt && mv -n b.txt dir/ && cat dir/b.txt && ls",
                "`-n` 目标已存在时跳过。").Then("`cat` 仍输出 `2`（dir 中原来的内容），当前目录中的 `b.txt` 没有被移走。"),
            Step.Change(
                "覆盖前询问",
                "mv -i b.txt dir/",
                "目标 `dir/b.txt` 已存在。").Then("提示 `mv: 是否覆盖 'dir/b.txt'？`，输入 `y` 才覆盖。"),
        ],

        Verify =
        [
            "1. `ls -R /tmp/mv-demo` 显示文件都在预期的位置。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/mv-demo` 清理。",
        ],
    };
}
