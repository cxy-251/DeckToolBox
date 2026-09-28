using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class MkdirTouchData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-mkdir-touch",
        Title = "mkdir 与 touch：新建目录与文件",
        Keywords = "mkdir -p · touch · 花括号展开",
        Level = ModuleLevel.Basic,
        Overview = "mkdir 创建目录，touch 创建空文件或更新文件的修改时间。配合 bash 的花括号展开，一条命令就能建好一整套目录结构。",

        Why =
        [
            "1. mkdir：",
            "- `mkdir 目录`：创建一个目录，父目录必须已存在。",
            "- `mkdir -p a/b/c`：连同不存在的中间目录一起创建；目录已存在时不报错，因此适合写在脚本里重复执行。",
            "- `mkdir -m 700 目录`：创建时直接指定权限。",
            "2. touch：",
            "- 文件不存在时创建一个空文件。",
            "- 文件已存在时，只把它的修改时间更新为现在，内容不变。",
            "- `touch -d '2026-01-01 12:00' 文件`：把修改时间设为指定时间。",
            "3. 花括号展开（bash 功能）：`{a,b,c}` 展开为三个词，`{1..5}` 展开为 1 到 5。`mkdir -p proj/{src,docs,tests}` 一次创建三个子目录。",
        ],

        Consequences =
        [
            "• mkdir 不会覆盖已存在的目录或其中内容。",
            "• touch 不会改变已有文件的内容，只改时间；但依赖修改时间的工具（如 `cp -u`、`rsync`、`make`）会因此认为文件「变新了」。",
        ],

        Steps =
        [
            Step.Change(
                "一次创建多层目录",
                "mkdir -pv /tmp/mk-demo/project/src/utils",
                "`-p` 自动创建中间目录，`-v` 显示创建了哪些。").Then("逐行输出 `mkdir: 已创建目录 '/tmp/mk-demo'` 等四行，每层目录一行。"),
            Step.Change(
                "用花括号一次创建多个目录",
                "mkdir -p /tmp/mk-demo/project/{docs,tests,assets/{images,sounds}} && tree /tmp/mk-demo",
                "花括号可以嵌套。").Then("`tree` 显示 project 下有 `assets`（含 images、sounds）、`docs`、`src`、`tests` 等目录。"),
            Step.Change(
                "创建多个空文件",
                "touch /tmp/mk-demo/project/docs/{readme,notes}.txt && ls -l /tmp/mk-demo/project/docs",
                "不存在的文件会被创建为空文件。").Then("列出 `notes.txt` 和 `readme.txt`，大小都是 0。"),
            Step.Change(
                "修改文件的时间戳",
                "touch -d '2026-01-01 08:00' /tmp/mk-demo/project/docs/notes.txt && ls -l /tmp/mk-demo/project/docs",
                "`-d` 指定时间。").Then("`notes.txt` 的时间显示为 `2026年 1月 1日`，内容与大小不变。"),
            Step.Change(
                "创建只有自己能访问的目录",
                "mkdir -m 700 /tmp/mk-demo/private && ls -ld /tmp/mk-demo/private",
                "`-m` 直接设置权限。").Then("输出 `drwx------ ... /tmp/mk-demo/private`：其他用户无法进入。"),
        ],

        Verify =
        [
            "1. 再执行一次第 1 步的 `mkdir -p`，没有报错也没有输出，说明它可以安全地重复执行。",
            "2. `find /tmp/mk-demo -type d | wc -l` 统计出创建的目录数量。",
        ],
    };
}
