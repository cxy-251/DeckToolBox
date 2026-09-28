using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class RmTrashData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-rm-trash",
        Title = "rm 与回收站：删除文件",
        Keywords = "rm · rmdir · -i · -r · gio trash · kioclient",
        Level = ModuleLevel.Basic,
        Overview = "rm 直接删除文件，不经过回收站，无法恢复。日常删除可以改用 gio trash 或 kioclient 把文件移进回收站，与在 Dolphin 中按 Delete 键效果相同，删错了还能找回来。",

        Why =
        [
            "1. rm 与 rmdir：",
            "- `rm 文件`：删除文件。",
            "- `rm -r 目录`：删除目录及其中的一切。",
            "- `rm -i`：每个文件删除前询问；`rm -I`：删除超过 3 个文件或递归删除时只询问一次。",
            "- `rm -f`：不询问、忽略不存在的文件。**`-rf` 组合最危险**，路径写错时会一声不响地删除大量文件。",
            "- `rmdir 目录`：只能删除空目录，目录不空时报错，因此很安全。",
            "2. 回收站 (Trash)：遵循 freedesktop 规范，位于 `~/.local/share/Trash/`，`files/` 存放被删除的文件，`info/` 记录原始路径和删除时间。",
            "- `gio trash <文件>`：把文件移进回收站（GLib 提供）。",
            "- `kioclient move <文件> trash:/`：KDE 的方式，并且能用 `kioclient ls trash:/` 列出回收站内容、把文件移回原处。",
            "3. 通配符由 shell 展开：执行 `rm *.log` 前，先用 `ls *.log` 看看会匹配到哪些文件。",
        ],

        Consequences =
        [
            "• **rm 删除的文件无法恢复**，没有回收站，也没有撤销。",
            "• 路径中含变量时（如脚本里的 `rm -r \"$DIR\"/*`），变量为空会变成删除根目录下的内容。写脚本时先检查变量不为空。",
            "• 放进回收站的文件仍占用空间，直到清空回收站。",
            "• `gio trash` 不支持 `/tmp` 等内存文件系统，也不能跨分区放入家目录的回收站；这些位置的文件只能用 rm 删除。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习文件",
                "mkdir -p ~/rm-demo/empty ~/rm-demo/full && cd ~/rm-demo && touch 1.log 2.log keep.txt full/x.txt",
                "放在家目录中，以便测试回收站。").Then("没有输出，当前目录变为 `~/rm-demo`。"),
            Step.Read(
                "删除前先确认通配符会匹配到什么",
                "ls *.log",
                "shell 先把 `*.log` 展开成文件列表，再交给命令。").Then("输出 `1.log  2.log`，这就是 `rm *.log` 将会删除的文件。"),
            Step.Danger(
                "逐个确认后删除",
                "rm -i *.log",
                "`-i` 每个文件都询问。").Then("依次提示 `rm: 是否删除一般空文件 '1.log'？`，输入 `y` 删除、`n` 保留。"),
            Step.Change(
                "rmdir 只删除空目录",
                "rmdir empty full",
                "`full` 中有文件。").Then("`empty` 被删除；对 `full` 报错 `rmdir: 删除 'full' 失败: 目录非空`，它保持不变。"),
            Step.Change(
                "把文件移进回收站（可恢复）",
                "gio trash keep.txt && ls ~/.local/share/Trash/files/ | grep keep",
                "与在 Dolphin 中按 Delete 键效果相同。").Then("`keep.txt` 从当前目录消失，出现在回收站目录中；桌面上的回收站图标显示为非空。"),
            Step.Read(
                "列出回收站内容",
                "kioclient ls trash:/",
                "KDE 的回收站视图，名称前带有编号前缀。").Then("每行一个条目，如 `0-keep.txt`。"),
            Step.Change(
                "从回收站移回原处",
                "kioclient move 'trash:/0-keep.txt' ~/rm-demo/keep.txt && ls ~/rm-demo",
                "名称使用上一步列出的形式。").Then("`keep.txt` 回到 `~/rm-demo`，回收站中不再有它。"),
            Step.Danger(
                "删除整个目录",
                "cd ~ && rm -r ~/rm-demo",
                "`-r` 删除目录及其全部内容。先 `cd ~` 离开它。").Then("没有输出，`~/rm-demo` 及其中所有文件被永久删除。"),
        ],

        Verify =
        [
            "1. `ls ~/rm-demo` 报「没有那个文件或目录」。",
            "2. `kioclient ls trash:/` 中没有练习留下的文件。",
        ],
    };
}
