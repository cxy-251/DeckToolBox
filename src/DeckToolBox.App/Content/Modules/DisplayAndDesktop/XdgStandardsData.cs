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
        Overview = "掌握 freedesktop.org 规范标准，彻底弄懂 XDG 目录层次与 .desktop 启动项语法，拒绝黑盒菜单编辑器。",

        Why =
        [
            "1. XDG 基础目录规范 (Base Directory Specification)：",
            "• 数据存储 ($XDG_DATA_HOME)：默认指向 ~/.local/share/。用于放置应用数据、用户级桌面启动项 (~/.local/share/applications/) 和图标。",
            "• 配置存储 ($XDG_CONFIG_HOME)：默认指向 ~/.config/。放置 ini/json/conf 等用户偏好配置。",
            "• 缓存存储 ($XDG_CACHE_HOME)：默认指向 ~/.cache/。即使全量删除也不会导致应用损坏的数据。",
            "2. .desktop 桌面启动规范关键字段详解：",
            "• Type=Application：声明这是一个应用程序。",
            "• Exec=可执行绝对路径 %u：启动命令与参数宏传递。",
            "• Icon=图标标识或绝对路径：指向系统主题图标名称或 png/svg 绝对路径。",
            "• Categories=Game;Utility;Development;：决定它在 KDE 开始菜单中的分类树位置。",
            "• NoDisplay=true：优雅隐藏此项，菜单中不显示，但不破坏关联。",
            "• StartupWMClass=类名：至关重要！让 KDE 任务栏将运行中的窗口与启动图标正确归类聚合，防止任务栏出现双图标。",
        ],

        Consequences =
        [
            "• 用户空间优先级最高：~/.local/share/applications/ 里的同名 .desktop 文件会自动覆盖系统级 /usr/share/applications/ 的设置，因此我们无需任何 root 权限就能自由定制菜单。",
            "• KDE 的菜单编辑器（右键开始菜单 → 编辑应用程序）同样通过写入 `~/.local/share/applications` 实现；手写 `.desktop` 文件的好处是改了什么一目了然、便于备份。",
        ],

        Steps =
        [
            Step.Read(
                "查看 XDG 目录变量（未设置时使用默认值）",
                "echo \"CONFIG=${XDG_CONFIG_HOME:-~/.config}  DATA=${XDG_DATA_HOME:-~/.local/share}  CACHE=${XDG_CACHE_HOME:-~/.cache}\"",
                "`${变量:-默认值}` 表示变量为空时使用默认值。").Then("输出一行，如 `CONFIG=/home/deck/.config  DATA=/home/deck/.local/share  CACHE=/home/deck/.cache`。"),
            Step.Read(
                "查看你自定义或覆盖过的菜单项",
                "ls -la ~/.local/share/applications/",
                "这里的文件优先于 `/usr/share/applications` 中的同名文件。").Then("列出 `.desktop` 文件，包括你手工创建的、KDE 菜单编辑器生成的、Distrobox 导出的；目录为空时只显示 `.` 和 `..`。"),
            Step.Change(
                "为便携应用创建一个菜单项",
                "cat > ~/.local/share/applications/<程序名>.desktop <<'EOF'\n[Desktop Entry]\nType=Application\nName=<显示名称>\nExec=/home/deck/Applications/<程序名>/<可执行文件> %U\nIcon=<图标名或图标文件绝对路径>\nTerminal=false\nCategories=Utility;\nEOF",
                "`Exec` 必须写绝对路径（`~` 不会被展开）。`Categories` 决定出现在菜单的哪个分类，可选 `Game;`、`Utility;`、`Network;`、`AudioVideo;`、`Development;` 等。").Then("没有输出，`~/.local/share/applications/` 下出现新的 `.desktop` 文件。几秒后 KDE 应用菜单的对应分类中出现这个程序。"),
            Step.Read(
                "检查 .desktop 文件的语法",
                "desktop-file-validate ~/.local/share/applications/<程序名>.desktop",
                "由 desktop-file-utils 提供。").Then("格式正确时没有任何输出；有问题时每行一条 `error:` 或 `warning:` 说明哪个键写错了。"),
            Step.Change(
                "隐藏一个系统自带的菜单项（不删除程序）",
                "cp /usr/share/applications/<名称>.desktop ~/.local/share/applications/ && echo 'NoDisplay=true' >> ~/.local/share/applications/<名称>.desktop",
                "用户目录中的同名副本覆盖系统版本。恢复显示只需删除这个副本。先用 `ls /usr/share/applications` 找到文件名。").Then("没有输出。刷新菜单后该程序不再显示，但程序本身和它的文件关联仍然存在。"),
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
