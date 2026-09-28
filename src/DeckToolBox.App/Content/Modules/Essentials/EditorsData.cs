using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class EditorsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "editors",
        Title = "在终端编辑文件：nano 与 vim",
        Keywords = "nano · vim · sudoedit · EDITOR",
        Level = ModuleLevel.Basic,
        Overview = "修改配置文件是 Linux 的日常。SteamOS 自带 nano 和 vim：nano 屏幕底部就有快捷键提示，适合新手；vim 功能强大，至少要会「保存」和「退出」。编辑系统文件时用 sudoedit 更安全。",

        Why =
        [
            "1. nano：打开后直接可以输入。底部的 `^` 表示 Ctrl：",
            "- `Ctrl+O` 回车：保存；`Ctrl+X`：退出（有未保存修改时会询问）。",
            "- `Ctrl+W`：搜索；`Ctrl+K` / `Ctrl+U`：剪切 / 粘贴整行；`Alt+U`：撤销。",
            "2. vim 的「模式」：打开时处于普通模式，按键是命令而不是文字：",
            "- `i`：进入插入模式开始输入；`Esc`：回到普通模式。",
            "- 普通模式下 `:w` 回车保存，`:q` 退出，`:wq` 保存并退出，`:q!` 不保存强制退出。",
            "- `/关键词` 搜索，`n` 下一个；`u` 撤销；`dd` 删除一行。",
            "- 看不懂当前状态时，先按几次 `Esc`，再输入 `:q!` 回车，就能安全退出。",
            "3. 编辑系统文件：用 `sudoedit <文件>` 而不是 `sudo nano <文件>`：",
            "• sudoedit 先把文件复制到临时位置，用**你自己的身份**运行编辑器，保存后再以 root 写回。编辑器本身不以 root 运行，更安全，也不会在你的家目录里留下属于 root 的配置文件。",
            "4. `EDITOR` 环境变量决定 `sudoedit`、`git` 等程序调用哪个编辑器，可在 `~/.bashrc` 中设置 `export EDITOR=nano`。",
        ],

        Consequences =
        [
            "• 保存即生效，编辑器不会替你备份。修改重要配置前先 `cp <文件> <文件>.bak`。",
            "• SteamOS 的 `/usr` 只读，其中的文件无法保存；`/etc` 可以修改，改动保存在覆盖层中，系统更新后保留。",
        ],

        Steps =
        [
            Step.Change(
                "用 nano 新建并编辑一个文件",
                "nano ~/notes.txt",
                "输入几行文字，`Ctrl+O` 回车保存，`Ctrl+X` 退出。"),
            Step.Change(
                "用 vim 打开同一个文件",
                "vim ~/notes.txt",
                "按 `i` 输入，`Esc` 后输入 `:wq` 回车保存退出。"),
            Step.Change(
                "把默认编辑器设为 nano",
                "echo 'export EDITOR=nano' >> ~/.bashrc && source ~/.bashrc",
                "之后 `sudoedit` 等命令都会使用 nano。"),
            Step.Root(
                "安全地编辑系统配置文件",
                "sudoedit /etc/<配置文件>",
                "例如 `/etc/hosts`。修改前先用 `sudo cp /etc/<配置文件> /etc/<配置文件>.bak` 备份。"),
        ],

        Verify =
        [
            "1. `cat ~/notes.txt` 显示你输入的内容。",
            "2. `echo $EDITOR` 输出 `nano`。",
        ],
    };
}
