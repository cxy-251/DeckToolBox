using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class RemovableDrivesData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "removable-drives",
        Title = "U 盘、SD 卡与移动硬盘",
        Keywords = "lsblk · df · udisksctl · 格式化 · 安全弹出",
        Level = ModuleLevel.Basic,
        Overview = "识别插入的存储设备、查看剩余空间、挂载和安全弹出，以及选对文件系统格式。命令行里的操作和 Dolphin 侧栏里点「安全移除」背后是同一套机制（udisks）。",

        Why =
        [
            "1. 设备名：",
            "- `nvme0n1`：内置固态硬盘，`nvme0n1p8` 是它的第 8 个分区。",
            "- `mmcblk0`：microSD 卡，分区为 `mmcblk0p1`。",
            "- `sda`、`sdb`：USB 接口的 U 盘或移动硬盘，分区为 `sda1` 等。",
            "2. 挂载位置：插入后，桌面会自动把分区挂载到 `/run/media/deck/<卷标>`。游戏模式中格式化的 SD 卡也挂载在这里。",
            "3. 为什么要「安全弹出」：写入的数据会先放在内存缓存里，稍后才真正写到设备。直接拔出可能丢失最后写入的数据，甚至损坏文件系统。卸载 (unmount) 会先把缓存全部写回。",
            "4. 选择文件系统：",
            "- **exFAT**：Windows、macOS、Linux 都能读写，适合在不同电脑之间拷文件的 U 盘。不支持 Linux 权限与符号链接。",
            "- **ext4**：Linux 原生，支持权限，最稳定。Steam 在游戏模式中格式化 SD 卡就用它；放 Steam 游戏库必须用 Linux 文件系统。",
            "- **NTFS**：Linux 能读写（内核 ntfs3 驱动），但不建议用来放 Steam 游戏库。",
        ],

        Consequences =
        [
            "• `lsblk`、`df` 只读。挂载 / 卸载不改变设备上的数据。",
            "• **格式化会清除分区上的全部数据**。执行 `mkfs` 前用 `lsblk` 反复确认设备名：写错成内置硬盘分区（`nvme0n1p*`）会直接毁掉系统或家目录。",
            "• 卸载时若提示 `target is busy`，说明有程序（包括停在该目录下的终端）还在使用它，先关闭这些程序或 `cd ~`。",
        ],

        Steps =
        [
            Step.Read(
                "列出所有存储设备与分区",
                "lsblk -o NAME,SIZE,FSTYPE,LABEL,MOUNTPOINTS,HOTPLUG",
                "`HOTPLUG` 为 1 的是可插拔设备（SD 卡、U 盘）。").Then("树状表格列出内置硬盘、SD 卡、U 盘及其分区的大小、格式、卷标和挂载点。新插入的 U 盘通常显示为 `sda`，分区为 `sda1`。"),
            Step.Read(
                "查看已挂载设备的剩余空间",
                "df -h -x tmpfs -x devtmpfs -x efivarfs",
                "`-x` 排除内存文件系统，只看真实磁盘。").Then("表格：每个真实分区的容量、已用、可用、使用率与挂载点。"),
            Step.Change(
                "挂载一个分区",
                "udisksctl mount -b /dev/<分区名>",
                "`<分区名>`（如 `sda1`）由首步 `lsblk` 查得。无需 sudo。").Then("输出 `Mounted /dev/sda1 at /run/media/deck/<卷标>`，Dolphin 侧栏中出现这个设备。"),
            Step.Change(
                "卸载并断电，然后可以安全拔出",
                "udisksctl unmount -b /dev/<分区名> && udisksctl power-off -b /dev/<磁盘名>",
                "`power-off` 使用磁盘名而不是分区名，例如 `/dev/sda`。").Then("先输出 `Unmounted /dev/sda1.`，随后磁盘断电（U 盘指示灯通常熄灭），它从 `lsblk` 中消失。此时拔出不会丢数据。"),
            Step.Root(
                "修改 exFAT 分区的卷标",
                "sudo exfatlabel /dev/<分区名> <新卷标>",
                "卷标决定挂载目录名。修改前先卸载该分区。ext4 分区用 `sudo e2label /dev/<分区名> <新卷标>`。").Then("输入 sudo 密码后提示新卷标已设置。下次挂载时目录名变为新卷标。"),
            Step.Danger(
                "把 U 盘格式化为 exFAT",
                "udisksctl unmount -b /dev/<分区名>; sudo mkfs.exfat -L <卷标> /dev/<分区名>",
                "`<分区名>`（如 `sda1`）由首步 `lsblk` 确认（绝非 `nvme0n1`）；`<卷标>` 为自定义名称（如 `USB_DATA`）。**清除全部数据**。").Then("先卸载，再显示 mkfs.exfat 的版本与创建过程，最后 `Format success.`。分区上原有的全部文件消失，变成一个空的 exFAT 分区。"),
        ],

        Verify =
        [
            "1. 挂载后 `lsblk` 的 `MOUNTPOINTS` 列显示 `/run/media/deck/<卷标>`，在 Dolphin 侧栏也能看到它。",
            "2. `power-off` 后设备从 `lsblk` 列表中消失，此时拔出是安全的。",
        ],
    };
}
