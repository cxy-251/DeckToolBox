using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SystemHygiene;

public static class PackageManagementData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "flatpak",
        Title = "不用 Flatpak：清理与替代",
        Keywords = "flatpak · Discover · 便携应用 · Distrobox",
        Level = ModuleLevel.Basic,
        Overview = "SteamOS 默认用 Flatpak（Discover 商店）安装图形应用。本应用的立场是不使用它：把 Flatpak 清理干净，并用便携应用和 Distrobox 容器覆盖同样的需求。这里讲清楚常见的取舍、如何确认已彻底清理，以及替代方案怎么选。",

        Why =
        [
            "1. 选择不用 Flatpak 的常见理由：",
            "- **体积**：每个应用依赖一套运行时（`org.kde.Platform`、`org.freedesktop.Platform` 等），不同应用依赖不同运行时或不同版本时，会并存多套，动辄数 GB。",
            "- **沙箱摩擦**：应用默认看不到 `~/Games`、SD 卡等目录，需要逐个调整权限；给 Mod 工具、文件管理类程序带来额外麻烦。",
            "- **不透明**：应用数据在 `~/.var/app/<AppID>`，程序实际运行在另一套运行时里，排查问题时多了一层。",
            "- **残留**：卸载应用默认不删除数据，运行时也要单独回收。",
            "2. Flatpak 在 SteamOS 上的几处痕迹：",
            "- 软件源 (remote)：`flathub` 等，定义从哪里下载应用。",
            "- `/var/lib/flatpak`（系统级安装）与 `~/.local/share/flatpak`（用户级安装）：应用、运行时和仓库元数据。",
            "- `~/.var/app/`：每个应用的配置、缓存和数据。",
            "- Discover 商店：只是 Flatpak 的图形前端，没有软件源时它就装不了任何应用。",
            "3. 替代方案：",
            "- **官方便携版 / AppImage** → 放进 `~/Applications`（见「便携应用与 AppImage」）。浏览器、聊天工具、编辑器等大多提供 tar.gz 或 AppImage 版本。",
            "- **Distrobox 容器** → 用 Arch 容器里的 `pacman` 或 AUR 安装，再用 `distrobox-export --app` 放进应用菜单（见「Distrobox 用户容器」）。适合没有便携版的软件和命令行工具。",
            "- **SteamOS 自带的程序** → 系统镜像已包含不少工具（见入门必修的「命令行工具箱」各模块），先确认是否已经有了。",
        ],

        Consequences =
        [
            "• 查看状态的命令（`list`、`remotes`、`du`）完全只读。",
            "• 删除软件源后，Discover 里将搜不到任何应用；Steam、游戏和 `~/Applications` 下的程序不受影响。",
            "• **删除 `~/.var/app/<AppID>` 会永久删除该应用的配置和本地数据**，确认不再需要再删。",
            "• 修改系统级的 Flatpak 安装（`/var/lib/flatpak`）需要 sudo。SteamOS 系统更新后，建议用本模块的检查命令复查一遍，确认没有重新出现软件源。",
            "• 替代方案各有代价：便携应用不会自动更新，需要自己关注新版本；Distrobox 容器不是安全沙箱（能读写整个家目录）。",
        ],

        Steps =
        [
            Step.Read(
                "确认没有安装任何 Flatpak 应用和运行时",
                "flatpak list --app; flatpak list --runtime",
                "分别列出应用和运行时。").Then("两条都没有输出，表示应用和运行时都已清空；否则列出剩余的 AppID。"),
            Step.Read(
                "查看还剩哪些软件源",
                "flatpak remotes --show-details",
                "`system` 表示系统级源，`user` 表示用户级源。").Then("每个软件源一行，例如 `flathub-beta  Flathub Beta  https://dl.flathub.org/beta-repo/ ... system`；没有输出表示所有源都已删除。"),
            Step.Root(
                "删除系统级软件源",
                "sudo flatpak remote-delete <源名称>",
                "例如 `flathub`、`flathub-beta`。用户级源不需要 sudo，写作 `flatpak remote-delete --user <源名称>`。").Then("输入 sudo 密码后没有输出即成功。之后 `flatpak remotes` 中不再有这个源，Discover 也无法再从它安装应用。"),
            Step.Read(
                "查看 Flatpak 目录的剩余占用",
                "du -sh /var/lib/flatpak ~/.local/share/flatpak ~/.var/app 2>/dev/null",
                "没有应用时，这里剩下的主要是仓库元数据与缓存。").Then("每个目录一行，例如 `110M  /var/lib/flatpak`。不存在的目录不显示。"),
            Step.Change(
                "回收已无用的运行时与仓库对象",
                "flatpak uninstall --unused -y; flatpak repair --user",
                "`--unused` 删除没有应用在用的运行时；`repair` 清理仓库中不再被引用的对象。").Then("没有可回收的内容时提示 `Nothing unused to uninstall`；`repair` 会逐项检查并输出清理结果，完成后相应目录变小。"),
            Step.Read(
                "找出「应用已卸载、数据还留着」的目录",
                "comm -13 <(flatpak list --app --columns=application | sort) <(ls ~/.var/app | sort)",
                "`comm -13` 输出只在右侧出现的行。").Then("每行一个 AppID，例如 `com.google.Chrome`，表示 `~/.var/app` 里还留着它的数据；没有输出表示没有遗留。"),
            Step.Danger(
                "删除一个遗留的应用数据目录",
                "rm -rf ~/.var/app/<AppID>",
                "只对上一步列出的 AppID 执行，逐个确认。").Then("没有输出。该目录及其中的配置、缓存被永久删除，不经过回收站。"),
            Step.Read(
                "查看已有的便携应用",
                "ls ~/Applications",
                "这里是 Flatpak 的主要替代位置。").Then("列出 `~/Applications` 下的目录和文件名，例如 `chrome`。目录不存在时提示「没有那个文件或目录」。"),
            Step.Read(
                "查看已有的 Distrobox 容器与导出的菜单项",
                "distrobox list; grep -l 'distrobox' ~/.local/share/applications/*.desktop 2>/dev/null",
                "第二条列出由容器导出到应用菜单的程序。").Then("先是容器表格（ID、名称、状态、镜像），然后列出由容器导出的 `.desktop` 文件路径；都没有时只显示表头。"),
        ],

        Verify =
        [
            "1. `flatpak list` 与 `flatpak remotes` 均无输出，说明 Flatpak 已完全停用。",
            "2. 打开 Discover，搜索任意应用都没有结果。",
            "3. 你常用的图形程序都能从应用菜单启动，且来源是 `~/Applications` 或 Distrobox 导出的菜单项。",
        ],
    };
}
