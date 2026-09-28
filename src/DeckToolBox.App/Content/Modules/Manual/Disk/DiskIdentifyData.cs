using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Disk;

public static class DiskIdentifyData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-disk-identify",
        Title = "lsblk、findmnt、blkid",
        Keywords = "识别磁盘与分区 · UUID · PARTLABEL",
        Level = ModuleLevel.Basic,
        Overview = "在对任何磁盘做操作（挂载、格式化、写镜像）之前，第一步永远是准确认出它是哪一个设备。lsblk 按设备树列出磁盘与分区，findmnt 按挂载点列出，blkid 读取分区的文件系统签名。",

        Why =
        [
            "1. 设备命名规则：",
            "- `nvme0n1`：第 1 块 NVMe 固态硬盘；分区为 `nvme0n1p1`、`p2`…",
            "- `mmcblk0`：SD 卡；分区为 `mmcblk0p1`。",
            "- `sda`、`sdb`：USB 硬盘、U 盘；分区为 `sda1`、`sda2`。",
            "- `loop0`：把文件当作磁盘使用的回环设备；`zram0`：内存压缩交换设备。",
            "• 设备名按识别顺序分配，插拔后可能变化；脚本和 `/etc/fstab` 中应使用 UUID 或 LABEL 指代分区。",
            "2. lsblk 常用列（`-o 列1,列2`）：",
            "- `SIZE`、`TYPE`（disk/part/loop）、`FSTYPE`、`LABEL`（卷标）、`UUID`。",
            "- `PTTYPE`（分区表类型 gpt/dos）、`PARTLABEL`（GPT 分区名，SteamOS 上可以看到 rootfs-A 等）。",
            "- `MOUNTPOINTS`、`RM`（可移除）、`HOTPLUG`、`TRAN`（接口）、`MODEL`。",
            "- `-f` 是常用文件系统相关列的简写；`-d` 只列磁盘不列分区；`-J` 输出 JSON。",
            "3. findmnt：以挂载点为中心，`-t ext4,btrfs` 按类型筛选，`findmnt 路径` 查看某个路径位于哪个文件系统。",
            "4. blkid：读取分区上的文件系统签名（类型、UUID、卷标）。需要读取设备本身，普通用户没有权限时**没有任何输出**，要加 sudo。",
        ],

        Consequences =
        [
            "• 本模块的命令全部只读。",
            "• 对磁盘执行写操作前，至少核对两项：大小（SIZE）与型号或卷标，确保它就是你要操作的那一个。",
        ],

        Steps =
        [
            Step.Read(
                "列出磁盘与分区及其文件系统",
                "lsblk -o NAME,SIZE,TYPE,FSTYPE,LABEL,MOUNTPOINTS",
                "树形显示磁盘与其下的分区。").Then("看到 `nvme0n1`（内置硬盘）下的 8 个分区、`mmcblk0`（SD 卡）及其分区，以及 `zram0`；插着 U 盘时还有 `sda`。"),
            Step.Read(
                "查看分区表与 GPT 分区名",
                "lsblk -o NAME,SIZE,PTTYPE,PARTLABEL,UUID",
                "`PARTLABEL` 是 GPT 分区的名称。").Then("内置硬盘与 SD 卡都是 `gpt` 分区表；内置硬盘分区名为 `esp`、`efi-A`、`rootfs-A`、`var-B`、`home` 等，每个分区都有唯一的 UUID。"),
            Step.Read(
                "只列出磁盘本身",
                "lsblk -d -o NAME,SIZE,MODEL,TRAN,RM",
                "`-d` 不显示分区。").Then("每个磁盘一行，如 `nvme0n1 476.9G Micron_2400_... nvme 0`；`RM` 为 1 表示可移除设备。"),
            Step.Read(
                "查看某个路径位于哪个分区",
                "findmnt -T ~/Downloads; findmnt -T /run/media/deck/* 2>/dev/null | tail -n 1",
                "`-T` 查询包含该路径的挂载点。").Then("第一条显示家目录所在的 `/home`（`/dev/nvme0n1p8`，ext4），第二条显示 SD 卡的挂载点与设备。"),
            Step.Read(
                "只列出真实磁盘上的文件系统",
                "findmnt -t ext4,btrfs,vfat,exfat -o TARGET,SOURCE,FSTYPE,OPTIONS",
                "`-t` 按文件系统类型筛选。").Then("以树形列出 `/`（btrfs）、`/home`、`/var`、SD 卡等，以及从 home 分区绑定出去的各个目录（SOURCE 中带 `[/.steamos/offload/...]`）。"),
            Step.Root(
                "读取分区的文件系统签名",
                "sudo blkid",
                "普通用户执行没有输出。").Then("每个分区一行：`/dev/nvme0n1p8: LABEL=\"home\" UUID=\"...\" TYPE=\"ext4\" PARTLABEL=\"home\" ...`。"),
        ],

        Verify =
        [
            "1. 插入一个 U 盘前后各执行一次 `lsblk -d`，新多出的那一行就是它。",
        ],
    };
}
