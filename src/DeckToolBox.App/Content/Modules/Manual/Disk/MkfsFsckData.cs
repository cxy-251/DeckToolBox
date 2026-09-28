using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Disk;

public static class MkfsFsckData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-mkfs-fsck",
        Title = "分区、格式化与检查",
        Keywords = "parted · sfdisk · mkfs.ext4 · mkfs.exfat · root_owner · fsck · wipefs",
        Level = ModuleLevel.Intermediate,
        Overview = "给 U 盘、移动硬盘或 SD 卡重新分区、格式化，以及在意外断电后检查修复文件系统。这些操作都会清除或改写数据，命令本身简单，关键是每一步之前确认设备名。图形界面的「KDE 分区管理器」可以完成同样的工作。",

        Why =
        [
            "1. 流程：确认设备 → 卸载其上所有分区 → （可选）清除旧签名 → 分区 → 格式化 → 挂载使用。",
            "2. 分区工具（SteamOS 自带）：",
            "- `parted`：`sudo parted /dev/sdX print` 查看；`mklabel gpt` 新建分区表；`mkpart 名称 ext4 0% 100%` 建一个占满整盘的分区。",
            "- `sfdisk`、`gdisk`、`fdisk`：同类工具，`sfdisk -l` 列出分区表。",
            "- `partitionmanager`：KDE 分区管理器，图形界面，适合不熟悉命令行时使用。",
            "3. 格式化（mkfs = make filesystem）：",
            "- `mkfs.ext4 -L 卷标 分区`：Linux 原生格式。**加上 `-E root_owner=$(id -u):$(id -g)`，否则新文件系统的根目录属于 root，普通用户无法写入。**",
            "- `mkfs.exfat -L 卷标 分区`：与 Windows、macOS 通用。",
            "- `mkfs.vfat -F 32`：FAT32，兼容性最广，但单个文件不能超过 4 GB。",
            "- `mkfs.btrfs`、`mkfs.ntfs` 同样可用。",
            "4. `wipefs -a 设备`：清除设备上旧的分区表与文件系统签名，避免系统误识别残留格式。",
            "5. fsck（file system check）：检查并修复文件系统错误，**必须在卸载状态下运行**。`fsck.ext4 -f` 强制检查，`-n` 只检查不修复；exFAT 用 `fsck.exfat`。SD 卡意外拔出后无法挂载时，先试这一步。",
        ],

        Consequences =
        [
            "• **分区、wipefs、mkfs 会清除设备上的全部数据**，写错设备名（例如写成 `nvme0n1`）会毁掉系统或家目录。每一步之前都用 `lsblk` 核对设备名和大小。",
            "• 不要对已挂载的分区运行 fsck，也不要对 SteamOS 的系统分区运行它。",
            "• 用于 Steam 游戏库的 SD 卡，最简单的方式是在游戏模式中「设置 → 存储 → 格式化」，由 Steam 自动完成分区与 ext4 格式化。",
        ],

        Steps =
        [
            Step.Read(
                "确认目标设备",
                "lsblk -o NAME,SIZE,MODEL,TRAN,FSTYPE,LABEL,MOUNTPOINTS",
                "找到 U 盘对应的 `sdX`，核对大小与型号。").Then("U 盘通常显示为 `sda`，`TRAN` 为 `usb`，大小与 U 盘标称容量接近。"),
            Step.Root(
                "查看它的分区表",
                "sudo parted /dev/<磁盘名> print",
                "`<磁盘名>` 如 `sda`，是整个磁盘而不是分区。").Then("显示型号、容量、分区表类型（gpt 或 msdos）以及每个分区的起止位置、大小、文件系统。"),
            Step.Danger(
                "清除旧签名并新建一个占满整盘的分区",
                "udisksctl unmount -b /dev/<分区名>; sudo wipefs -a /dev/<磁盘名> && sudo parted -s /dev/<磁盘名> mklabel gpt mkpart data 0% 100%",
                "**会清除整个磁盘上的所有数据。**`-s` 表示不逐项询问。").Then("wipefs 列出被擦除的签名；parted 没有输出。`lsblk` 显示该磁盘下只有一个新分区（如 `sda1`），尚无文件系统。"),
            Step.Danger(
                "格式化为 ext4，并让根目录属于自己",
                "sudo mkfs.ext4 -L <卷标> -E root_owner=$(id -u):$(id -g) /dev/<分区名>",
                "`root_owner` 让你挂载后无需 sudo 就能写入。").Then("输出创建过程（块数、inode 数、写入超级块），最后 `写入超级块和文件系统账户统计信息： 已完成`。"),
            Step.Danger(
                "或者格式化为 exFAT（与 Windows/macOS 通用）",
                "sudo mkfs.exfat -L <卷标> /dev/<分区名>",
                "exFAT 没有 Linux 权限，挂载后普通用户可以直接写入。").Then("输出 `Format success.`。"),
            Step.Root(
                "检查并修复文件系统（卸载状态下）",
                "udisksctl unmount -b /dev/<分区名>; sudo fsck.ext4 -f /dev/<分区名>",
                "exFAT 分区改用 `sudo fsck.exfat /dev/<分区名>`。").Then("逐项输出 `第 1 步：检查 inode、块和大小` 到第 5 步，最后一行汇总文件数与块使用情况；发现错误时会询问是否修复。"),
        ],

        Verify =
        [
            "1. `udisksctl mount -b /dev/<分区名>` 挂载后，`touch /run/media/deck/<卷标>/test` 成功，说明普通用户可写。",
        ],
    };
}
