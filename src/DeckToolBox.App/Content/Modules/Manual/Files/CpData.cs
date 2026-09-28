using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class CpData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-cp",
        Title = "cp：复制文件与目录",
        Keywords = "cp -r · -i · -n · -u · -a · -v",
        Level = ModuleLevel.Basic,
        Overview = "cp 把文件或目录复制到另一个位置。要特别注意它默认会直接覆盖同名文件，而且不会提示；学会 -i、-n、-u 这几个选项，就能避免误覆盖。",

        Why =
        [
            "1. 基本形式：",
            "- `cp 源文件 目标文件`：复制并命名为目标文件名。",
            "- `cp 源文件... 目标目录/`：把一个或多个文件复制到目录中，保持原名。",
            "- `cp -r 源目录 目标`：复制整个目录（`-r` 表示递归）。目标目录不存在时，复制出的目录名为目标名；已存在时，复制到它的里面。",
            "2. 防止覆盖：",
            "- `-i`：目标已存在时询问，输入 `y` 才覆盖。",
            "- `-n`：目标已存在时跳过，从不覆盖。",
            "- `-u`：只在源文件比目标新、或目标不存在时才复制，适合反复同步。",
            "3. 保留属性：`-a`（归档模式）= 递归 + 保留权限、时间戳、符号链接。备份时用 `-a`，复制出的文件修改时间与原文件一致。",
            "4. `-v`：显示每个被复制的文件，便于确认。",
            "5. 大量文件或需要断点续传、跨机器复制时，改用 `rsync`（见「压缩包与 rsync 备份」）。",
        ],

        Consequences =
        [
            "• **不加 `-i` 或 `-n` 时，同名的目标文件会被直接覆盖，没有提示，也无法撤销。**",
            "• 复制到空间不足的磁盘会中途失败，留下不完整的文件。",
            "• 复制大文件到 SD 卡或 U 盘时，命令结束后数据可能还在内存缓存中；拔出前先安全弹出（见「U 盘、SD 卡与移动硬盘」）。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习文件",
                "mkdir -p /tmp/cp-demo/src/sub && echo v1 > /tmp/cp-demo/src/a.txt && echo sub > /tmp/cp-demo/src/sub/b.txt && cd /tmp/cp-demo",
                "所有练习在 `/tmp/cp-demo` 中进行，重启后自动清除。").Then("没有输出，终端的当前目录变为 `/tmp/cp-demo`。"),
            Step.Change(
                "复制一个文件并显示过程",
                "cp -v src/a.txt a-copy.txt",
                "`-v` 显示复制了什么。").Then("输出 `'src/a.txt' -> 'a-copy.txt'`，当前目录多出 `a-copy.txt`。"),
            Step.Change(
                "复制整个目录",
                "cp -rv src backup",
                "`-r` 递归复制目录。").Then("逐行列出每个被复制的文件和目录，如 `'src/sub/b.txt' -> 'backup/sub/b.txt'`；`backup` 目录结构与 `src` 相同。"),
            Step.Change(
                "覆盖前询问",
                "cp -i src/a.txt a-copy.txt",
                "`a-copy.txt` 已存在。").Then("提示 `cp: 是否覆盖 'a-copy.txt'？`：输入 `y` 回车覆盖，直接回车或输入 `n` 则保留原文件。"),
            Step.Change(
                "只复制目标中没有的文件",
                "cp -rn src/. backup/",
                "`-n` 已存在的文件一律跳过；`src/.` 表示复制 src 的内容而不是 src 目录本身。").Then("没有输出，`backup` 中已有的文件保持不变，只补上缺少的文件。"),
            Step.Change(
                "只复制比目标更新的文件",
                "echo v2 > src/a.txt && cp -ruv src/. backup/",
                "`-u` 只在源文件更新时复制。").Then("只输出一行 `'src/./a.txt' -> 'backup/./a.txt'`：只有刚修改过的 a.txt 被复制，其他文件被跳过。"),
            Step.Change(
                "保留时间戳与权限复制（备份用）",
                "cp -a src archive && ls -l --time-style=+%T src/a.txt archive/a.txt",
                "`-a` 归档模式。").Then("两个文件的修改时间完全相同；不加 `-a` 复制的文件，时间会是复制那一刻。"),
        ],

        Verify =
        [
            "1. `diff -r src backup` 没有输出，说明两个目录内容一致。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/cp-demo` 清理。",
        ],
    };
}
