using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.DisplayAndDesktop;

public static class XdgStandardsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "xdg-desktop",
        Title = "XDG 目录与 .desktop 菜单",
        Keywords = "XDG · .desktop · 菜单",
        Level = ModuleLevel.Basic,
        Overview = "XDG 是 freedesktop.org 制定的一组桌面规范：配置、数据、缓存分别放在哪里，应用菜单里的每一项是怎样定义的。遵循它，就能手动为便携应用添加菜单项、隐藏不需要的菜单项，而且这些做法在所有主流桌面上通用。",

        Why =
        [
            "1. 基础目录规范：",
            "- `$XDG_CONFIG_HOME`（默认 `~/.config`）：用户配置。",
            "- `$XDG_DATA_HOME`（默认 `~/.local/share`）：用户数据，其中 `applications/` 放用户自己的菜单项，`icons/` 放图标。",
            "- `$XDG_CACHE_HOME`（默认 `~/.cache`）：缓存，删除后程序会重新生成。",
            "2. `.desktop` 文件的常用字段：",
            "- `Type=Application`、`Name=`：类型与显示名称。",
            "- `Exec=`：启动命令，需写绝对路径；`%u`/`%f` 等表示传入的网址或文件。",
            "- `Icon=`：图标主题中的名称或图标文件的绝对路径。",
            "- `Categories=`：决定出现在哪个菜单分类。",
            "- `NoDisplay=true`：不在菜单中显示。",
            "- `StartupWMClass=`：让任务栏把运行中的窗口与这个菜单项对应起来，避免出现两个图标。",
            "3. 覆盖规则：`~/.local/share/applications/` 中的文件优先于 `/usr/share/applications/` 中的同名文件，因此不需要 root 就能修改或隐藏系统自带的菜单项。",
        ],

        Consequences =
        [
            "• 查看与验证命令只读。",
            "• 用户目录中的菜单项只影响你自己；删除对应的 `.desktop` 文件即可恢复原状。",
            "• KDE 的菜单编辑器（右键开始菜单 → 编辑应用程序）同样通过写入 `~/.local/share/applications` 实现；手写 `.desktop` 文件的好处是内容清楚可查、便于备份。",
        ],

        Steps =
        [
            Step.Read(
                "查看 XDG 目录变量（未设置时使用默认值）",
                "echo \"CONFIG=${XDG_CONFIG_HOME:-~/.config}  DATA=${XDG_DATA_HOME:-~/.local/share}  CACHE=${XDG_CACHE_HOME:-~/.cache}\"",
                "`${变量:-默认值}` 表示变量为空时使用默认值。").Then("输出一行，如 `CONFIG=/home/deck/.config  DATA=/home/deck/.local/share  CACHE=/home/deck/.cache`。"),
            Step.Read(
                "查看用户目录中的菜单项",
                "ls -la ~/.local/share/applications/",
                "这里的文件优先于 `/usr/share/applications` 中的同名文件。").Then("列出 `.desktop` 文件，包括你手工创建的、KDE 菜单编辑器生成的、Distrobox 导出的；目录为空时只显示 `.` 和 `..`。"),
            Step.Change(
                "为便携应用创建一个菜单项",
                "cat > ~/.local/share/applications/<程序名>.desktop <<'EOF'\n[Desktop Entry]\nType=Application\nName=<显示名称>\nExec=/home/deck/Applications/<程序名>/<可执行文件> %U\nIcon=<图标名或图标文件绝对路径>\nTerminal=false\nCategories=Utility;\nEOF",
                "`<程序名>` 建议用全小写英文；`<显示名称>` 为菜单显示名；`<可执行文件>` 填二进制绝对路径；`<图标名或图标文件绝对路径>` 填系统预置名或图片绝对路径。`Exec` 必须写绝对路径（`~` 不会被展开）。`Categories` 决定出现在菜单的哪个分类，可选 `Game;`、`Utility;`、`Network;`、`AudioVideo;`、`Development;` 等。").Then("没有输出，`~/.local/share/applications/` 下出现新的 `.desktop` 文件。几秒后 KDE 应用菜单的对应分类中出现这个程序。"),
            Step.Read(
                "检查 .desktop 文件的语法",
                "desktop-file-validate ~/.local/share/applications/<程序名>.desktop",
                "`<程序名>` 与上一步相同。命令由 desktop-file-utils 提供，按 XDG 规范检查必需的键和取值。").Then("格式正确时没有任何输出；有问题时每行一条 `error:` 或 `warning:` 说明哪个键写错了。"),
            Step.Change(
                "隐藏一个系统自带的菜单项（不删除程序）",
                "cp /usr/share/applications/<名称>.desktop ~/.local/share/applications/ && echo 'NoDisplay=true' >> ~/.local/share/applications/<名称>.desktop",
                "`<名称>` 可先用 `ls /usr/share/applications` 查得。用户目录副本优先于系统版本，通过追加 `NoDisplay=true` 隐藏图标而不改动系统包。").Then("没有输出。刷新菜单后该程序不再显示，但程序本身和它的文件关联仍然存在。"),
            Step.Change(
                "让 KDE 立即重新读取菜单",
                "kbuildsycoca6",
                "重建 KDE 的服务与菜单缓存。").Then("输出几行 `kbuildsycoca6 running...` 之类的信息后结束，应用菜单随即更新。"),
        ],

        Verify =
        [
            "1. 打开左下角的应用启动器，在对应分类下能找到新建的菜单项，点击可以正常启动。",
            "2. 被设置为 `NoDisplay=true` 的应用不再出现在菜单中；删除 `~/.local/share/applications` 里的副本后重新出现。",
        ],
    };
}
