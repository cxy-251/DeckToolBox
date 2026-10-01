using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class LnData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-ln",
        Title = "ln：软链接与硬链接",
        Keywords = "ln -s · 硬链接 · readlink · inode",
        Level = ModuleLevel.Basic,
        Overview = "链接让一个文件或目录在另一个位置「出现」而不必复制。最常用的是软链接（符号链接）：例如把 SD 卡上的目录链接到家目录里，程序按原路径访问，数据实际放在 SD 卡上。",

        Why =
        [
            "1. 软链接（符号链接，`ln -s 目标 链接名`）：",
            "• 一个特殊的小文件，内容是目标的路径。访问链接时系统自动转到目标。",
            "• 可以指向目录，可以跨分区、跨设备。",
            "• 目标被删除或移动后，链接失效（成为「悬空链接」），`ls -l` 中通常显示为红色。",
            "• 建议用绝对路径作为目标；使用相对路径时，它是相对于链接所在目录解析的。",
            "2. 硬链接（`ln 目标 链接名`）：",
            "• 同一份数据的另一个文件名，两个名字指向同一个 inode，地位完全相同。",
            "• 删除其中一个名字，数据仍然存在，直到最后一个名字被删除。",
            "• 不能指向目录，不能跨分区。",
            "3. `ls -l` 第二列是硬链接数；`ls -i` 显示 inode 编号，同一 inode 就是同一份数据。",
            "4. `readlink -f 路径`：解析出链接最终指向的真实路径。",
        ],

        Consequences =
        [
            "• 创建链接不会复制数据，也不会修改目标。",
            "• 通过链接修改内容，就是修改目标本身。",
            "• 删除软链接 (`rm 链接名`) 只删除链接本身，不影响目标；但**不要在链接名后加斜杠执行 `rm -r 链接名/`**，那样会删除目标目录里的内容。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习文件",
                "mkdir -p /tmp/ln-demo && cd /tmp/ln-demo && echo 原始内容 > original.txt",
                "在 `/tmp/ln-demo` 中练习。").Then("没有输出，当前目录变为 `/tmp/ln-demo`。"),
            Step.Change(
                "创建软链接与硬链接",
                "ln -s /tmp/ln-demo/original.txt soft.txt && ln original.txt hard.txt && ls -li",
                "`-s` 表示软链接；不加为硬链接。`-i` 显示 inode 编号。").Then("三行：`hard.txt` 与 `original.txt` 的 inode 编号相同、链接数为 2；`soft.txt` 以 `l` 开头，显示 `soft.txt -> /tmp/ln-demo/original.txt`。"),
            Step.Read(
                "通过链接读取",
                "cat soft.txt hard.txt",
                "两个链接都指向同一份内容。").Then("输出两行「原始内容」。"),
            Step.Change(
                "删除原文件后观察两种链接",
                "rm original.txt && cat hard.txt; cat soft.txt",
                "硬链接仍持有数据，软链接失去目标。").Then("`hard.txt` 仍输出「原始内容」；`soft.txt` 报错 `没有那个文件或目录`，它成为悬空链接。"),
            Step.Read(
                "查看链接指向的真实路径",
                "readlink soft.txt; readlink -f ~/.steam/steam",
                "`-f` 逐级解析到最终路径。").Then("第一行 `/tmp/ln-demo/original.txt`；第二行 `/home/deck/.local/share/Steam`，说明 `~/.steam/steam` 是指向它的软链接。"),
            Step.Change(
                "把 SD 卡上的目录链接到家目录（示例）",
                "ln -s /run/media/deck/<卷标>/<目录> ~/<链接名>",
                "`<卷标>` 由 `lsblk` 查得；`<目录>` 为卡内真实目录名；`<链接名>` 为家目录下自定义的链接名称。").Then("家目录中出现一个带箭头图标的链接，打开它看到的就是 SD 卡上的内容；拔出 SD 卡后链接暂时失效。"),
        ],

        Verify =
        [
            "1. `ls -l ~ | grep '^l'` 列出家目录中的所有软链接及其目标。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/ln-demo` 清理。",
        ],
    };
}
