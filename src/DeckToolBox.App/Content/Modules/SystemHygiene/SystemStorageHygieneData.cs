using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SystemHygiene;

public static class SystemStorageHygieneData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "storage-cleanup",
        Title = "缓存、日志与存储清理",
        Keywords = "journal · shadercache · 回收站",
        Level = ModuleLevel.Basic,
        Overview = "深入排查 Linux 桌面系统四大隐蔽空间黑洞：Systemd 滚动日志膨胀、桌面高分辨率缩略图累积、Steam 卸载游戏遗留的着色器缓存 (Shader Cache) 与崩溃转储 (Coredump)。",

        Why =
        [
            "1. Systemd Journal 二进制日志的无节制扩张：",
            "• 现代 Linux 系统的日志由 systemd-journald 统一以二进制结构化文件记录在 `/var/log/journal/` 中。若未对 `SystemMaxUse` 进行硬性封顶，默认配置下日志最多可吃掉磁盘总容量的 10% 或 4GB 空间！很多长期运行未重启的设备日志体积极大。",
            "2. 桌面缩略图缓存 (Freedesktop Thumbnails) 永不失效之谜：",
            "• 根据 FreeDesktop 规范，Dolphin 或其他文件管理器在浏览照片、视频甚至游戏海报时，会在 `~/.cache/thumbnails/` 生成三种规格的缩略图 (normal 128x128, large 256x256, x-large 512x512)。",
            "• **关键痛点**：Linux 桌面规范完全没有设计缩略图的自动垃圾回收机制！哪怕原始文件早已被删除，缩略图缓存依然会永久躺在磁盘上，日积月累常常包含数万个小碎片文件，吞噬数 GB 空间并拖慢文件系统 Inode 遍历速度。",
            "3. Steam 孤儿着色器缓存 (Shader Cache) 与兼容前缀 (Compatdata)：",
            "• Steam 为了避免 Windows 游戏在转译运行时的 DirectX->Vulkan 着色器编译卡顿，会为每个游戏预下载庞大的着色器缓存（位于 `~/.steam/steam/steamapps/shadercache/<AppID>/`）。",
            "• 很多时候用户在 Steam 库中卸载了游戏，但 Steam 并不会自动同步删除对应的 Shader 目录以及 `compatdata/<AppID>` 中的虚拟 C 盘环境，造成数十 GB 的隐形磁盘沉淀！",
            "4. 进程崩溃转储 (Coredump) 物理内存镜像：",
            "• 当复杂游戏或桌面进程发生段错误 (SIGSEGV) 异常崩溃时，内核 systemd-coredump 会把该进程死前的物理内存完整转储到 `/var/lib/systemd/coredump/`，单个文件动辄几百 MB 到数 GB。",
        ],

        Consequences =
        [
            "• 日志瘦身安全边界：使用 `journalctl --vacuum-size` 只会裁撤最古老的历史日志条目，保留最新的实时诊断日志，绝不会影响系统的任何功能。",
            "• 缩略图清空影响：清空 `~/.cache/thumbnails/` 完全零风险。下次打开文件管理器时，系统仅在需要时重新动态生成当前文件的缩略图。",
            "• 着色器清理影响：清理已卸载游戏的 AppID 目录可立即回收大量物理空间；若误删了未卸载游戏的着色器，Steam 在下次启动游戏时会自动重新下载或即时编译，无破坏性后果。",
        ],

        Steps =
        [
            Step.Read(
                "审计系统当前 systemd 二进制日志所占用的物理磁盘大小",
                "journalctl --disk-usage",
                "查看已归档和活动 journal 文件的磁盘占用总量。"),
            Step.Change(
                "安全裁撤历史系统日志，硬性限制日志总量至 100MB",
                "journalctl --vacuum-size=100M",
                "自动从最旧的日志块开始安全删除，直至总占用压缩在 100MB 安全基线以内。"),
            Step.Danger(
                "检查并彻底清空桌面累积的高分辨率缩略图碎片缓存",
                "du -sh ~/.cache/thumbnails/ 2>/dev/null; rm -rf ~/.cache/thumbnails/*",
                "先打印当前缩略图占用大小，随后一键安全清空无用的历史缩略图缓存。"),
            Step.Read(
                "排查系统崩溃转储 (Coredump) 堆积情况与清理",
                "ls -lh /var/lib/systemd/coredump/ 2>/dev/null || echo \"无系统崩溃转储文件\"",
                "查看是否有因游戏闪退残留的未释放巨大核心转储文件。"),
            Step.Read(
                "审计 Steam 着色器缓存与各游戏转译 C 盘前缀大小排行榜",
                "du -h --max-depth=1 ~/.steam/steam/steamapps/shadercache/ 2>/dev/null | sort -hr | head -n 15",
                "按占用空间由大到小排序，快速定位哪些游戏 AppID 的着色器占用了数 GB 闪存。"),
            Step.Danger(
                "清空回收站中被遗忘删除的全部废弃文件",
                "rm -rf ~/.local/share/Trash/*",
                "彻底物理擦除桌面回收站中的幽灵文件，真正将存储空间归还给文件系统。"),
        ],

        Verify =
        [
            "1. 日志体积复核：再次执行 `journalctl --disk-usage`，确认输出显示已在 100MB 以内。",
            "2. 用户主目录缓存核验：执行 `du -sh ~/.cache/` 查看用户缓存总量，通常由原本的数 GB 骤降至几百 MB，空间释放效果显著。",
        ],
    };
}
