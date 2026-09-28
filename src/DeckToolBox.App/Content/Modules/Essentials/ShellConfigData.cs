using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class ShellConfigData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "shell-config",
        Title = "环境变量与 Shell 配置",
        Keywords = "PATH · ~/.bashrc · alias · export",
        Level = ModuleLevel.Basic,
        Overview = "为什么放进 ~/.local/bin 的程序有时找不到？为什么改了配置要重开终端？答案都在环境变量和 bash 的启动文件里。学会安全地修改它们，避免越改越乱。",

        Why =
        [
            "1. 环境变量：进程携带的一组「名称=值」，子进程会继承。常见的有：",
            "- `HOME`：家目录；`USER`：用户名；`LANG`：语言与编码。",
            "- `PATH`：用冒号分隔的目录列表。输入命令名时，shell 按顺序在这些目录里查找同名可执行文件。",
            "2. `export`：`VAR=值` 只在当前 shell 中有效；`export VAR=值` 才会传给之后启动的程序。在终端里 `export` 只影响这个终端窗口，关掉就没了。",
            "3. 永久生效靠启动文件：",
            "- `~/.bashrc`：每打开一个交互式终端时执行。别名 (`alias`)、提示符、大多数 `export` 放这里。",
            "- `~/.bash_profile`：登录 shell 执行（例如 SSH 登录）。常见写法是在里面 `source ~/.bashrc`，让两种情况行为一致。",
            "- 修改后对**新开的终端**生效；当前终端执行 `source ~/.bashrc` 立即加载。",
            "4. 重复追加的陷阱：每次都往 `.bashrc` 追加一行 `export PATH=\"$HOME/.local/bin:$PATH\"`，时间久了 PATH 里会出现多个相同目录。用 `case` 先判断是否已包含，再添加，就能重复执行也不会重复。",
            "5. 别名 (alias)：给常用命令起短名字，例如 `alias ll='ls -lah'`。",
        ],

        Consequences =
        [
            "• 查看变量、`type`、`alias` 都是只读的。",
            "• 修改 `~/.bashrc` 前先备份。语法错误会导致新终端启动时报错，但不会影响系统；用备份恢复即可。",
            "• `.bashrc` 只影响终端里的 bash，不影响从 KDE 菜单或 Steam 启动的程序。给图形程序设置环境变量应使用 `~/.config/environment.d/*.conf`（重新登录后生效）。",
        ],

        Steps =
        [
            Step.Read(
                "查看 PATH，每个目录一行",
                "echo \"$PATH\" | tr ':' '\\n'",
                "如果同一个目录出现多次，说明启动文件里有重复的添加语句。"),
            Step.Read(
                "查看全部环境变量",
                "env | sort | less",
                "按 `q` 退出。只看某一个：`echo \"$LANG\"`。"),
            Step.Read(
                "找出启动文件中修改 PATH 的所有行",
                "grep -n 'PATH' ~/.bashrc ~/.bash_profile ~/.profile 2>/dev/null",
                "检查是否有重复或冲突的写法。"),
            Step.Change(
                "修改前备份 .bashrc",
                "cp ~/.bashrc ~/.bashrc.bak-$(date +%F)",
                "出问题时用 `cp ~/.bashrc.bak-<日期> ~/.bashrc` 恢复。"),
            Step.Change(
                "以「不重复」的方式把 ~/.local/bin 加入 PATH",
                "cat >> ~/.bashrc <<'EOF'\n\n# 把 ~/.local/bin 加入 PATH（已存在则跳过）\ncase \":$PATH:\" in\n  *\":$HOME/.local/bin:\"*) ;;\n  *) export PATH=\"$HOME/.local/bin:$PATH\" ;;\nesac\nEOF",
                "`case` 先检查 PATH 中是否已有该目录。追加前先确认 `.bashrc` 里没有同样的语句，已有就不要重复执行这一步。"),
            Step.Change(
                "添加一个别名并立即生效",
                "echo \"alias ll='ls -lah'\" >> ~/.bashrc && source ~/.bashrc && type ll",
                "`source` 在当前 shell 中重新执行 .bashrc。"),
        ],

        Verify =
        [
            "1. 新开一个终端，`echo \"$PATH\" | tr ':' '\\n' | sort | uniq -d` 无输出，说明 PATH 中没有重复目录。",
            "2. 输入 `ll` 等同于 `ls -lah`；`type ll` 显示 `ll 是 \"ls -lah\" 的别名`。",
        ],
    };
}
