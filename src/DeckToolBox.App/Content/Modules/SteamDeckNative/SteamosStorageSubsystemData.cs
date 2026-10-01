using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamosStorageSubsystemData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steamos-storage-subsystem",
        Title = "SD 卡的自动挂载与格式化",
        Keywords = "steamos-automount · block-device-event · format-device · f3probe · casefold · ext4",
        Level = ModuleLevel.Advanced,
        Overview = "插入 SD 卡或 U 盘后 SteamOS 自己的一套脚本决定要不要挂载、怎样挂载；游戏模式里的「格式化 SD 卡」也由脚本完成，并会先检测扩容卡。了解这套流程，就能解释「为什么我的 NTFS 移动硬盘插上没反应」「格式化为什么这么慢」这类问题，并从日志中找到原因。",

        Why =
        [
            "1. 自动挂载的流程：",
            "- udev 规则 `/usr/lib/udev/rules.d/99-steamos-automount.rules` 匹配名为 `mmcblk*`（SD 卡）和 `sd*`（USB 存储）、且含有文件系统的分区，在插入或拔出时用 `systemd-run` 在后台运行 `/usr/lib/hwsupport/block-device-event.sh`。",
            "- 该脚本先排除 SteamOS 自己的系统分区，再调用 `steamos-automount.sh`：先运行 `fsck.ext4 -y` 检查修复，然后通过 UDisks2 以 deck 用户身份挂载到 `/run/media/deck/<卷标>`，挂载选项含 `noatime`。fsck 无法修复时改为只读挂载，并通过 `steam://system/devicemountresult/...` 通知 Steam 显示错误。",
            "- Steam 通过 UDisks2 得知新挂载点，若其中有 `steamapps` 等库目录就加入库列表。",
            "2. 只自动挂载 ext4：脚本中写明 Steam 库需要符号链接，所以**只有 ext4 分区会被自动挂载**；exFAT、NTFS 等分区被跳过，日志记一行 `wrong fstype`。这些分区在桌面模式中可以用 Dolphin 或 `udisksctl mount` 手动挂载（见「U 盘、SD 卡与移动硬盘」），但不适合作为 Steam 库。",
            "3. 游戏模式的格式化：Steam 调用 `/usr/bin/steamos-polkit-helpers/steamos-format-device`（polkit 允许当前用户免密码执行），再由它以 root 运行 `/usr/lib/hwsupport/format-device.sh`：",
            "- 卸载该设备上的分区。",
            "- 默认先用 `f3probe --destructive` 检测真实容量。扩容卡（宣称容量大于实际容量）会被识别出来：脚本把它恢复成 exFAT 并报专门的错误，不允许用作游戏库，避免之后静默丢数据。这一步是格式化耗时较长的主要原因。",
            "- 新建 GPT 分区表和一个占满整盘的分区，执行 `mkfs.ext4 -m 0 -O casefold`：`-m 0` 不为 root 预留空间，`casefold` 开启大小写不敏感特性（脚本注释说明是为了 Proton 兼容）；随后挂载。",
            "4. casefold 只是「允许」：带 `casefold` 特性的 ext4 文件系统，仍然只有被设置了 `F` 属性的空目录（`chattr +F`）才按大小写不敏感处理，其余目录照常区分大小写。`lsattr -d <目录>` 输出中有 `F` 才说明该目录启用了。",
            "5. 日志：自动挂载的输出记在 `block-device-event.sh` 名下，格式化的输出记在 `steamos-format-device` 名下，都可以用 `journalctl -t <名字>` 查看。",
        ],

        Consequences =
        [
            "• 查看规则、脚本、挂载状态和日志都是只读的。",
            "• 格式化会清除整张卡的数据；`f3probe --destructive` 检测阶段同样会覆盖卡上内容。",
            "• 不要修改 `/usr/lib/udev/rules.d/` 或 `/usr/lib/hwsupport/` 下的文件，系统更新会把它们还原。",
        ],

        Steps =
        [
            Step.Read(
                "查看自动挂载的 udev 规则",
                "grep -v '^#' /usr/lib/udev/rules.d/99-steamos-automount.rules",
                "去掉注释行，只看规则本身。").Then("几行规则：`KERNEL!=\"mmcblk*|sd*\"` 与 `ENV{ID_FS_USAGE}!=\"filesystem\"` 时跳过；`ACTION==\"add\"` 与 `\"remove\"` 时各用 `systemd-run --no-block --collect` 运行 `block-device-event.sh add/remove %k`。"),
            Step.Read(
                "确认只有 ext4 会被自动挂载",
                "grep -n -B1 -A4 'only automount ext4' /usr/lib/hwsupport/steamos-automount.sh",
                "`-B1 -A4` 同时显示匹配行前 1 行、后 4 行。").Then("显示脚本中的注释 `We need symlinks for Steam for now, so only automount ext4` 以及紧随其后的判断：文件系统不是 ext4 时输出 `Error mounting ...: wrong fstype` 并退出。"),
            Step.Read(
                "查看最近的自动挂载记录",
                "journalctl -b --no-pager -t block-device-event.sh -g 'Mounted|Error|Checking' | tail -n 10",
                "`-t` 只看这个脚本的日志，`-g` 只保留关键行。插拔一次设备后再执行。").Then("每次插入有一行 `Checking if /dev/sda1 is an OS partition ...`；成功时有 `**** Mounted /dev/mmcblk0p1 at /run/media/deck/<卷标> ****`；NTFS、exFAT 分区则是 `Error mounting /dev/sda1: wrong fstype: ntfs`。"),
            Step.Read(
                "查看已挂载的外置存储及其选项",
                "findmnt -l -o TARGET,SOURCE,FSTYPE,OPTIONS | grep /run/media",
                "没有插 SD 卡或移动硬盘时没有输出。").Then("每个设备一行，如 `/run/media/deck/<卷标>  /dev/mmcblk0p1  ext4  rw,nosuid,nodev,noatime,errors=remount-ro`。"),
            Step.Read(
                "查看格式化脚本中的关键步骤",
                "grep -n -E 'f3probe --destructive|mkfs.ext4|mkfs.exfat|stage=' /usr/lib/hwsupport/format-device.sh",
                "只读地查看脚本，不要直接运行它。").Then("带行号的几行：`echo \"stage=testing\"`、`f3probe --destructive \"$STORAGE_DEVICE\"`、扩容卡回退时的 `stage=rescuing` 与 `mkfs.exfat`、`echo \"stage=formatting\"`，以及 `mkfs.ext4 -m 0 -O casefold ...`。"),
            Step.Read(
                "确认 casefold 特性与目录属性",
                "ls /sys/fs/ext4/features/ | grep casefold; lsattr -d /run/media/deck/*/",
                "第一条确认内核支持；第二条查看 SD 卡根目录的属性，`F` 表示该目录大小写不敏感。").Then("先输出 `casefold` 与 `encrypted_casefold`；再每个挂载点一行属性，如 `--------------e------- /run/media/deck/<卷标>/`：只有 `e`（extent）没有 `F`，说明根目录仍区分大小写。"),
            Step.Read(
                "查看最近一次格式化的日志",
                "journalctl --no-pager -t steamos-format-device | tail -n 20",
                "格式化失败（如提示卡有问题）时用来查看原因。从未在本机格式化过时显示 `-- No entries --`。").Then("依次是 `stage=testing` 与 f3probe 的检测结果（如 `Good news: The device ... is the real thing`）、`stage=formatting` 与 mkfs.ext4 的输出；扩容卡会在检测阶段报告实际可用容量并转去创建 exFAT。"),
            Step.Danger(
                "从终端格式化 SD 卡（与游戏模式相同的流程）",
                "lsblk -o NAME,SIZE,TRAN,MOUNTPOINT /dev/mmcblk0 && /usr/bin/steamos-polkit-helpers/steamos-format-device --device /dev/mmcblk0 --label <卷标>",
                "**清除整张卡的全部数据。**先看 `lsblk` 确认 `mmcblk0` 是 SD 卡且容量正确。`<卷标>` 是挂载目录名，用英文和数字。加 `--skip-validation` 可跳过 f3probe 检测，但不建议。").Then("不需要密码。先输出 `stage=testing`，f3probe 检测可能持续数分钟；通过后输出 `stage=formatting` 与 mkfs.ext4 的创建过程，最后卡被自动挂载到 `/run/media/deck/<卷标>`，Steam 的存储设置中出现这张卡。"),
        ],

        Verify =
        [
            "1. 插入 ext4 格式的 SD 卡后，`findmnt -l | grep /run/media` 能看到它，挂载选项含 `noatime`。",
            "2. 插入 NTFS 移动硬盘后，`journalctl -b -t block-device-event.sh | tail` 中出现 `wrong fstype: ntfs`，这是预期行为而不是故障。",
        ],
    };
}
