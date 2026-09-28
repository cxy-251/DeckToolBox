using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Disk;

public static class MountLoopData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-mount-loop",
        Title = "挂载分区与镜像文件",
        Keywords = "mount · umount · udisksctl · loop-setup · ISO · img · fstab",
        Level = ModuleLevel.Intermediate,
        Overview = "除了 U 盘和 SD 卡，ISO 光盘镜像、.img 磁盘镜像、游戏模拟器使用的镜像文件也可以挂载成目录直接浏览。udisksctl 让普通用户完成这些操作；mount 需要 root，但能指定更多选项。",

        Why =
        [
            "1. 两种挂载方式：",
            "- `udisksctl mount -b 设备`：普通用户可用，挂载到 `/run/media/deck/<卷标>`，与在 Dolphin 中点击设备效果相同。",
            "- `sudo mount [-o 选项] 设备 目录`：需要 root，可以挂载到任意目录、指定选项；`sudo umount 目录或设备` 卸载。",
            "2. 镜像文件与 loop 设备：",
            "- 镜像文件本身不是块设备，需要先通过 loop 设备「伪装」成磁盘。",
            "- `udisksctl loop-setup -f 文件` 创建 loop 设备（如 `/dev/loop0`），之后像普通磁盘一样挂载；只读挂载加 `-r`。",
            "- 用完后先卸载，再 `udisksctl loop-delete -b /dev/loopN` 释放。",
            "- 对 ISO 文件，Dolphin 右键菜单中的「挂载」做的是同样的事。",
            "3. 常用挂载选项（`-o`）：`ro` 只读、`noexec` 禁止执行程序、`uid=1000,gid=1000`（vfat/exfat/ntfs 等不支持 Linux 权限的文件系统，用它指定文件的所有者）。",
            "4. 开机自动挂载：写入 `/etc/fstab`（每行：设备 UUID、挂载点、类型、选项、两个数字）。SteamOS 的 `/etc` 修改会保留，但写错 fstab 可能导致开机进入紧急模式，务必加 `nofail` 选项。",
        ],

        Consequences =
        [
            "• 挂载本身不修改设备内容；在挂载点中写入才会修改。镜像文件以读写方式挂载时，写入的内容会保存进镜像。",
            "• 新格式化的 ext4 镜像或分区，根目录归 root 所有，普通用户无法写入（见「mkfs 与 fsck：格式化与检查」中的 `root_owner` 选项）。",
            "• 卸载提示 `target is busy` 时，用 `fuser -vm 挂载点` 找出占用的程序（见「lsof 与 fuser：谁在占用」）。",
        ],

        Steps =
        [
            Step.Change(
                "制作一个 64 MB 的练习镜像",
                "cd /tmp && truncate -s 64M demo.img && mkfs.ext4 -q -F -L DEMOIMG -E root_owner=$(id -u):$(id -g) demo.img && file demo.img",
                "`truncate` 创建一个空文件，`mkfs.ext4` 在其中建立文件系统，`root_owner` 让根目录属于你。").Then("`file` 输出 `demo.img: Linux rev 1.0 ext4 filesystem data, UUID=..., volume name \"DEMOIMG\"`。"),
            Step.Change(
                "把镜像映射为 loop 设备",
                "dev=$(udisksctl loop-setup -f /tmp/demo.img | grep -o '/dev/loop[0-9]*'); echo \"$dev\"",
                "不需要 root。把分配到的设备名存进变量 `dev`，后面的步骤在同一个终端里直接使用它。").Then("输出分配到的设备名，如 `/dev/loop0`；`lsblk` 中出现对应的 loop 设备。"),
            Step.Change(
                "挂载并写入",
                "udisksctl mount -b \"$dev\" && echo 你好 > /run/media/deck/DEMOIMG/hello.txt && ls -l /run/media/deck/DEMOIMG",
                "`$dev` 是上一步保存的设备名。").Then("输出 `Mounted /dev/loop0 at /run/media/deck/DEMOIMG`，目录中有 `lost+found` 和刚写入的 `hello.txt`；Dolphin 侧栏也出现了 DEMOIMG。"),
            Step.Change(
                "卸载并释放 loop 设备",
                "udisksctl unmount -b \"$dev\" && udisksctl loop-delete -b \"$dev\"",
                "先卸载，再删除映射。").Then("输出 `Unmounted /dev/loop0.`；`lsblk` 中不再有 loop0。`hello.txt` 已保存在 `demo.img` 中，下次挂载仍在。"),
            Step.Change(
                "只读挂载一个 ISO 镜像（示例）",
                "dev=$(udisksctl loop-setup -r -f ~/Downloads/<文件>.iso | grep -o '/dev/loop[0-9]*') && udisksctl mount -b \"$dev\"",
                "`-r` 只读；ISO 的挂载目录名为光盘卷标。").Then("ISO 内容出现在 `/run/media/deck/<卷标>`，可以直接浏览、复制其中的文件。"),
            Step.Root(
                "用 mount 挂载到指定目录并指定选项（示例）",
                "sudo mkdir -p /mnt/usb && sudo mount -o uid=$(id -u),gid=$(id -g) /dev/<分区名> /mnt/usb",
                "`uid`/`gid` 适用于 exFAT、FAT、NTFS 等不支持 Linux 权限的文件系统。卸载用 `sudo umount /mnt/usb`。").Then("该分区出现在 `/mnt/usb`，其中的文件属于你，可以直接读写。"),
        ],

        Verify =
        [
            "1. 再次 loop-setup 并挂载 `demo.img`，`cat /run/media/deck/DEMOIMG/hello.txt` 输出「你好」。",
            "2. 练习结束后卸载、删除 loop 设备，再执行 `rm /tmp/demo.img`。",
        ],
    };
}
