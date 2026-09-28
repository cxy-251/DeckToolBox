using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.StorageAndFilesystems;

public static class BtrfsAndFilesystemsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "filesystems",
        Title = "ext4 与 Btrfs 文件系统",
        Keywords = "ext4 · btrfs · inode",
        Level = ModuleLevel.Advanced,
        Overview = "SteamOS 同时使用 btrfs（系统分区）和 ext4（home 分区与 SD 卡）。两者的设计取舍不同：ext4 简单稳定，btrfs 提供快照、压缩与写时复制。了解它们，才能看懂磁盘空间、inode 与快照相关的现象。",

        Why =
        [
            "1. ext4：",
            "• 每个文件有一个 inode，保存权限、大小、时间戳和数据块的位置；文件名保存在所在目录的数据中。",
            "• inode 的总数在格式化时就已确定。存放大量小文件时，可能出现「磁盘还有空间，却提示 No space left on device」，这时是 inode 用完了，`df -i` 可以看到。",
            "• 采用日志 (journal) 机制：意外断电后通过回放日志快速恢复元数据的一致性。",
            "2. btrfs：",
            "• 写时复制 (Copy-on-Write)：修改数据时不覆盖原来的块，而是写到新位置，最后原子地更新元数据指针。突然断电时，文件系统要么是旧状态要么是新状态，不会出现结构损坏（尚未写入的数据仍会丢失）。",
            "• 快照 (Snapshot)：创建快照只需复制元数据，几乎瞬间完成，初始不额外占用空间；之后随着原数据被修改，快照保留的旧数据块才开始占用空间。",
            "• 透明压缩：可选用 zstd 等算法在写入时压缩，读取时自动解压。对文本、日志等可压缩数据能显著节省空间，对已压缩的视频、游戏资源包几乎无效。",
            "• inode 按需分配，不会出现 inode 耗尽。",
        ],

        Consequences =
        [
            "• 本模块的命令只读。",
            "• 排查「写不进文件」时，同时检查 `df -h`（空间）和 `df -i`（inode）。",
            "• btrfs 的写时复制对频繁随机改写的大文件（虚拟机磁盘镜像、大型数据库）不友好，容易产生碎片；这类目录可以用 `chattr +C` 关闭写时复制（只对之后新建的文件生效）。",
        ],

        Steps =
        [
            Step.Read(
                "查看各分区的文件系统类型与 UUID",
                "lsblk -f",
                "UUID 是分区的唯一标识，常用于 `/etc/fstab`。").Then("树状表格：每个分区的 `FSTYPE`（btrfs、ext4、vfat、swap）、卷标、UUID、可用空间和挂载点。"),
            Step.Read(
                "查看 inode 使用情况",
                "df -i",
                "inode 耗尽时即使还有空间也无法新建文件。").Then("类似 `df -h` 的表格，但各列是 inode 数量；`IUse%` 为 inode 使用率。btrfs 分区显示为 0，因为它按需分配 inode。"),
            Step.Read(
                "查看真实磁盘分区的剩余空间",
                "df -h -x tmpfs -x devtmpfs",
                "`-x` 排除指定类型的文件系统。").Then("表格列出真实磁盘分区的总量、已用、可用和使用率，不包含内存盘。"),
        ],

        Verify =
        [
            "1. `lsblk -f` 中 `rootfs-A/B` 为 btrfs，`home` 为 ext4，`esp`/`efi` 为 vfat。",
            "2. `df -i /home` 的 `IUse%` 远低于 100%。",
        ],
    };
}
