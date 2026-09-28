using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class KeysHistoryData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-keys-history",
        Title = "命令行快捷键与历史",
        Keywords = "Ctrl+A/E/W/U/K · Alt+. · Ctrl+R · !! · !$ · history",
        Level = ModuleLevel.Basic,
        Overview = "在终端里编辑命令时，不必按着方向键一格一格移动。bash 提供了一组快捷键：跳到行首行尾、按词删除、调出上一条命令的参数、搜索历史。熟练后输入效率能提高很多，Deck 用外接键盘或 SSH 操作时尤其有用。",

        Why =
        [
            "1. 移动光标：",
            "- `Ctrl+A` / `Ctrl+E`：行首 / 行尾。",
            "- `Alt+B` / `Alt+F`：向前 / 向后移动一个词。",
            "2. 删除与粘贴：",
            "- `Ctrl+W`：删除光标前的一个词。",
            "- `Ctrl+U` / `Ctrl+K`：删除光标前 / 后的全部内容。",
            "- `Ctrl+Y`：粘贴刚才用上面三个快捷键删除的内容。",
            "3. 历史：",
            "- `↑` / `↓`：上一条 / 下一条命令。",
            "- `Ctrl+R`：反向搜索历史，输入关键词后反复按 `Ctrl+R` 找更早的匹配，回车执行，`→` 取出来编辑，`Ctrl+G` 放弃。",
            "- `Alt+.`：插入上一条命令的最后一个参数，反复按可以取更早的。",
            "4. 历史展开（以 `!` 开头，执行前由 bash 替换）：",
            "- `!!`：上一条完整命令，常用于 `sudo !!`（忘了加 sudo 时）。",
            "- `!$`：上一条命令的最后一个参数。",
            "- `!数字`：`history` 列表中对应编号的命令。",
            "5. 其他：`Ctrl+L` 清屏（等同 `clear`）；`Tab` 补全；`Ctrl+C` 放弃当前输入的整行。",
            "6. 历史记录保存在 `~/.bash_history`，在 shell 退出时写入；`HISTSIZE` 变量控制保留的条数。",
        ],

        Consequences =
        [
            "• `!!`、`!$` 会在按回车后立即被替换并执行，不再确认；bash 会先把替换后的命令打印出来。对危险命令不要使用历史展开。",
            "• 历史文件中会留下你输入过的所有内容。不要在命令行参数中直接写密码；以空格开头的命令不会被记录（前提是 `HISTCONTROL` 包含 `ignorespace`）。",
        ],

        Steps =
        [
            Step.Read(
                "练习：快速编辑一条长命令",
                "echo 第一个参数 第二个参数 第三个参数",
                "不要按回车。先按 `Ctrl+A` 跳到行首，`Ctrl+E` 回到行尾，按一次 `Ctrl+W` 删除最后一个词，再按 `Ctrl+Y` 粘贴回来，最后回车。").Then("光标随按键跳动，`Ctrl+W` 删除「第三个参数」，`Ctrl+Y` 又把它贴回；回车后输出完整的三个参数。"),
            Step.Read(
                "复用上一条命令的最后一个参数",
                "ls -ld ~/Downloads",
                "执行后，输入 `cd ` 再按 `Alt+.`（或输入 `cd !$`）然后回车。").Then("`Alt+.` 自动填入 `/home/deck/Downloads`，回车后当前目录切换到 Downloads。"),
            Step.Root(
                "忘记加 sudo 时",
                "sudo !!",
                "在一条因权限不足而失败的命令之后执行。").Then("bash 先打印展开后的完整命令（`sudo 上一条命令`），然后要求输入密码并执行它。"),
            Step.Read(
                "搜索历史命令",
                "history | grep -i ssh | tail -n 5",
                "也可以直接按 `Ctrl+R` 然后输入 ssh。").Then("输出最近 5 条包含 ssh 的历史命令，每行前是编号；用 `!编号` 可以再次执行其中某条。"),
            Step.Read(
                "查看历史设置",
                "echo \"HISTSIZE=$HISTSIZE HISTCONTROL=$HISTCONTROL\"; wc -l ~/.bash_history",
                "变量未设置时显示为空。").Then("输出当前的历史条数上限与控制选项，以及历史文件中已保存的行数。"),
        ],

        Verify =
        [
            "1. 输入 `echo abc`，回车后再输入 `echo !$`，输出 `abc`。",
        ],
    };
}
