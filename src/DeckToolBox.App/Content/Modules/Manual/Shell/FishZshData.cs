using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class FishZshData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-fish-zsh",
        Title = "fish 与 zsh：换一个 shell",
        Keywords = "fish · zsh · chsh · /etc/shells · Konsole 配置",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 除了默认的 bash，还自带 fish 和 zsh。fish 开箱即有自动建议、语法高亮和更友好的补全；zsh 与 bash 语法高度兼容，可配置性强。可以先临时试用，喜欢再设为默认。",

        Why =
        [
            "1. 三种 shell 的特点：",
            "- **bash**：几乎所有 Linux 的默认 shell，网上的教程和脚本大多按 bash 编写。",
            "- **fish**：输入时根据历史给出灰色的自动建议（按 `→` 接受），命令正确与否实时以颜色提示，补全带说明文字，无需任何配置。缺点是语法与 bash 不兼容（例如 `export A=1` 要写成 `set -x A 1`，`$(命令)` 要写成 `(命令)`，较新版本也支持 `$( )`）。",
            "- **zsh**：语法与 bash 大体兼容，补全和提示符可高度定制；不加配置时功能与 bash 接近，首次启动会运行配置向导。",
            "2. 如何切换：",
            "- 临时：直接输入 `fish` 或 `zsh` 进入，`exit` 回到 bash。",
            "- 只改 Konsole：Konsole → 设置 → 编辑当前配置方案 → 命令，改为 `/usr/bin/fish`。只影响 Konsole 新开的标签页，最稳妥。",
            "- 改登录 shell：`chsh -s /usr/bin/fish`，需要输入 deck 的密码，影响所有终端和 SSH 登录。",
            "3. `/etc/shells` 列出允许作为登录 shell 的程序，chsh 只接受其中的路径。",
            "4. 脚本不受影响：以 `#!/bin/bash` 开头的脚本总是由 bash 执行，与你的交互式 shell 无关。",
        ],

        Consequences =
        [
            "• 切换后，`~/.bashrc` 中的设置（PATH、别名、代理变量）不会自动生效：fish 读取 `~/.config/fish/config.fish`，zsh 读取 `~/.zshrc`，需要把设置迁移过去。",
            "• 从网上复制的 bash 命令在 fish 中可能报语法错误，可以临时输入 `bash` 回到 bash 执行。",
            "• 改回 bash：`chsh -s /bin/bash`。",
        ],

        Steps =
        [
            Step.Read(
                "查看可用的 shell 与当前登录 shell",
                "cat /etc/shells; getent passwd \"$USER\" | cut -d: -f7",
                "最后一行是你当前的登录 shell。").Then("列出 `/bin/bash`、`/usr/bin/fish`、`/bin/zsh` 等，最后一行 `/bin/bash`。"),
            Step.Read(
                "临时试用 fish",
                "fish",
                "进入后输入 `ls ~/Do`，观察灰色建议；按 `→` 接受。输入 `exit` 返回 bash。").Then("提示符变为 `deck@<主机名> ~>` 样式并显示欢迎语；输入命令时按颜色区分有效（蓝色）与无效（红色）的命令名。"),
            Step.Read(
                "对比 fish 与 bash 的语法",
                "fish -c 'set -x DEMO 你好; echo $DEMO; for i in 1 2; echo 第 $i 次; end'",
                "`-c` 执行一条 fish 命令后退出。").Then("输出 `你好`、`第 1 次`、`第 2 次`。同样的逻辑在 bash 中写作 `export DEMO=你好; for i in 1 2; do ...; done`。"),
            Step.Change(
                "把 ~/.local/bin 加入 fish 的 PATH",
                "fish -c 'fish_add_path ~/.local/bin'",
                "fish 的设置持久保存，不必写配置文件。").Then("没有输出；之后在 fish 中 `echo $PATH` 可以看到 `/home/deck/.local/bin`。"),
            Step.Change(
                "把登录 shell 改为 fish",
                "chsh -s /usr/bin/fish",
                "需要输入 deck 用户的密码。改回 bash：`chsh -s /bin/bash`。").Then("提示 `Shell changed.`（或无输出）；新开的终端与 SSH 登录都会进入 fish，已打开的终端不变。"),
        ],

        Verify =
        [
            "1. 新开一个终端，执行 `echo $SHELL`，输出 `/usr/bin/fish`（改为 fish 后）。",
        ],
    };
}
