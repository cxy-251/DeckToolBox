using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class ArchPacmanAurData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "arch-pacman",
        Title = "Arch Linux：pacman",
        Keywords = "pacman · AUR · .pacnew",
        Level = ModuleLevel.Intermediate,
        Overview = "深入解析极客最爱的 Arch Linux 滚动更新生态，剖析 .pkg.tar.zst 极速格式、pacman 的 KISS 极简哲学、.pacnew 配置合并机制、PKGBUILD 规范与 AUR 社区构建。",

        Why =
        [
            "1. 为什么 Arch 的包解包速度远超 Debian 和 Red Hat？(`.pkg.tar.zst`)：",
            "• 一个 Arch 软件包在本质上就是一个由 Facebook (Meta) 研发的 **Zstandard (.zst)** 强力压缩的 tar 归档！",
            "• 它没有任何复杂的专有元数据数据库层，解压时几乎能榨干 NVMe 固态硬盘的物理连续读取速度，安装耗时常常比传统包管理快数倍以上。",
            "2. pacman 的核心哲学：KISS (保持简单透明) 与拒绝暗箱交互：",
            "• pacman 绝不会在安装软件时弹出花哨的文本菜单去强迫你做交互式配置。如果上游软件的新版本配置文件与你本地 `/etc/` 下的旧配置发生冲突，pacman 绝不暴力覆盖，而是会将新配置以 `.pacnew` 为后缀写入磁盘（例如 `/etc/pacman.conf.pacnew`），将合并与决定的绝对权力完全交还给用户！",
            "3. 滚动更新黄金铁律：严禁“部分更新 (Partial Upgrades)”：",
            "• 在 Arch / SteamOS 体系中，软件包以最新 ABI 接口联动滚动。如果执行 `pacman -Sy <包名>` 单独更新某一个软件而不同步更新全系统底层库，新软件可能会引用新版 `glibc.so`，而其他未更新的程序会因找不到旧版符号而瞬间全面雪崩！**必须永远执行全量同步更新 (`pacman -Syu`)**。",
            "4. Linux 世界最繁荣的自留地：AUR (Arch User Repository)：",
            "• 任何用户无需官方收录即可分享打包脚本。其核心是一份声明式的 **`PKGBUILD`** 纯 Bash 脚本：",
            "  - `source=()` 声明源码下载地址；",
            "  - `build()` 函数定义编译步骤 (`cmake` / `make`)；",
            "  - `package()` 函数以普通非特权用户将编译产物安装进虚拟目录 `$pkgdir`，最终由 `makepkg` 打包为 `.pkg.tar.zst` 并安全交付给 pacman 安装。",
            "• AUR 助手 (`paru` / `yay`) 则是在网络层自动帮用户拉取 git 仓库、展示 PKGBUILD 供安全审查并自动化触发构建流程。",
        ],

        Consequences =
        [
            "• 严禁不看 PKGBUILD 直接通过 AUR 助手闭眼安装：AUR 任何人都能提交脚本，安装前必须快速扫视其 `source` 下载链接是否来自官方上游，检查是否有可疑的 curl/bash 恶意后门。",
            "• .pacnew 配置遗留隐患：长期不合并 .pacnew 文件可能导致某些基础服务在升级后因配置语法过时而启动失败。",
        ],

        Steps =
        [
            Step.Read(
                "列出系统当前所有由用户“显式主动安装 (Explicit)”的软件清单",
                "pacman -Qe",
                "仅查看用户显式主动装过的应用包，排除作为依赖被连带装入的底层库。").On(Hosts.Arch),
            Step.Root(
                "排查并清理系统中失去所有依赖引用的孤儿包 (Orphan Packages)",
                "pacman -Qtdq ; sudo pacman -Rns $(pacman -Qtdq)",
                "查询并级联彻底清理失去所有引用的孤儿包，保持系统绝对纯粹与极简。").On(Hosts.Arch),
            Step.Read(
                "反向查询硬盘上任意可执行文件是由哪一个官方包提供 (pacman -Qo)",
                "pacman -Qo /usr/bin/bash",
                "秒级反向定位提供该命令的 Arch 官方或 AUR 软件包名。").On(Hosts.Arch),
            Step.Read(
                "扫描全盘所有尚未被管理员合并的 .pacnew 配置文件遗留",
                "find /etc -name \"*.pacnew\" 2>/dev/null",
                "扫描尚未被系统管理员合并的最新上游配置文件，预防配置语法失谐导致的服务启动失败。").On(Hosts.Arch),
            Step.Read(
                "学习声明式 PKGBUILD 标准骨架与本地安全构建流程 (makepkg)",
                "echo '规范范式: 编写 PKGBUILD -> 执行 makepkg -si'",
                "标准 PKGBUILD 规范：使用普通非特权用户沙箱构建，最终交由 pacman 统一安全写入系统。"),
        ],

        Verify =
        [
            "1. 包数据库核验：执行 `pacman -Qk <包名>`，若返回 `0 missing files`，代表该软件包在系统中的所有文件节点均完整健在，无丢失或损坏。",
            "2. 缓存清理：Arch 默认保留所有下载过的历史安装包以供紧急降级。可使用 `sudo paccache -r` 安全保留最近 3 个历史版本，清空更旧的包，释放数十 GB 空间。",
        ],
    };
}
