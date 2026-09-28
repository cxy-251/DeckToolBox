using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SystemHygiene;

public static class PackageManagementData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "flatpak",
        Title = "Flatpak 应用管理",
        Keywords = "flatpak · Discover · 权限 · 清理",
        Level = ModuleLevel.Basic,
        Overview = "Flatpak 是 SteamOS 官方的图形应用安装方式，Discover 商店背后就是它。本模块讲清怎么装、怎么收紧或放宽沙箱权限、怎么回收空间；如果你决定不用 Flatpak，也讲清怎么干净地移除。",

        Why =
        [
            "1. 为什么 SteamOS 上装软件要走 Flatpak：",
            "• 根分区只读，而且每次系统更新都会整体替换（见「只读系统与 A/B 更新」）。用 `pacman` 装进 `/usr` 的软件会在下次更新后消失。",
            "• Flatpak 应用装在 `/var/lib/flatpak`（系统级）或 `~/.local/share/flatpak`（用户级，`--user`）。在 SteamOS 上这两处的数据都落在 `/home` 分区，系统更新不会碰。",
            "2. 运行时 (Runtime) 共享：",
            "• 应用依赖 `org.kde.Platform`、`org.freedesktop.Platform`、`org.gnome.Platform` 等运行时。第一次装某一系列的应用会顺带下载几百 MB 到 1~2 GB 的运行时，之后同系列应用复用它。",
            "• 应用卸载后运行时不会自动删除，需要 `flatpak uninstall --unused` 回收。",
            "3. 沙箱与门户 (Portal)：",
            "• 应用运行在 bubblewrap 沙箱里，默认只能看到自己被授权的目录。打开文件时走 XDG Desktop Portal 的文件选择框，由你选择的文件才会传给应用。",
            "• 所以浏览器、下载工具「看不到 SD 卡或 `~/Games`」是设计如此。可以用 `flatpak override` 为单个应用放行指定目录，而不是整体关掉沙箱。",
            "4. 数据位置：每个应用的配置与缓存都在 `~/.var/app/<AppID>/`。**卸载应用时默认不删除这里**，需要手动或用 `--delete-data` 清理。",
        ],

        Consequences =
        [
            "• 查询类命令（`list`、`info`、`search`）完全只读。",
            "• `flatpak install` / `update` 只写 Flatpak 自己的目录，不触碰只读根分区，随时可以卸载。",
            "• `flatpak uninstall --unused` 只删除没有任何应用在用的运行时，是安全的回收操作。",
            "• `--filesystem=` 覆盖会削弱该应用的隔离：只放行真正需要的目录（例如 `/run/media/deck`），不要随手给 `host` 或 `home`。`--reset` 可一键撤销。",
            "• **`--delete-data` 和删除 `~/.var/app/<AppID>` 会永久删除该应用的配置、登录状态和本地数据**（例如浏览器书签若未同步就会丢失），执行前确认。",
            "• 如果你选择完全不用 Flatpak：Steam 本体、游戏和 `~/Applications` 下的便携应用都不受影响；以后想用时重新添加 flathub 源即可。",
        ],

        Steps =
        [
            Step.Read(
                "列出已安装的应用（含安装位置与占用）",
                "flatpak list --app --columns=application,version,installation,size",
                "`installation` 列为 system 或 user，决定卸载时是否需要加 `--user`。"),
            Step.Read(
                "查看已配置的软件源",
                "flatpak remotes --show-details",
                "SteamOS 默认带 `flathub`。如果列表里没有它，Discover 里也就搜不到应用。"),
            Step.Change(
                "（可选）重新添加 Flathub 官方源",
                "flatpak remote-add --if-not-exists flathub https://dl.flathub.org/repo/flathub.flatpakrepo",
                "`--if-not-exists` 保证重复执行无副作用。只在上一步确认缺少 flathub 时需要。"),
            Step.Read(
                "搜索应用，拿到它的 AppID",
                "flatpak search <关键词>",
                "例如 `flatpak search vlc` 得到 `org.videolan.VLC`。后续命令都用 AppID。"),
            Step.Change(
                "安装应用",
                "flatpak install flathub <AppID>",
                "安装前会列出将要下载的运行时与大小，确认后输入 y。"),
            Step.Change(
                "更新全部应用与运行时",
                "flatpak update",
                "Discover 的「更新」做的就是这件事。"),
            Step.Read(
                "查看某个应用拥有哪些沙箱权限",
                "flatpak info --show-permissions <AppID>",
                "重点看 `[Context]` 下的 `filesystems=`、`devices=`、`shared=network`。"),
            Step.Change(
                "只为某个应用放行 SD 卡目录",
                "flatpak override --user --filesystem=/run/media/deck <AppID>",
                "改的是 `~/.local/share/flatpak/overrides/<AppID>`，重启该应用后生效。"),
            Step.Change(
                "撤销该应用的所有权限覆盖",
                "flatpak override --user --reset <AppID>",
                "恢复为应用自带的默认权限。"),
            Step.Change(
                "回收没有应用在用的运行时",
                "flatpak uninstall --unused",
                "卸载过应用之后执行，常能释放 1~5 GB。"),
            Step.Read(
                "找出「应用已卸载、数据还留着」的孤儿目录",
                "comm -13 <(flatpak list --app --columns=application | sort) <(ls ~/.var/app | sort)",
                "`comm -13` 输出只在右侧出现的行：`~/.var/app` 里有、但已不再安装的 AppID。"),
            Step.Danger(
                "卸载应用并同时删除它的数据",
                "flatpak uninstall --delete-data <AppID>",
                "等同于卸载后再删除 `~/.var/app/<AppID>`。只卸载、想保留数据时去掉 `--delete-data`。"),
            Step.Danger(
                "删除一个孤儿数据目录",
                "rm -rf ~/.var/app/<AppID>",
                "只对上一步「孤儿目录」列表里的 AppID 执行，逐个确认，不要用通配符一次删完。"),
        ],

        Verify =
        [
            "1. 安装验证：`flatpak list --app` 中出现该 AppID，KDE 应用菜单里也会出现它的图标。",
            "2. 权限验证：`flatpak override --user --show <AppID>` 显示你添加的覆盖项；在应用内打开文件对话框，能直接浏览 `/run/media/deck`。",
            "3. 清理验证：再执行一次 `flatpak uninstall --unused`，提示没有可卸载的内容；孤儿目录命令输出为空。",
        ],
    };
}
