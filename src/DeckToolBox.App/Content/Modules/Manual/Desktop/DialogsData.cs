using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Desktop;

public static class DialogsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-dialogs",
        Title = "kdialog 与 zenity：给脚本加对话框",
        Keywords = "kdialog · zenity · --yesno · --inputbox · --getopenfilename · 进度条",
        Level = ModuleLevel.Intermediate,
        Overview = "脚本也可以有图形界面：弹出提示、询问是否继续、让用户输入文字或选择文件、显示进度。kdialog（KDE 风格）和 zenity（GTK 风格）都是 SteamOS 自带的。给自己写的脚本加上对话框后，把它添加到 Steam 或桌面菜单，不打开终端也能使用，也方便在游戏模式中用手柄操作。",

        Why =
        [
            "1. 结果如何返回：",
            "- 按钮类对话框通过退出码返回：`0` 表示「是/确定」，`1` 表示「否/取消」。",
            "- 输入类对话框把用户输入的内容输出到标准输出，用 `$( )` 接收。",
            "2. kdialog 常用类型：",
            "- `--msgbox 文字`、`--error 文字`、`--sorry 文字`：提示框。",
            "- `--yesno 文字`：是/否。",
            "- `--inputbox 提示 默认值`、`--password 提示`：输入。",
            "- `--getopenfilename 起始目录`、`--getexistingdirectory`：选择文件、目录。",
            "- `--menu 提示 键1 显示1 键2 显示2`：从列表中选一项。",
            "- `--passivepopup 文字 秒数`：不需要点击的气泡提示。",
            "- `--progressbar`：进度条，通过 qdbus 更新（较复杂）。",
            "- `--title` 设置窗口标题。",
            "3. zenity 写法略有不同：`--info --text=`、`--question --text=`、`--entry --text=`、`--file-selection`，进度条可以直接从标准输入读取百分比（`--progress`），写起来更简单。",
        ],

        Consequences =
        [
            "• 对话框只在图形会话中能弹出；通过 SSH 或定时任务运行时找不到显示，会直接失败。",
            "• `--password` 获取的密码会存在脚本变量中，处理完后不要打印或写入文件。",
        ],

        Steps =
        [
            Step.Change(
                "弹出一个提示框",
                "kdialog --title 'DeckToolBox' --msgbox '备份已完成'",
                "点击确定后命令结束。").Then("桌面弹出一个标题为 DeckToolBox 的提示框，点击「确定」后终端返回。"),
            Step.Change(
                "询问是否继续，并根据结果执行",
                "if kdialog --yesno '要清空缩略图缓存吗？'; then echo 你选择了是; else echo 你选择了否; fi",
                "退出码 0 为「是」。").Then("弹出带「是」「否」按钮的对话框；点击后终端输出对应的一行文字。"),
            Step.Change(
                "让用户输入文字",
                "name=$(kdialog --inputbox '请输入备份名称' \"backup-$(date +%F)\") && echo \"输入的是: $name\"",
                "第二个参数是输入框的默认值。").Then("弹出输入框，默认填好 `backup-<日期>`；点击确定后终端输出输入的内容，点击取消则不输出。"),
            Step.Change(
                "让用户选择一个文件",
                "f=$(kdialog --getopenfilename ~/Downloads '*.zip *.7z') && echo \"选择了: $f\"",
                "第二个参数是文件类型过滤。").Then("弹出 KDE 的文件选择窗口，只显示 zip 与 7z 文件；选择后终端输出完整路径。"),
            Step.Change(
                "不打断操作的气泡提示",
                "kdialog --passivepopup '下载完成' 5",
                "5 秒后自动消失。").Then("屏幕角落出现一个气泡通知，5 秒后自动消失，不需要点击。"),
            Step.Change(
                "用 zenity 显示进度条",
                "(for i in 20 40 60 80 100; do echo $i; echo \"# 正在处理 $i%\"; sleep 1; done) | zenity --progress --title='处理中' --auto-close",
                "向 zenity 的标准输入写入数字即更新百分比，以 `#` 开头的行更新提示文字。").Then("弹出进度条窗口，每秒前进 20%，提示文字同步变化，到 100% 后自动关闭。"),
        ],

        Verify =
        [
            "1. `kdialog --yesno 测试; echo $?` 点击「是」输出 0，点击「否」输出 1。",
        ],
    };
}
