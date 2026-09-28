using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Text;

public static class ViewTextData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-view-text",
        Title = "cat、less、head、tail：查看文件内容",
        Keywords = "cat · less · head -n · tail -f · nl · tac",
        Level = ModuleLevel.Basic,
        Overview = "查看文本文件最常用的四个命令：cat 一次输出全部，less 分页浏览并可搜索，head 和 tail 看开头和结尾。tail -f 可以持续跟踪不断增长的日志文件。",

        Why =
        [
            "1. cat：把文件内容原样输出到终端，适合短文件；也用于把多个文件连接起来（cat = concatenate）。",
            "- `-n` 显示行号；`-A` 显示不可见字符（行尾 `$`、制表符 `^I`、Windows 换行 `^M`）。",
            "2. less：分页查看，不会一次把长文件刷满屏幕。常用按键：",
            "- 空格 / `b`：下一页 / 上一页；`↑` `↓`：逐行滚动。",
            "- `g` / `G`：跳到开头 / 结尾。",
            "- `/关键词` 回车：向下搜索，`n` 下一个，`N` 上一个；`?关键词` 向上搜索。",
            "- `F`：像 `tail -f` 一样持续跟踪文件末尾，`Ctrl+C` 停止跟踪。",
            "- `q`：退出。",
            "3. head / tail：",
            "- `head -n 20 文件`：前 20 行；`tail -n 20 文件`：最后 20 行。",
            "- `tail -n +5 文件`：从第 5 行开始到结尾。",
            "- `tail -f 文件`：显示末尾后持续等待，文件有新内容就立即显示，`Ctrl+C` 退出。看日志最常用。",
            "4. 其他：`nl` 给每行编号；`tac` 反向输出（最后一行在前）。",
        ],

        Consequences =
        [
            "• 这些命令只读取文件。",
            "• 用 cat 查看二进制文件（程序、图片）会在终端输出乱码，甚至改变终端状态；出现这种情况时输入 `reset` 回车恢复。先用 `file` 确认是文本再查看。",
            "• `cat 文件 > 同一个文件` 会先清空文件，导致内容丢失。",
        ],

        Steps =
        [
            Step.Read(
                "输出一个短文件",
                "cat /etc/os-release",
                "适合几十行以内的文件。").Then("终端一次性打印出全部内容：`NAME=\"SteamOS\"`、`ID=steamos` 等十几行。"),
            Step.Read(
                "带行号，并显示不可见字符",
                "cat -n ~/.bash_profile | head; cat -A ~/.bashrc | head -n 3",
                "`-A` 让行尾显示为 `$`、制表符显示为 `^I`。").Then("第一部分每行前有行号；第二部分每行末尾出现 `$`。若文件来自 Windows，行尾会显示 `^M$`。"),
            Step.Read(
                "分页浏览并搜索",
                "less /etc/passwd",
                "进入后输入 `/deck` 回车搜索，`n` 找下一个，`q` 退出。").Then("终端切换到全屏的分页界面，底部显示文件名或 `:` 提示符；搜索到的 `deck` 高亮显示。退出后终端恢复原样，不留下文件内容。"),
            Step.Read(
                "看文件开头与结尾",
                "head -n 3 /etc/passwd; echo ...; tail -n 3 /etc/passwd",
                "`-n` 指定行数，默认 10 行。").Then("先输出 `/etc/passwd` 的前 3 行（第一行是 `root:x:0:0:...`），`...` 之后是最后 3 行。"),
            Step.Read(
                "从某一行开始显示",
                "tail -n +10 /etc/os-release",
                "`+10` 表示从第 10 行开始。").Then("跳过前 9 行，输出剩余内容。"),
            Step.Read(
                "持续跟踪日志文件",
                "tail -f ~/.local/share/Steam/logs/console_log.txt",
                "在 Steam 中操作（例如打开商店页面），观察新日志出现。`Ctrl+C` 退出。").Then("先显示末尾 10 行，然后停住等待；每当 Steam 写入新内容，它就立刻出现在底部。"),
            Step.Read(
                "反向查看：最新的行在前",
                "tac ~/.bash_history 2>/dev/null | head -n 5",
                "bash 历史文件按时间追加，反向后最新的在前。").Then("输出最近执行过的 5 条命令（写入历史文件的），最新的在第一行。"),
        ],

        Verify =
        [
            "1. `head -n 1 /etc/passwd` 与 `sed -n 1p /etc/passwd` 输出相同的一行。",
        ],
    };
}
