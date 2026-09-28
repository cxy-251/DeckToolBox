using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SystemHygiene;

public static class SystemStorageHygieneData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "storage-cleanup",
        Title = "缓存、日志与存储清理",
        Keywords = "journal · shadercache · compatdata · 回收站",
        Level = ModuleLevel.Basic,
        Overview = "SteamOS 上最常见的「空间去哪了」：着色器缓存、已卸载游戏残留的 Proton 前缀、系统日志、缩略图缓存和回收站。先测量，再有针对性地清理。",

        Why =
        [
            "1. Steam 着色器缓存 (`steamapps/shadercache/<AppID>`)：",
            "• 为减少游戏卡顿，Steam 会为每个游戏下载或生成预编译着色器，单个游戏可达数 GB。",
            "• 卸载游戏时 Steam 通常会一并删除，但并不总是如此；删除仍在玩的游戏的缓存也无害，下次启动会重新生成（首次进游戏可能短暂卡顿）。",
            "2. Proton 前缀 (`steamapps/compatdata/<AppID>`)：",
            "• 每个 Windows 游戏一个虚拟 C 盘。卸载游戏后有时会残留下来。",
            "• **前缀里常常保存着游戏存档**（`AppData`、`Documents` 下），不支持 Steam 云存档的游戏删掉前缀就等于删档。",
            "3. systemd 日志 (`/var/log/journal`)：默认上限约为所在文件系统的 10%，最多 4 GB。SteamOS 上 `/var/log` 实际位于 home 分区。",
            "4. 缩略图缓存 (`~/.cache/thumbnails`)：文件管理器生成，原文件删除后缩略图不会自动清除；全部删除是安全的，需要时会重新生成。",
            "5. 回收站 (`~/.local/share/Trash`)：Dolphin 中「删除」只是移入回收站，空间要清空回收站才会释放。",
        ],

        Consequences =
        [
            "• `du`、`journalctl --disk-usage` 等测量命令完全只读，先测量再决定删什么。",
            "• 清理着色器缓存和缩略图可逆：会自动重新生成。",
            "• `journalctl --vacuum-*` 只删除最旧的归档日志，不影响系统运行，但被删的日志无法再用于排错。",
            "• **删除 compatdata 前缀和清空回收站不可恢复**。删除前缀前先确认游戏支持云存档，或先备份前缀中的存档目录。",
        ],

        Steps =
        [
            Step.Read(
                "查看各分区剩余空间",
                "df -h / /home /run/media/deck/* 2>/dev/null",
                "内置存储的空间看 `/home` 这一行。"),
            Step.Read(
                "找出家目录下最占空间的目录",
                "du -h --max-depth=2 ~ 2>/dev/null | sort -h | tail -n 20",
                "数据量大时要扫描十几秒到几分钟。交互式浏览可以用系统自带的 `ncdu ~`（方向键进入目录，`q` 退出）。"),
            Step.Read(
                "着色器缓存按大小排行",
                "du -sh ~/.local/share/Steam/steamapps/shadercache/* 2>/dev/null | sort -h | tail -n 15",
                "目录名是 AppID，可在 `store.steampowered.com/app/<AppID>` 查到对应游戏。"),
            Step.Read(
                "列出内置存储上「游戏已卸载但前缀还在」的 compatdata",
                "cd ~/.local/share/Steam/steamapps && for d in compatdata/*/; do id=$(basename \"$d\"); [ -f \"appmanifest_$id.acf\" ] || du -sh \"$d\"; done",
                "只检查内置库。装在 SD 卡上的游戏清单位于 SD 卡的 `steamapps` 目录，会被误报；非 Steam 游戏（很大的 AppID）也没有清单文件。"),
            Step.Change(
                "删除某个游戏的着色器缓存",
                "rm -rf ~/.local/share/Steam/steamapps/shadercache/<AppID>",
                "可再生数据。下次启动该游戏时重新下载或编译。"),
            Step.Danger(
                "删除某个残留的 Proton 前缀",
                "rm -rf ~/.local/share/Steam/steamapps/compatdata/<AppID>",
                "**会删除前缀中的本地存档**。只对上一步确认已卸载、且不需要存档的 AppID 执行。"),
            Step.Read(
                "查看系统日志占用",
                "journalctl --disk-usage",
                "普通用户看到的可能只是自己的日志，加 `sudo` 查看全部。"),
            Step.Root(
                "把系统日志压缩到 200 MB 以内",
                "sudo journalctl --vacuum-size=200M",
                "从最旧的日志开始删除，直到总量低于该值。"),
            Step.Change(
                "清空缩略图缓存",
                "rm -rf ~/.cache/thumbnails/*",
                "可再生数据，完全安全。"),
            Step.Danger(
                "清空回收站",
                "rm -rf ~/.local/share/Trash/files/* ~/.local/share/Trash/info/*",
                "等同于在 Dolphin 中「清空回收站」，删除后无法找回。"),
        ],

        Verify =
        [
            "1. 再次执行第一步的 `df -h`，对比 `/home` 的可用空间变化。",
            "2. `journalctl --disk-usage` 显示的总量低于设定值。",
            "3. 清理着色器缓存后首次进入对应游戏，Steam 会显示「正在处理 Vulkan 着色器」，属正常现象。",
        ],
    };
}
