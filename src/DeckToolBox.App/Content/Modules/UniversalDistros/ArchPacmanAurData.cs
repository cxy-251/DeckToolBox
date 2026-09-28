using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class ArchPacmanAurData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "arch-pacman",
        Title = "Arch Linux：pacman",
        Keywords = "pacman · makepkg · AUR · .pacnew",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 3 就是基于 Arch Linux 构建的，系统里的软件都由 pacman 管理。查询类命令可以直接在 Deck 上运行；安装类操作请在 Arch 容器里练习。",

        Why =
        [
            "1. 包格式：`.pkg.tar.zst` 就是 zstd 压缩的 tar 包，外加 `.PKGINFO`（元数据）、`.MTREE`（文件校验信息）等几个小文件，解压快、结构简单。",
            "2. 滚动更新与「禁止部分升级」：",
            "• Arch 没有大版本号，软件持续更新到最新版。整个仓库只保证「所有包一起更新到最新」时互相兼容。",
            "• 所以不要只刷新数据库再装单个包（`pacman -Sy <包>`）：新包可能依赖尚未升级的库，导致其他程序损坏。正确做法是 `pacman -Syu` 升级全部后再安装。",
            "3. `.pacnew`：升级时若软件的新默认配置与你修改过的 `/etc` 配置冲突，pacman 不覆盖你的文件，而是把新版本存为 `<文件>.pacnew`，由你手动合并。",
            "4. AUR (Arch User Repository)：用户提交的构建脚本仓库，不是二进制仓库：",
            "• 每个 AUR 包是一份 `PKGBUILD`（bash 脚本），声明源码地址 `source=()`、编译步骤 `build()` 和安装步骤 `package()`。",
            "• `makepkg` 以普通用户执行脚本并打包，最后交给 `pacman -U` 安装。`yay`、`paru` 等 AUR 助手把这些步骤自动化。",
            "5. 在 SteamOS 上：可以用 pacman 查询系统里的任何文件、任何包，但不要在主机上用它安装（只读系统，且更新会覆盖）。",
        ],

        Consequences =
        [
            "• `pacman -Q*` 查询命令完全只读，在 SteamOS 主机上执行也安全。",
            "• **AUR 内容无人审核**：安装前务必阅读 PKGBUILD，确认 `source` 指向官方上游、没有可疑的下载执行命令。",
            "• `pacman -Rns` 会同时删除依赖和配置文件，执行前看清列表。",
            "• 长期不处理 `.pacnew` 可能导致服务在升级后因配置格式过旧而启动失败。",
        ],

        Steps =
        [
            Step.Read(
                "查询某个命令属于哪个包（可在 Deck 上直接运行）",
                "pacman -Qo $(command -v 7z)",
                "`-Q` 查询本地数据库，`-o` 按文件查所属包；`$(command -v 7z)` 先找到 7z 的完整路径。").Then("输出一行，如 `/usr/bin/7z 由 7zip 25.01-1.1 所拥有`。"),
            Step.Read(
                "列出显式安装、且不被其他包依赖的包（可在 Deck 上运行）",
                "pacman -Qet",
                "`-e` 显式安装，`-t` 没有被依赖。").Then("每行「包名 版本」，在 SteamOS 上约有一两百个，大多是 Valve 在系统镜像中直接指定安装的软件。"),
            Step.Read(
                "校验某个包的文件是否完整（可在 Deck 上运行）",
                "pacman -Qk coreutils",
                "在 SteamOS 上，文档目录被系统镜像删除以节省空间（所以也没有 man 手册），这些警告属正常现象。").Then("先可能有几行 `警告：coreutils: /usr/share/info/ (没有那个文件或目录)`，最后一行汇总 `coreutils: xxx 总文件，N 缺少的文件`。"),
            Step.Change(
                "在 Deck 上创建一个 Arch 练习容器并进入",
                "distrobox create --name arch -i docker.io/library/archlinux:latest && distrobox enter arch",
                "如已在「Distrobox 用户容器」模块创建过，直接 `distrobox enter arch`。").Then("下载 Arch 镜像并初始化，完成后提示符变为 `(deck@arch ~)$`。"),
            Step.Root(
                "升级全部并安装软件包",
                "sudo pacman -Syu <包名>",
                "先同步数据库、升级全部，再安装指定包，避免部分升级。").On(Hosts.Arch).Then("先同步各仓库数据库，列出要升级和新装的包及总大小，询问 `:: 进行安装吗？ [Y/n]`，回车后下载、校验签名并安装。"),
            Step.Read(
                "列出孤儿包（作为依赖安装、现在没人需要）",
                "pacman -Qdtq",
                "`-d` 作为依赖安装，`-t` 没有被依赖，`-q` 只输出包名。").On(Hosts.Arch).Then("每行一个包名；没有输出表示没有孤儿包。"),
            Step.Root(
                "删除孤儿包",
                "sudo pacman -Rns $(pacman -Qdtq)",
                "只在上一步有输出时执行；`-n` 同时删除备份配置文件，`-s` 连带删除只被它们依赖的包。").On(Hosts.Arch).Then("列出将被删除的包并询问确认，确认后删除它们及其配置文件。若没有孤儿包，会报错「没有指定目标」，属正常。"),
            Step.Read(
                "查找待合并的 .pacnew 文件",
                "find /etc -name '*.pacnew' 2>/dev/null",
                "对比后合并：`diff /etc/<文件> /etc/<文件>.pacnew`。").On(Hosts.Arch).Then("每行一个文件路径，如 `/etc/pacman.conf.pacnew`；没有输出表示没有待合并的配置。"),
            Step.Root(
                "从 AUR 手动构建一个包（示例：yay）",
                "sudo pacman -S --needed git base-devel && git clone https://aur.archlinux.org/yay-bin.git && cd yay-bin && less PKGBUILD && makepkg -si",
                "`less PKGBUILD` 是在安装前审阅脚本。").On(Hosts.Arch).Then("依次：安装构建工具 → 克隆仓库 → 用 `less` 打开 PKGBUILD 供你审阅（按 `q` 退出） → makepkg 下载、打包，最后询问 sudo 并安装。完成后容器里可以使用 `yay`。"),
        ],

        Verify =
        [
            "1. `pacman -Q <包名>` 输出版本号，说明已安装。",
            "2. `pacman -Qk <包名>` 显示没有缺失文件。",
            "3. 在 SteamOS 主机上执行 `pacman -Q | wc -l`，可以看到整个系统由上千个 Arch 格式的包组成。",
        ],
    };
}
