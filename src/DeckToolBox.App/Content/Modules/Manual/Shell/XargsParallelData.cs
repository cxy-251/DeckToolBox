using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class XargsParallelData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-xargs-parallel",
        Title = "xargs 与 parallel：批量执行命令",
        Keywords = "xargs -0 · -n · -I{} · -P · parallel",
        Level = ModuleLevel.Intermediate,
        Overview = "很多命令只接受参数、不读取标准输入（如 rm、mv、ffmpeg）。xargs 把输入的每一行变成参数交给命令执行；GNU parallel 做同样的事，但能同时运行多个任务，适合批量转码、压缩这类耗时工作。两者 SteamOS 都自带。",

        Why =
        [
            "1. xargs 的基本行为：读取标准输入，按空白切分成参数，尽量多地拼到命令后面一次执行，例如 `find ... | xargs rm` 相当于 `rm 文件1 文件2 ...`。",
            "2. 常用选项：",
            "- `-n 1`：每次只传 1 个参数（命令执行多次）。",
            "- `-I {}`：把输入的每一行放到 `{}` 所在的位置，可以出现在命令中间，如 `xargs -I{} cp {} /备份/`。",
            "- `-0`：以空字符分隔输入，配合 `find -print0` 使用，文件名含空格、换行也不会出错。**处理文件名时应始终使用 `-print0 | xargs -0`。**",
            "- `-P 4`：同时运行 4 个进程。",
            "- `-p`：执行每条命令前询问。",
            "3. parallel：",
            "- `parallel 命令 {} ::: 参数1 参数2` 或 `... | parallel 命令 {}`。",
            "- 默认同时运行的任务数等于 CPU 线程数（Deck 为 8）；`-j 2` 限制为 2 个。",
            "- `{.}` 代表去掉扩展名的文件名，`{/}` 代表去掉目录的文件名，方便生成输出文件名。",
            "- 各任务的输出按任务分组显示，不会互相穿插；`--bar` 显示进度条。",
            "• 在终端中首次运行可能显示一段关于学术引用的说明，执行 `parallel --citation` 按提示确认后不再出现，不影响功能。",
        ],

        Consequences =
        [
            "• xargs 与 parallel 本身不改变任何东西，效果取决于它们执行的命令。与 rm、mv 组合前，先把命令换成 `echo` 演示一遍，例如 `... | xargs echo rm`。",
            "• 不加 `-0` 时，含空格的文件名会被拆成多个参数，可能误操作其他文件。",
            "• 并行任务数过多会占满 CPU 和内存，Deck 上建议不超过 4~6 个重负载任务，或者配合 `nice` 降低优先级。",
        ],

        Steps =
        [
            Step.Read(
                "把每一行变成参数",
                "printf '1\\n2\\n3\\n' | xargs -n1 echo 第",
                "`-n1` 每次传一个参数，echo 被执行三次。").Then("输出三行：`第 1`、`第 2`、`第 3`。"),
            Step.Read(
                "把参数放到命令中间",
                "printf 'Downloads\\nPictures\\n' | xargs -I{} du -sh ~/{}",
                "`-I{}` 用每一行替换 `{}`。").Then("输出两行，分别是 Downloads 与 Pictures 目录的大小。"),
            Step.Change(
                "安全地处理带空格的文件名",
                "rm -rf /tmp/xa-demo && mkdir -p /tmp/xa-demo && cd /tmp/xa-demo && touch 'a b.log' c.log && find . -name '*.log' -print0 | xargs -0 ls -l",
                "先删除上次练习留下的目录，重复练习时结果才与下面的说明一致。`-print0` 与 `-0` 以空字符分隔，空格不会拆开文件名。").Then("正确列出 `./a b.log` 与 `./c.log` 两个文件；若不用 `-0`，会报错找不到 `./a` 和 `b.log`。").Practice(),
            Step.Read(
                "先演示再执行",
                "find /tmp/xa-demo -name '*.log' -print0 | xargs -0 echo rm",
                "把危险命令前加上 echo，只打印将要执行的命令。").Then("输出一行 `rm /tmp/xa-demo/a b.log /tmp/xa-demo/c.log`，没有真正删除。"),
            Step.Read(
                "用 parallel 同时运行多个任务",
                "parallel -j 3 'sleep 1; echo 任务 {} 完成' ::: 1 2 3 4 5 6",
                "`-j 3` 同时 3 个；`:::` 后面是参数列表。").Then("约 2 秒后输出六行「任务 N 完成」（分两批，每批 3 个同时运行）；串行执行则需要 6 秒。"),
            Step.Change(
                "批量转换：把一批 WAV 转成 MP3（示例）",
                "parallel -j 4 --bar 'ffmpeg -loglevel error -i {} -q:a 2 {.}.mp3' ::: *.wav",
                "在存放 wav 文件的目录中执行；`{.}` 是去掉扩展名后的文件名。").Then("终端底部显示进度条；完成后每个 `x.wav` 旁边出现对应的 `x.mp3`，原文件保留。"),
        ],

        Verify =
        [
            "1. `time (seq 4 | parallel -j 4 sleep 1)` 的 real 时间约为 1 秒，而 `time (seq 4 | xargs -n1 sleep)` 约为 4 秒。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/xa-demo` 清理。",
        ],
    };
}
