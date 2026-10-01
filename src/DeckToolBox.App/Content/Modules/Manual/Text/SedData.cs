using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class SedData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-sed",
        Title = "sed：查找替换与按行编辑",
        Keywords = "sed s/// · -i · -n p · d · -E · 捕获组",
        Level = ModuleLevel.Intermediate,
        Overview = "sed 是流编辑器：逐行读入文本，按规则替换、删除或打印，再输出结果。最常见的用途是批量替换文件中的文字，以及从配置文件中提取某个值。",

        Why =
        [
            "1. 替换命令 `s/旧/新/标志`：",
            "- 默认每行只替换第一个匹配；标志 `g` 替换该行所有匹配，`I` 忽略大小写。",
            "- 分隔符不一定是 `/`，替换路径时可以写成 `s|/old/path|/new/path|`，避免转义。",
            "- `&` 在替换部分代表整个匹配到的文字。",
            "2. 行的选择：",
            "- `3`：第 3 行；`2,5`：第 2 到 5 行；`$`：最后一行。",
            "- `/正则/`：匹配该正则的行，例如 `/^#/` 表示注释行。",
            "- 选择后接命令：`p` 打印、`d` 删除、`s///` 替换，例如 `/^#/d` 删除所有注释行。",
            "3. `-n`：不自动输出每一行，只输出 `p` 命令指定的内容。`sed -n '2,3p'` 只打印第 2、3 行。",
            "4. `-E`：扩展正则，捕获组写作 `( )`，替换中用 `\\1`、`\\2` 引用。",
            "5. `-i`：直接修改文件本身（in-place）；`-i.bak` 修改前先把原文件备份为 `.bak`。",
        ],

        Consequences =
        [
            "• 不带 `-i` 时 sed 只输出结果，不修改文件，可以放心尝试。",
            "• **`-i` 直接改写文件，没有撤销。**先不带 `-i` 运行确认输出正确，再加 `-i.bak`。",
            "• sed 规则要放在单引号中，防止 shell 展开 `$`、`*` 等字符。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习文件",
                "rm -rf /tmp/sed-demo && mkdir -p /tmp/sed-demo && cd /tmp/sed-demo && printf '# 配置文件\\nvolume=50\\nname=deck\\n# 结束\\nvolume_max=100\\n' > app.conf && cat app.conf",
                "先删除上次练习留下的目录，重复练习时结果才与下面的说明一致。一个模拟的配置文件。").Then("显示五行内容，其中两行以 `#` 开头。").Practice(),
            Step.Read(
                "替换（只输出结果，不改文件）",
                "sed 's/deck/steamdeck/' app.conf",
                "`s/旧/新/` 执行单次正则替换，不加 `g` 标志时每行仅替换首处匹配，直接流式输出至终端而不改动源文件。").Then("输出全部五行，其中 `name=deck` 变为 `name=steamdeck`；`cat app.conf` 可以确认原文件没有变化。"),
            Step.Read(
                "只打印指定行",
                "sed -n '2,3p' app.conf",
                "`-n` 关闭自动输出，`2,3p` 打印第 2 到 3 行。").Then("输出 `volume=50` 与 `name=deck`。"),
            Step.Read(
                "删除注释行",
                "sed '/^#/d' app.conf",
                "`/^#/` 选中以 # 开头的行，`d` 删除。").Then("输出三行，不再有以 `#` 开头的行。"),
            Step.Read(
                "用捕获组提取值",
                "sed -nE 's/^volume=([0-9]+)$/\\1/p' app.conf",
                "`([0-9]+)` 捕获数字，`\\1` 引用它，`p` 只打印替换成功的行。").Then("只输出 `50`：`volume_max=100` 因为不符合 `^volume=数字$` 而没有被匹配。"),
            Step.Change(
                "直接修改文件并保留备份",
                "sed -i.bak 's/^volume=.*/volume=80/' app.conf && cat app.conf && ls",
                "`-i.bak` 修改前把原文件备份为 `app.conf.bak`。").Then("`cat` 显示 `volume=80`；`ls` 显示 `app.conf` 与 `app.conf.bak` 两个文件。").Practice(),
            Step.Read(
                "对比修改前后",
                "diff app.conf.bak app.conf",
                "确认只改了预期的那一行。").Then("输出 `2c2`、`< volume=50`、`---`、`> volume=80`。"),
        ],

        Verify =
        [
            "1. `grep -c . app.conf` 与 `grep -c . app.conf.bak` 行数相同（5），说明没有误删行。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/sed-demo` 清理。",
        ],
    };
}
