using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class TreeDuData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-tree-du",
        Title = "tree、du 与 ncdu：目录结构与占用",
        Keywords = "tree -L · du -sh · du --max-depth · ncdu",
        Level = ModuleLevel.Basic,
        Overview = "tree 以树形显示目录结构，du 统计目录占用的磁盘空间，ncdu 是可以用方向键浏览的交互式 du。三者都是 SteamOS 自带的，找「空间被谁占了」时从 ncdu 开始最直观。",

        Why =
        [
            "1. tree：",
            "- `-L 2`：只显示两层；`-d`：只显示目录。",
            "- `-a`：包括隐藏文件；`-h`：显示大小；`--du`：目录大小为其内容之和。",
            "- 最后一行汇总目录和文件数量。",
            "2. du（disk usage）：",
            "- `du -sh 目录`：只显示该目录的总大小（`-s` 汇总，`-h` 易读）。",
            "- `du -h --max-depth=1 目录`：显示每个直接子目录的大小。",
            "- 结果是实际占用的磁盘块，而不是文件的字节数；稀疏文件、压缩文件系统上两者可能不同，`--apparent-size` 显示字节数。",
            "- 配合 `sort -h` 按大小排序。",
            "3. ncdu：扫描完成后进入交互界面，按大小列出子目录与文件：",
            "- 方向键或 `j`/`k` 移动，回车进入目录，`←` 或 `h` 返回上一级。",
            "- `d` 删除选中的项（会先确认），`q` 退出，`?` 查看帮助。",
            "- `-x` 不进入其他文件系统（不扫 SD 卡）。",
        ],

        Consequences =
        [
            "• tree、du 只读；扫描大目录（如整个家目录）需要几十秒到几分钟。",
            "• **ncdu 中按 `d` 会直接删除文件或整个目录**，确认框中选 yes 之前看清路径。只想查看时不要按 `d`。",
        ],

        Steps =
        [
            Step.Read(
                "以树形查看家目录的前两层（只看目录）",
                "tree -L 2 -d ~ | head -n 40",
                "`-L 2` 两层、`-d` 只看目录。").Then("以 `├──`、`└──` 连线画出目录树，最后一行（完整输出时）汇总目录数量。"),
            Step.Read(
                "显示目录树并附带大小",
                "tree -h --du -L 1 ~/Documents",
                "`--du` 让目录显示为其全部内容之和。").Then("每项前面方括号中显示大小，如 `[ 12M]  Projects`。"),
            Step.Read(
                "查看一个目录的总大小",
                "du -sh ~/Downloads ~/Pictures",
                "`-s` 汇总、`-h` 易读。").Then("每个目录一行，如 `3.2G	/home/deck/Downloads`。"),
            Step.Read(
                "按大小列出家目录下各子目录",
                "du -h --max-depth=1 ~ 2>/dev/null | sort -h",
                "`sort -h` 能正确比较 K、M、G。").Then("每行「大小  目录」，从小到大，最后一行是家目录总计，倒数第二行就是最大的子目录。"),
            Step.Read(
                "交互式浏览占用",
                "ncdu -x ~",
                "`-x` 不进入其他文件系统。").Then("先显示扫描进度，完成后全屏列出家目录中的项目，按大小排序并带条形图。回车进入目录，`←` 返回，`q` 退出。"),
        ],

        Verify =
        [
            "1. `du -sh ~` 的结果与 ncdu 顶部显示的总大小基本一致。",
        ],
    };
}
