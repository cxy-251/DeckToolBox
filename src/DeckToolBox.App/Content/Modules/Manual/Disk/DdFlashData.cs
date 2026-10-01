using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Disk;

public static class DdFlashData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-dd",
        Title = "dd：写入系统镜像到 U 盘",
        Keywords = "dd if= of= · bs · status=progress · conv=fsync · 恢复镜像",
        Level = ModuleLevel.Intermediate,
        Overview = "制作 SteamOS 恢复盘、Linux 安装盘时，需要把 .img 或 .iso 镜像逐字节写到 U 盘上。dd 能完成这件事，它也能生成测试文件、测量磁盘写入速度。dd 会直接覆盖目标设备，写错设备的后果不可挽回。",

        Why =
        [
            "1. dd 的参数格式是 `名称=值`：",
            "- `if=`：输入文件 (input file)，如镜像文件或 `/dev/zero`。",
            "- `of=`：输出文件 (output file)，写镜像时是**整个磁盘**（如 `/dev/sda`），而不是分区（`/dev/sda1`）。",
            "- `bs=4M`：每次读写的块大小，较大的值速度更快。",
            "- `count=N`：只复制 N 个块。",
            "- `status=progress`：显示实时进度。",
            "- `conv=fsync` 或 `oflag=direct`：写完后确保数据真正落盘，而不是停留在内存缓存中。",
            "2. SteamOS 恢复镜像：从 Valve 官网下载 `steamdeck-repair-*.img.bz2`，用 `bzcat` 解压的同时交给 dd 写入 U 盘，然后按住「音量 −」开机进入引导菜单，从 U 盘启动。",
            "3. 解压与写入可以用管道一步完成：`bzcat 镜像.img.bz2 | sudo dd of=/dev/sdX bs=4M status=progress conv=fsync`。",
            "4. 其他用途：`dd if=/dev/zero of=文件 bs=1M count=100` 生成 100 MB 的测试文件；配合 `oflag=direct` 可以粗略测量磁盘的顺序写入速度。",
        ],

        Consequences =
        [
            "• **`of=` 写错设备会立即、无提示地覆盖该设备上的全部数据**，包括写成内置硬盘 `nvme0n1`。执行前用 `lsblk` 核对设备名和容量，U 盘插拔前后各看一次。",
            "• 写镜像期间不要拔出 U 盘；dd 结束后等待 `sync` 完成再拔。",
            "• 写入后 U 盘原有的分区和文件全部消失；日后想把它当普通 U 盘用，需要重新分区格式化（见「分区、格式化与检查」）。",
        ],

        Steps =
        [
            Step.Change(
                "生成一个 100 MB 的测试文件并测量写入速度",
                "dd if=/dev/zero of=/tmp/dd-test.bin bs=1M count=100 oflag=direct status=progress",
                "`oflag=direct` 绕过缓存，测得的是实际写入速度。`/tmp` 是内存盘，换成 SD 卡上的路径可以测 SD 卡的速度。").Then("显示一行不断刷新的进度，结束时输出 `输入了 100+0 块记录`、`输出了 100+0 块记录` 和 `104857600 字节 (105 MB, 100 MiB) 已复制，0.07 s，1.5 GB/s`（写入内存盘所以很快）。"),
            Step.Read(
                "确认 U 盘的设备名",
                "lsblk -d -o NAME,SIZE,MODEL,TRAN",
                "`TRAN` 为 usb 的就是 U 盘。").Then("U 盘通常为 `sda`，大小与其标称容量接近。"),
            Step.Danger(
                "把 .img 镜像写入 U 盘",
                "udisksctl unmount -b /dev/<磁盘名>1 2>/dev/null; sudo dd if=<镜像文件>.img of=/dev/<磁盘名> bs=4M status=progress conv=fsync",
                "**会清除该磁盘全部数据**。`<磁盘名>` 依上步 `lsblk` 确认（如 `sda`，严禁填 `nvme0n1`）；`<镜像文件>` 为镜像名；`of=` 指向整盘；`conv=fsync` 确保落盘。").Then("显示实时写入进度（已写字节数、耗时、速度），结束后输出总计；U 盘上随即出现镜像中的分区。"),
            Step.Danger(
                "边解压边写入 SteamOS 恢复镜像",
                "bzcat ~/Downloads/steamdeck-repair-*.img.bz2 | sudo dd of=/dev/<磁盘名> bs=4M status=progress conv=fsync",
                "`<磁盘名>` 填入上步确认的 U 盘盘符（如 `sda`）。`bzcat` 流式解压直接管道写入，省去解压中转文件。").Then("与上一步相同的进度输出，完成后这个 U 盘就是可启动的恢复盘。"),
            Step.Read(
                "确认数据已全部写入",
                "sync && lsblk -o NAME,SIZE,FSTYPE,LABEL /dev/<磁盘名>",
                "`<磁盘名>` 填入对应设备；`sync` 强制刷出内核脏页，命令返回后方可安全拔出 U 盘。").Then("`sync` 返回后列出 U 盘上新的分区结构，此时可以安全拔出。"),
        ],

        Verify =
        [
            "1. 写入恢复镜像后，关机，按住「音量 −」再按电源键，在引导菜单中能看到这个 U 盘。",
            "2. 练习结束后执行 `rm /tmp/dd-test.bin` 删除测试文件。",
        ],
    };
}
