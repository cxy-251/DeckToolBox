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
            "• 避免黑盒菜单编辑器：KDE 自带的 kmenuedit 会在 ~/.config/menus/ 生成复杂的 XML 过滤器，极易导致菜单配置混乱失效。手工编写单个 .desktop 文件是最干净可控的工业级做法。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前用户已有的个性化菜单项",
                "ls -la ~/.local/share/applications/",
                "列出用户层的所有自定义 .desktop 快捷项。"),
            Step.Change(
                "为绿色应用编写一个标准规范的 .desktop 启动模版",
                "cat <<'EOF' > ~/.local/share/applications/my-custom-app.desktop\n[Desktop Entry]\nVersion=1.0\nType=Application\nName=我的独立应用\nComment=自包含绿色软件示例\nExec=/home/deck/Applications/MyTool/start.sh %u\nIcon=applications-games\nTerminal=false\nCategories=Game;\nStartupNotify=true\nEOF",
                "直接写入规范文本，不经过任何黑盒转换。"),
            Step.Change(
                "刷新系统桌面数据库（使改动即刻在开始菜单生效）",
                "update-desktop-database ~/.local/share/applications/",
                "通知 KDE Plasma 重新索引启动项。"),
        ],

        Verify =
        [
            "执行 update-desktop-database 后，点击屏幕左下角的 Application Launcher (开始菜单)，即可看到新添加的软件已经整齐排列在指定分类下。",
        ],
    };
}
