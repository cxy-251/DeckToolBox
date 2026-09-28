using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Archive;

public static class TarData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-tar",
        Title = "tar：打包与解包",
        Keywords = "tar -c -x -t · -f · -C · -a · --zstd · --exclude",
        Level = ModuleLevel.Basic,
        Overview = "tar 把许多文件和目录打成一个包（并保留目录结构、权限、时间），通常再配合压缩算法生成 .tar.gz、.tar.xz、.tar.zst。Linux 软件、备份、源码几乎都以这种格式分发。",

        Why =
        [
            "1. 必须记住的几个字母：",
            "- `c` 创建 (create)、`x` 解包 (extract)、`t` 列出内容 (list)，三者选一。",
            "- `f 文件名`：指定归档文件，几乎总要写，而且 `f` 后面必须紧跟文件名。",
            "- `v`：显示处理的每个文件。",
            "- `C 目录`：先切换到该目录再打包或解包。",
            "2. 压缩：",
            "- 解包时 GNU tar 自动识别压缩格式，只写 `tar -xf 文件` 即可。",
            "- 创建时用 `-a`，按扩展名自动选择：`.tar.gz`（gzip）、`.tar.xz`（xz）、`.tar.zst`（zstd）、`.tar.bz2`（bzip2）。也可以明确写 `-z`、`-J`、`--zstd`、`-j`。",
            "- 各压缩算法的速度与压缩率对比见「gzip、xz、zstd：压缩算法对比」。",
            "3. 常用组合：",
            "- 打包：`tar -caf 包.tar.zst 目录`。",
            "- 列出：`tar -tvf 包`。",
            "- 解包到指定目录：`tar -xf 包 -C 目标目录`（目标目录须已存在）。",
            "- 只解出其中某个文件：`tar -xf 包 路径/文件`，路径要与 `-t` 列出的完全一致。",
            "- 排除：`--exclude='*.log'`、`--exclude='.cache'`。",
            "4. 选项前的 `-` 可以省略（`tar czf` 与 `tar -czf` 相同），网上两种写法都很常见。",
        ],

        Consequences =
        [
            "• 打包不会删除原文件。",
            "• 解包时，包内路径相同的文件会**覆盖**目标位置已有的文件。不确定时先 `-t` 查看，或解到一个新的空目录。",
            "• 来源不明的压缩包可能包含 `../` 之类的路径；GNU tar 默认会去掉开头的 `/` 并拒绝越出目标目录，但解包前先查看内容仍是好习惯。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习数据",
                "mkdir -p /tmp/tar-demo/proj/logs && cd /tmp/tar-demo && echo 代码 > proj/main.txt && echo 日志 > proj/logs/run.log",
                "在 `/tmp/tar-demo` 中练习。").Then("没有输出，当前目录变为 `/tmp/tar-demo`。"),
            Step.Change(
                "打包并用 zstd 压缩",
                "tar -cavf proj.tar.zst proj",
                "`-a` 按扩展名选择压缩算法，`-v` 列出文件。").Then("逐行输出 `proj/`、`proj/main.txt`、`proj/logs/`、`proj/logs/run.log`，生成 `proj.tar.zst`。"),
            Step.Read(
                "查看包里有什么",
                "tar -tvf proj.tar.zst",
                "`-t` 只列出，不解包。").Then("每行显示权限、所有者、大小、时间和路径，形如 `-rw-r--r-- deck/deck 7 2026-09-28 20:20 proj/main.txt`。"),
            Step.Change(
                "排除某类文件打包",
                "tar -caf proj-nolog.tar.gz --exclude='*.log' proj && tar -tf proj-nolog.tar.gz",
                "`--exclude` 可以写多次。").Then("列出的内容中没有 `run.log`。"),
            Step.Change(
                "解包到指定目录",
                "mkdir -p restore && tar -xf proj.tar.zst -C restore && find restore",
                "`-C` 指定的目录必须先存在，否则报错「无法 open」。").Then("列出 `restore/proj/main.txt`、`restore/proj/logs/run.log` 等，目录结构与原来一致。"),
            Step.Change(
                "只解出其中一个文件",
                "mkdir -p one && tar -xf proj.tar.zst -C one proj/main.txt && find one -type f",
                "文件路径要与 `tar -t` 列出的完全一致。").Then("只输出 `one/proj/main.txt`。"),
            Step.Read(
                "查看网上下载的软件包内容（示例）",
                "tar -tf ~/Downloads/<文件>.tar.gz | head",
                "解包前先看顶层目录结构，决定解到哪里。").Then("列出前 10 个路径；若都以同一个目录名开头，解包后会得到一个整洁的目录，否则文件会散落在当前目录中。"),
        ],

        Verify =
        [
            "1. `diff -r proj restore/proj` 没有输出，说明解包结果与原目录一致。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/tar-demo` 清理。",
        ],
    };
}
