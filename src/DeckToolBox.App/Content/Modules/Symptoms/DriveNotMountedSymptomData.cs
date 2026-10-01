using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class DriveNotMountedSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-drive-not-mounted",
        Title = "插上 SD 卡或移动硬盘没反应",
        Keywords = "SD 卡 · U 盘 · 移动硬盘 · 不识别 · 不挂载 · NTFS · exFAT",
        Level = ModuleLevel.Basic,
        Overview = "插入 SD 卡、U 盘或移动硬盘后，Steam 的存储设置里看不到它，Dolphin 侧栏也没有。按下面的顺序检查：系统有没有识别到设备、上面是什么文件系统、自动挂载为什么跳过了它。",

        Why =
        [
            "常见原因，按出现频率排序：",
            "1. **文件系统不是 ext4**：SteamOS 只自动挂载 ext4 分区，NTFS、exFAT、FAT32 的盘插上后会被跳过。这不是故障，桌面模式中可以手动挂载使用，但不适合作 Steam 库。",
            "2. **没有分区或文件系统损坏**：新买的盘可能没有分区；拔出前没有卸载的盘可能需要修复。",
            "3. **系统根本没有识别到设备**：接触不良、读卡器或线材问题、扩展坞供电不足，或者卡本身坏了。",
        ],

        Consequences =
        [
            "• 下面的检查都是只读的，不会改动盘上的数据。",
            "• 若决定把盘格式化为 ext4 供 Steam 使用，盘上的数据会全部清除，先把需要的文件复制出来。",
        ],

        Steps =
        [
            Step.Read(
                "系统有没有识别到设备",
                "lsblk -o NAME,SIZE,FSTYPE,LABEL,MOUNTPOINTS,TRAN | grep -vE '^(loop|zram)'",
                "`TRAN` 列是连接方式：`mmc` 是 SD 卡，`usb` 是 USB 设备，`nvme` 是内置固态硬盘。").Then("每个磁盘及其分区一行。**如果列表里没有你的设备** → 系统没识别到，换个接口、重插读卡器或换线再试，仍然没有就可能是设备故障。**如果有设备但没有分区行或 `FSTYPE` 为空** → 盘没有分区或文件系统，见「分区、格式化与检查」。**如果 `FSTYPE` 是 `ntfs`、`exfat` 或 `vfat`** → 是第 2 步说的情况。**如果 `MOUNTPOINTS` 有路径** → 已经挂载好了，问题在 Steam 是否把它加为库。"),
            Step.Read(
                "自动挂载为什么跳过了它",
                "journalctl -b --no-pager -t block-device-event.sh -g 'Mounted|Error' | tail -n 6",
                "这是 SteamOS 自动挂载脚本的日志，`-g` 只保留关键行。插拔一次设备后执行。").Then("**如果看到 `Error mounting /dev/sda1: wrong fstype: ntfs`**（或 exfat、vfat）→ 文件系统不是 ext4，被有意跳过。**如果看到 `**** Mounted ... at /run/media/deck/<卷标> ****`** → 挂载成功。没有任何输出说明脚本没被触发，回到第 1 步确认设备是否被识别。"),
        ],

        Verify =
        [
            "• 只想拷文件：在桌面模式用 Dolphin 侧栏点击该设备，或执行 `udisksctl mount -b /dev/<分区名>`，之后能在 `/run/media/deck/` 下看到它。",
            "• 想作为 Steam 游戏库：在游戏模式「设置 → 存储空间」中格式化它（会清空数据），完成后 Steam 的存储列表中出现这张卡或这块盘。",
        ],

        Related = ["steamos-storage-subsystem", "removable-drives", "man-mkfs-fsck", "man-f3"],
    };
}
