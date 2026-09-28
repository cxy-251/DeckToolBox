using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class PackageManagerRosettaData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "package-rosetta",
        Title = "包管理命令对照表",
        Keywords = "apt · dnf · pacman · zypper · apk",
        Level = ModuleLevel.Basic,
        Overview = "换到一个陌生的发行版，第一件事是弄清它用哪个包管理器，以及「搜索、安装、升级、删除、查归属」这几个动作分别怎么写。下面按 Debian/Ubuntu → Fedora → Arch → openSUSE → Alpine 的顺序对照。",

        Why =
        [
            "1. 刷新软件源索引：",
            "• `apt update` · `dnf makecache` · `pacman -Sy`（只能与 `-u` 一起用，见下）· `zypper ref` · `apk update`",
            "2. 升级整个系统：",
            "• `apt full-upgrade` · `dnf upgrade` · `pacman -Syu` · `zypper up`（Tumbleweed 用 `zypper dup`）· `apk upgrade`",
            "3. 搜索软件包：",
            "• `apt search` · `dnf search` · `pacman -Ss` · `zypper se` · `apk search`",
            "4. 安装：",
            "• `apt install` · `dnf install` · `pacman -S` · `zypper in` · `apk add`",
            "5. 删除（连同不再需要的依赖）：",
            "• `apt autoremove --purge <包>` · `dnf remove` · `pacman -Rns` · `zypper rm --clean-deps` · `apk del`",
            "6. 查看包信息：",
            "• `apt show` · `dnf info` · `pacman -Si`（已装用 `-Qi`）· `zypper info` · `apk info -a`",
            "7. 列出已安装的包：",
            "• `dpkg -l` · `dnf list --installed` · `pacman -Q` · `zypper se -i` · `apk info`",
            "8. 某个文件属于哪个包：",
            "• `dpkg -S` · `rpm -qf` · `pacman -Qo` · `rpm -qf` · `apk info --who-owns`",
            "9. 包里有哪些文件：",
            "• `dpkg -L` · `rpm -ql` · `pacman -Ql` · `rpm -ql` · `apk info -L`",
            "10. 哪个包能提供某个命令/文件（包括未安装的）：",
            "• `apt-file search`（需先安装 apt-file）· `dnf provides` · `pacman -F`（先 `pacman -Fy`）· `zypper se --provides` · `apk search -e cmd:<命令>`",
            "11. 清理下载缓存：",
            "• `apt clean` · `dnf clean all` · `paccache -r`（pacman-contrib）· `zypper clean` · `apk cache clean`",
        ],

        Consequences =
        [
            "• 以上写入类操作（安装、升级、删除、清理）都需要 root，前面加 `sudo`。",
            "• openSUSE 与 Fedora 同属 RPM 家族，查询已装包时都可以直接用 `rpm`。",
            "• 包名在不同发行版间常有差异，例如开发头文件在 Debian 系叫 `libfoo-dev`，在 Fedora / openSUSE 系叫 `libfoo-devel`，Arch 通常不单独拆分。",
            "• SteamOS 虽然有 pacman，但系统只读且会被更新覆盖，不要用它安装软件（见「只读系统与 A/B 更新」）。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前是哪个发行版",
                "cat /etc/os-release",
                "`ID` 是发行版标识，`ID_LIKE` 是它的上游家族。").Then("输出十几行 `名称=值`：`NAME`、`ID`（SteamOS 为 `steamos`）、`ID_LIKE`（SteamOS 为 `arch`）、`VERSION_ID` 等。"),
            Step.Read(
                "检测系统里有哪些包管理器",
                "for pm in apt dnf pacman zypper apk rpm-ostree nix flatpak; do command -v $pm >/dev/null && echo \"有 $pm\"; done",
                "`command -v` 找到命令时返回成功，`&&` 后的 echo 才会执行。").Then("每找到一个输出一行，SteamOS 上是 `有 pacman`、`有 flatpak`；在 Ubuntu 容器中是 `有 apt`。"),
            Step.Read(
                "在 Deck 上查询某个命令来自哪个包",
                "pacman -Qo $(command -v tar)",
                "对照表第 8 行的 Arch 写法，SteamOS 上直接可用。").Then("输出 `/usr/bin/tar 由 tar 1.35-2 所拥有`。"),
        ],

        Verify =
        [
            "1. 在各发行版的 Distrobox 容器里分别执行第 2 步，只会显示该发行版自己的包管理器。",
            "2. 在 Ubuntu 容器里用 `dpkg -S`、在 Fedora 容器里用 `rpm -qf` 查询 `$(command -v tar)`，都能得到 `tar` 包，印证对照表第 8 行。",
        ],
    };
}
