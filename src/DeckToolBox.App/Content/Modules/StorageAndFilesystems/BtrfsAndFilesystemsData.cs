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
        Overview = "深入对比 Ext4 传统日志文件系统与 Btrfs 现代写时复制架构，掌握 Inode 分配机制、秒级快照、透明压缩与磁盘空间真实审计。",

        Why =
        [
            "1. Ext4 (经典日志型文件系统的基石)：",
            "• Inode 与 Data Block 分离：文件内容存放在 Data Block 块中，文件的元数据（权限、大小、修改时间、块指针）独立存放在 Inode 结构中。文件名则存放在父目录的数据项 (dentry) 中！",
            "• 罕见但致命的“假性写满”故障（Inode 耗尽）：如果系统生成了数百万个几字节的琐碎临时小文件，会把有限的 Inode 编号全部分配光。此时即便磁盘还有 100GB 空闲物理空间，也会直接报错 No space left on device！",
            "2. Btrfs (写时复制与掌机闪存的革命性设计)：",
            "• 写时复制 (Copy-on-Write, CoW)：修改文件时从不就地覆盖原有数据块，而是将新数据写入全新空闲块，最后原子化更新 B-tree 根节点指针。即使突然拔电池关机，数据也绝不损坏！",
            "• 秒级无损快照 (Snapshots)：创建快照只需拷贝元数据指针，用时不到 0.001 秒，且快照刚建立时占用 0 字节额外磁盘！",
            "• ZSTD 透明压缩：数据进入内核写盘前由 CPU 实时以 Zstandard 算法压缩，对于 MicroSD 卡和 NVMe 固态硬盘，不仅节约 25%~35% 存储空间，更因物理写入字节变少而大幅提升读取带宽！",
        ],

        Consequences =
        [
            "• 掌握双重审计：排查磁盘不能只看 df -h（物理容量），一旦遇到诡异无法写入报错，必须执行 df -i（Inode 使用率）进行双重排查。",
            "• CoW 与虚拟机的冲突：Btrfs 的 CoW 特性对经常发生碎片随机写入的大型文件（如 Windows 虚拟机磁盘镜像、大型 SQLite 数据库）会产生严重碎片化，通常需要使用 chattr +C 手动关闭特定文件夹的 CoW。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统物理磁盘与各分区的真实文件系统格式 (FSTYPE) 与 UUID",
                "lsblk -f",
                "清晰看到每一个分区底层是 ext4、btrfs、vfat 还是 f2fs。"),
            Step.Read(
                "排查全局 Inode 索引节点消耗情况 (防范假性写满)",
                "df -i",
                "检查 IUse% 这一列，确保没有因海量小碎片导致 Inode 耗尽。"),
            Step.Read(
                "查看当前系统各分区的物理磁盘真实剩余容量",
                "df -h -x tmpfs -x devtmpfs",
                "过滤掉纯内存的临时文件系统，直接观察物理 NVMe 与 SD 卡剩余空间。"),
        ],

        Verify =
        [
            "执行 lsblk -f，你会看到 SteamOS 主系统根分区通常为 ext4/btrfs，而 EFI 分区为 vfat，从而精准掌握各挂载点的底层存储技术。",
        ],
    };
}
