using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Disk;

public static class BtrfsCmdData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-btrfs",
        Title = "btrfs 命令：空间、子卷与校验",
        Keywords = "btrfs filesystem df/usage · subvolume · snapshot · scrub · property",
        Level = ModuleLevel.Advanced,
        Overview = "btrfs 的空间分配方式与 ext4 不同，df 显示的可用空间有时不准确，要用 btrfs 自己的命令查看。本模块以 SteamOS 的系统分区为例，介绍查看空间、子卷与只读属性、快照和数据校验 (scrub)。",

        Why =
        [
            "1. 空间以「块组 (chunk)」为单位分配：数据 (Data)、元数据 (Metadata)、系统 (System) 各自占用块组。`btrfs filesystem df` 显示各类已分配与已使用的空间；`btrfs filesystem usage` 显示更完整的统计（普通用户运行时缺少每个设备的明细）。",
            "• 元数据默认以 DUP（双份）方式存储，所以占用是实际数据的两倍。",
            "2. 子卷 (subvolume)：btrfs 内部可以划分多个独立的目录树，各自可以挂载、做快照、设置属性。SteamOS 的根分区就是一个子卷，只读保护由它的 `ro` 属性实现。",
            "3. 快照：`btrfs subvolume snapshot 源 目标` 瞬间复制一个子卷，之后只保存变化的数据；`-r` 生成只读快照。",
            "4. scrub：读取全部数据与元数据并核对校验和，发现静默损坏（例如闪存老化导致的位翻转）；有冗余副本时自动修复。",
            "5. 多数写操作和部分查询（`subvolume list`、`scrub`）需要 root。",
        ],

        Consequences =
        [
            "• 查看类命令只读。",
            "• SteamOS 的根分区由系统管理，不要在其上创建或删除子卷、修改属性；本模块中对它的操作只用于查看。想练习快照，可以在一个 btrfs 格式的 U 盘或镜像文件上进行。",
            "• scrub 会读取整个文件系统，期间磁盘负载较高。",
        ],

        Steps =
        [
            Step.Read(
                "查看根分区各类空间的分配与使用",
                "btrfs filesystem df /",
                "普通用户即可运行。Btrfs 将空间按 Chunk 划分为数据（Data）、元数据（Metadata）与系统（System）；默认采用 DUP 双份元数据备份保证 B-Tree 冗余。").Then("四行：`Data, single: total=4.02GiB, used=3.23GiB`、`System, DUP`、`Metadata, DUP: total=493.50MiB, used=143.91MiB`、`GlobalReserve`。"),
            Step.Read(
                "查看完整的空间统计",
                "btrfs filesystem usage / 2>/dev/null | head -n 12",
                "普通用户运行时会提示缺少设备明细，已隐藏该警告。").Then("`Overall:` 下列出设备大小 (5.00GiB)、已分配、未分配、已用与估计可用空间。"),
            Step.Read(
                "查看根子卷的只读属性",
                "btrfs property get -ts / ro",
                "`-ts` 指定目标对象为子卷（subvolume）。Btrfs 在元数据层标定只读属性，即使挂载选项带有 rw，向其写入依然被内核拒绝。").Then("输出 `ro=true`（SteamOS 只读保护开启时）。"),
            Step.Read(
                "查看当前子卷的信息",
                "btrfs subvolume show / 2>&1 | head -n 8",
                "部分字段需要 root 才能显示。").Then("第一行可能是 `ERROR: Could not search B-tree: Operation not permitted`（权限不足，不影响后面的内容），随后显示子卷名称 `<FS_TREE>`、UUID、创建时间、`Subvolume ID: 5` 等。"),
            Step.Root(
                "列出所有子卷",
                "sudo btrfs subvolume list /",
                "普通用户执行会报 `Operation not permitted`。").Then("每行一个子卷的 ID、生成号与路径；SteamOS 的根分区上通常很少或没有额外子卷。"),
            Step.Root(
                "校验数据完整性",
                "sudo btrfs scrub start -B /",
                "`-B` 在前台运行，完成后才返回。5 GB 的根分区约需几十秒。").Then("结束时输出汇总：扫描的数据量、耗时，以及 `Error summary: no errors found`。"),
        ],

        Verify =
        [
            "1. `sudo btrfs scrub status /` 显示上一次 scrub 的时间与结果。",
        ],
    };
}
