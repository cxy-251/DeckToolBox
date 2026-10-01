using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class DiffCmpData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-diff-cmp",
        Title = "diff 与 cmp：比较文件和目录",
        Keywords = "diff -u · diff -r · -q · cmp · comm",
        Level = ModuleLevel.Basic,
        Overview = "修改配置前后有什么不同？备份和原目录是否一致？两个下载的文件是不是同一个？diff 逐行比较文本并标出差异，cmp 逐字节比较任意文件，diff -r 比较整个目录。",

        Why =
        [
            "1. diff 的两种输出格式：",
            "• 默认格式：`2c2` 表示第 2 行被修改 (change)，`3a4` 表示在第 3 行后增加 (add)，`d` 表示删除；`<` 开头是第一个文件的内容，`>` 开头是第二个文件的内容。",
            "• 统一格式 `-u`（最常用）：`---` 是旧文件、`+++` 是新文件；`@@ -1,3 +1,4 @@` 标明位置；以 `-` 开头的行被删除、`+` 开头的行被添加、空格开头的行未变。补丁 (patch) 和 git 都用这种格式。",
            "2. 常用选项：",
            "- `-r`：递归比较两个目录。",
            "- `-q`：只报告哪些文件不同，不显示具体内容。",
            "- `-i` 忽略大小写、`-w` 忽略所有空白、`-B` 忽略空行。",
            "- `--color`：带颜色显示。",
            "3. 退出码：0 相同、1 不同、2 出错。脚本里可以据此判断。",
            "4. cmp：逐字节比较，适合二进制文件（图片、程序、压缩包），报告第一个不同之处的位置；`-s` 静默，只返回退出码。",
            "5. comm：比较两个**已排序**的文件，分三列输出：只在第一个中、只在第二个中、两者都有。",
        ],

        Consequences =
        [
            "• 这些命令都只读，不会修改被比较的文件。",
            "• 比较大文件时，cmp 找到第一个差异就停止，比先算 sha256 再比较更快；判断两个文件是否完全相同，二者都可以。",
        ],

        Steps =
        [
            Step.Change(
                "准备两个版本的文件",
                "mkdir -p /tmp/diff-demo && cd /tmp/diff-demo && printf 'a\\nb\\nc\\n' > v1.txt && printf 'a\\nB\\nc\\nd\\n' > v2.txt",
                "v2 修改了第 2 行，并新增了第 4 行。").Then("没有输出，当前目录变为 `/tmp/diff-demo`。").Practice(),
            Step.Read(
                "默认格式比较",
                "diff v1.txt v2.txt",
                "`<` 来自第一个文件，`>` 来自第二个文件。").Then("输出 `2c2`、`< b`、`---`、`> B`，以及 `3a4`、`> d`。"),
            Step.Read(
                "统一格式比较",
                "diff -u --color v1.txt v2.txt",
                "最常用、最易读的格式。").Then("头部两行 `--- v1.txt` 与 `+++ v2.txt`，然后 `@@ -1,3 +1,4 @@`，以及 ` a`、`-b`（红色）、`+B`（绿色）、` c`、`+d`（绿色）。"),
            Step.Read(
                "只关心是否相同",
                "diff -q v1.txt v2.txt; echo \"退出码 $?\"; cmp v1.txt v2.txt",
                "`$?` 是上一条命令的退出码。").Then("`文件 v1.txt 和 v2.txt 不同`、`退出码 1`，以及 cmp 的 `v1.txt v2.txt 不同：第 3 字节（第 2 行）`。"),
            Step.Change(
                "比较两个目录",
                "mkdir -p d1 d2 && cp v1.txt d1/ && cp v1.txt d2/ && echo x > d2/extra.txt && diff -rq d1 d2",
                "`-r` 递归，`-q` 只列出不同。").Then("输出 `只在 d2 中存在：extra.txt`；两边都有且内容相同的 `v1.txt` 不会出现。").Practice(),
            Step.Read(
                "对比修改前后的配置（示例）",
                "diff -u ~/.bashrc.bak-2026-09-28 ~/.bashrc",
                "修改配置前先备份，改完用它确认改动范围。备份文件不存在时会报错。").Then("以统一格式显示备份与当前文件的差异，能清楚看到删掉和新增了哪些行。"),
        ],

        Verify =
        [
            "1. `diff v1.txt v1.txt; echo $?` 没有差异输出，退出码为 0。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/diff-demo` 清理。",
        ],
    };
}
