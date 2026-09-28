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
        Overview = "三种「一次打包、到处运行」的格式各有取舍。了解它们的实现方式，就能理解其他发行版为什么推荐它们、Ubuntu 上为什么到处是 snap，以及本应用在 SteamOS 上为什么只用 AppImage 与便携包。",

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
            "• 本应用在 SteamOS 上的做法：不使用 Flatpak（原因与清理方法见「不用 Flatpak：清理与替代」）；图形应用用 AppImage 或官方便携包放进 `~/Applications`，其余软件用 Distrobox 容器。",
            "• 在 Ubuntu 上：部分软件（如 Firefox）官方只提供 snap，`apt install` 装到的其实是一个安装 snap 的过渡包。",
            "• 任何格式都不能替你判断软件来源是否可信：Flathub 上有「已验证」标记，AppImage 只能自己确认下载来源。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统中的 loop 设备",
                "lsblk -o NAME,TYPE,SIZE,MOUNTPOINTS | grep -E '^NAME|loop'",
                "`grep -E '^NAME|loop'` 保留表头和 loop 行。").Then("输出表头 `NAME TYPE SIZE MOUNTPOINTS`；SteamOS 上通常只有这一行，Ubuntu 上会列出许多 `loopN` 及其挂载点 `/snap/...`。"),
            Step.Read(
                "在 Ubuntu 上列出已安装的 snap",
                "snap list",
                "`Publisher` 列带 ✓ 的是经过验证的发布者。").On(Hosts.Debian).Then("表格：名称、版本、修订号、跟踪通道、发布者、备注。"),
            Step.Change(
                "查看 AppImage 内部的文件结构",
                "./<程序名>.AppImage --appimage-extract && ls squashfs-root",
                "解出到当前目录的 `squashfs-root/`。").Then("先逐行打印解出的文件，最后 `ls` 显示 `AppRun`、`.desktop` 文件、图标以及 `usr/` 目录。"),
            Step.Read(
                "查看 AppImage 自带了哪些运行库",
                "ls squashfs-root/usr/lib | head -n 30",
                "在上一步解包后执行。这就是 AppImage 不依赖系统库、但体积较大的原因。").Then("列出一批 `.so` 文件，如 `libQt6Core.so.6`、`libgtk-3.so.0`。"),
        ],

        Verify =
        [
            "1. 隔离程度：Flatpak（命名空间沙箱 + Portal）≈ Snap（严格模式 + AppArmor）> AppImage（无沙箱）。",
            "2. 可移植性：AppImage（单文件拷走即用）> Flatpak（需要 flatpak 与仓库）> Snap（需要 snapd 与 Snap Store）。",
            "3. 在使用它们的发行版上，`flatpak info --show-permissions <AppID>` 与 `snap connections <snap名>` 分别查看两者的权限。",
        ],
    };
}
