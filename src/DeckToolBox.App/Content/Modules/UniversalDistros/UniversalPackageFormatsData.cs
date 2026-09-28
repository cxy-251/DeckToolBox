using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class UniversalPackageFormatsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "universal-formats",
        Title = "Flatpak、Snap 与 AppImage 对比",
        Keywords = "flatpak · snap · appimage · 选型",
        Level = ModuleLevel.Basic,
        Overview = "三种「一次打包、到处运行」的格式各有取舍。了解它们的实现方式，就知道在 SteamOS 上该选哪种、在 Ubuntu 上为什么到处是 snap。",

        Why =
        [
            "1. 为什么需要跨发行版格式：各发行版的库版本不同，同一个程序要为每个发行版单独打包。通用格式把依赖一起带上，只打包一次。",
            "2. **Flatpak**（Flathub 为主要仓库）：",
            "- 技术：OSTree 存储（相同文件去重）+ bubblewrap 沙箱 + XDG Desktop Portal 权限门户。",
            "- 定位：图形桌面应用。多个应用共享运行时。",
            "- 取舍：有沙箱、可去重；首次安装运行时较大，不适合命令行工具和系统服务。",
            "3. **Snap**（Canonical，Ubuntu 默认）：",
            "- 技术：每个包是一个 SquashFS 镜像，通过 loop 设备挂载；用 AppArmor 做隔离。",
            "- 定位：桌面应用、命令行工具、服务都支持，默认自动更新。",
            "- 取舍：只有 Canonical 的 Snap Store 一个商店；每个 snap 占用一个 loop 设备；依赖 snapd 守护进程和 AppArmor，在 SteamOS 上不可用。",
            "4. **AppImage**：",
            "- 技术：一个自带运行库的 SquashFS 单文件，运行时经 FUSE 挂载。",
            "- 定位：免安装的绿色软件，下载、`chmod +x`、运行。",
            "- 取舍：没有沙箱、没有统一的更新机制，每个文件自带全部依赖。",
        ],

        Consequences =
        [
            "• 在 SteamOS 上的建议：图形应用优先 Flatpak（系统更新不影响、有沙箱）；只有 AppImage 版本的软件放进 `~/Applications`；命令行开发工具用 Distrobox 容器。",
            "• 在 Ubuntu 上：部分软件（如 Firefox）官方只提供 snap，`apt install` 装到的其实是一个安装 snap 的过渡包。",
            "• 任何格式都不能替你判断软件来源是否可信：Flathub 上有「已验证」标记，AppImage 只能自己确认下载来源。",
        ],

        Steps =
        [
            Step.Read(
                "查看已安装的 Flatpak 应用及其运行时",
                "flatpak list --columns=application,runtime,size",
                "`runtime` 列显示每个应用依赖哪个运行时。"),
            Step.Read(
                "查看系统中的 loop 设备",
                "lsblk -o NAME,TYPE,SIZE,MOUNTPOINTS | grep -E '^NAME|loop'",
                "在 Ubuntu 上，每个 snap 都是一个 loop 设备；在 SteamOS 上通常没有。"),
            Step.Read(
                "在 Ubuntu 上列出已安装的 snap",
                "snap list",
                "`Publisher` 列带 ✓ 的是经过验证的发布者。").On(Hosts.Debian),
            Step.Change(
                "查看 AppImage 内部的文件结构",
                "./<程序名>.AppImage --appimage-extract && ls squashfs-root",
                "解出到当前目录的 `squashfs-root/`，可以看到 `AppRun` 和自带的 `usr/lib`。"),
            Step.Change(
                "让 Flatpak 应用访问 ~/Games 目录",
                "flatpak override --user --filesystem=~/Games <AppID>",
                "只放行需要的目录；撤销用 `flatpak override --user --reset <AppID>`。"),
        ],

        Verify =
        [
            "1. 隔离程度：Flatpak（命名空间沙箱 + Portal）≈ Snap（严格模式 + AppArmor）> AppImage（无沙箱）。",
            "2. 可移植性：AppImage（单文件拷走即用）> Flatpak（需要 flatpak 与仓库）> Snap（需要 snapd 与 Snap Store）。",
            "3. `flatpak info --show-permissions <AppID>` 与 `snap connections <snap名>` 分别查看两者的权限。",
        ],
    };
}
