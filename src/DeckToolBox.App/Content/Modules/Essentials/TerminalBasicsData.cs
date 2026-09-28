using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class TerminalBasicsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "terminal-basics",
        Title = "终端与命令行入门",
        Keywords = "Konsole · 路径 · Tab 补全 · --help",
        Level = ModuleLevel.Basic,
        Overview = "本应用里的所有操作都在终端完成。先掌握几件事：命令的基本结构、当前目录与路径、Tab 补全与历史记录、怎么查看帮助，以及卡住时怎么退出。",

        Why =
        [
            "1. 终端与 Shell：Konsole 是终端窗口，窗口里真正解释命令的程序是 Shell，SteamOS 默认是 **bash**。提示符 `(deck@steamdeck ~)$` 中的 `~` 表示当前位于家目录 `/home/deck`。",
            "2. 命令的结构：`命令 选项 参数`，例如 `ls -l ~/Downloads`：",
            "- 短选项用一个减号，可以合并：`ls -la` 等于 `ls -l -a`。",
            "- 长选项用两个减号：`ls --all`。",
            "- 参数是命令要处理的对象，通常是文件或目录。",
            "3. 路径：",
            "- 绝对路径从 `/` 开始，例如 `/home/deck/Downloads`。",
            "- 相对路径从当前目录算起：`.` 是当前目录，`..` 是上一级，`~` 是家目录。",
            "- Linux 区分大小写：`Downloads` 和 `downloads` 是两个不同的目录。含空格的路径要加引号：`\"My Games\"`。",
            "4. 查看帮助：SteamOS 为节省空间没有安装 man 手册，绝大多数命令都支持 `--help`；bash 内建命令（`cd`、`export` 等）用 `help <命令>`。",
            "5. 最有用的快捷键：",
            "- `Tab`：补全命令名和路径，按两次列出所有候选。",
            "- `↑` / `↓`：翻看历史命令；`Ctrl+R`：搜索历史命令。",
            "- `Ctrl+C`：中断正在运行的命令；`Ctrl+D`：结束输入或退出 shell。",
            "- Konsole 中复制粘贴是 `Ctrl+Shift+C` / `Ctrl+Shift+V`（`Ctrl+C` 已被用来中断命令）。",
        ],

        Consequences =
        [
            "• 本模块的命令全部只读，放心尝试。",
            "• 终端没有「回收站」：`rm` 删除的文件无法找回。执行删除、覆盖类命令前，先用 `ls` 确认目标。",
            "• 粘贴多行命令时，Konsole 会弹窗确认，这是防止误执行的保护，看清内容再确认。",
            "• 在 Deck 游戏模式下没有物理键盘时，按 `Steam + X` 调出屏幕键盘。",
        ],

        Steps =
        [
            Step.Read(
                "显示当前所在目录",
                "pwd",
                "print working directory。"),
            Step.Read(
                "列出目录内容（含隐藏文件与详细信息）",
                "ls -lah ~",
                "`-l` 详细信息，`-a` 包括以 `.` 开头的隐藏文件，`-h` 用 K/M/G 显示大小。"),
            Step.Read(
                "切换目录并返回",
                "cd ~/Downloads && pwd && cd - && pwd",
                "`cd -` 回到上一个目录；单独输入 `cd` 回到家目录。"),
            Step.Read(
                "查看一个命令的帮助",
                "ls --help | head -n 30",
                "`| head -n 30` 只显示前 30 行；想完整翻阅可改为 `| less`（空格翻页，`q` 退出）。"),
            Step.Read(
                "查看一个命令是什么、在哪里",
                "type cd ls; command -v nano",
                "`type` 会说明它是 shell 内建命令、别名还是外部程序。"),
            Step.Read(
                "搜索最近用过的命令",
                "history | tail -n 20",
                "也可以直接按 `Ctrl+R` 然后输入关键词，回车执行找到的命令。"),
        ],

        Verify =
        [
            "1. 输入 `cd ~/Dow` 后按 `Tab`，自动补全为 `cd ~/Downloads/`。",
            "2. 执行 `sleep 100`，再按 `Ctrl+C`，命令立即结束并回到提示符。",
        ],
    };
}
