using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.StorageAndFilesystems;

public static class MountsAndOverlayFsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "mounts-overlayfs",
        Title = "挂载与 OverlayFS",
        Keywords = "findmnt · bind mount · overlay · 命名空间",
        Level = ModuleLevel.Advanced,
        Overview = "Linux 只有一棵目录树，硬盘、SD 卡、内存盘都通过「挂载」接到树上的某个位置。bind mount 和 OverlayFS 是容器、Flatpak 和 SteamOS 的 /etc 能够工作的基础，而且普通用户就能亲手做一个 OverlayFS 实验。",

        Why =
        [
            "1. 单一目录树：没有 C 盘、D 盘，所有存储设备都挂载到 `/` 下的某个目录。SteamOS 上 `/` 是当前槽位的 rootfs，`/home` 是 home 分区，SD 卡在 `/run/media/deck/<卷标>`。",
            "2. 常见挂载选项：",
            "- `ro` / `rw`：只读 / 读写。",
            "- `noexec`：禁止执行该挂载点里的程序；`nodev`：忽略设备文件；`nosuid`：忽略 SUID 位。",
            "- 注意：SteamOS 根分区的只读由 btrfs 子卷属性实现，所以 `/` 的挂载选项显示为 `rw`。",
            "3. bind mount：把一个已存在的目录再挂到另一个位置，两处看到的是同一份数据。SteamOS 把 `/var/log`、`/var/lib/flatpak` 等目录从 home 分区 bind 过来；Distrobox、Flatpak 也用它把宿主目录带进容器。",
            "4. OverlayFS：把多个目录叠成一个视图：",
            "- `lowerdir`：下层，只读，永远不会被修改。",
            "- `upperdir`：上层，所有写入都落在这里。",
            "- `workdir`：内核内部使用的临时目录，必须与 upper 在同一文件系统。",
            "- `merged`：挂载点，看到的是上层覆盖下层后的结果。",
            "• 修改下层文件时，内核先把它复制到上层再改（copy-up）；删除下层文件时，在上层放一个设备号为 0,0 的字符设备作为「白化 (whiteout)」标记，让它在合并视图里消失。",
            "• SteamOS 的 `/etc` 就是这样实现的：系统镜像里的 `/etc` 是下层，你的修改在 `/var/lib/overlays/etc/upper`。",
            "5. 挂载命名空间：每个进程可以拥有自己的挂载视图。`unshare -rm` 创建一个新的用户 + 挂载命名空间，里面的挂载只对这个进程可见，这正是下面的实验不需要 root 的原因。",
        ],

        Consequences =
        [
            "• `findmnt`、`lsblk` 只读无害。",
            "• 下面的 OverlayFS 实验全部在 `/tmp/ovl` 下进行，挂载只存在于 `unshare` 启动的那个 shell 中，退出即消失，不影响系统。",
            "• 实验中的 `work` 目录会被内核设为无权限，清理时要先 `chmod`，否则 `rm` 会报「权限不够」。",
        ],

        Steps =
        [
            Step.Read(
                "以树形查看所有真实文件系统的挂载",
                "findmnt --real",
                "`--real` 排除 proc、sysfs 等虚拟文件系统。").Then("以 `/` 为根的树：`/home`、`/var`、`/esp`、`/efi`、SD 卡，以及 `/var/log` 等由 home 分区绑定过去的目录，每行带来源设备、类型和挂载选项。"),
            Step.Read(
                "查看 / 与 /home 的来源和挂载选项",
                "for m in / /home; do findmnt -no TARGET,SOURCE,FSTYPE,OPTIONS $m; done",
                "`-n` 不显示表头，`-o` 指定列。").Then("两行：`/ /dev/nvme0n1p5 btrfs rw,relatime,...` 与 `/home /dev/nvme0n1p8 ext4 rw,relatime`。"),
            Step.Read(
                "查看 /etc 的 OverlayFS 参数",
                "findmnt -no OPTIONS /etc | tr ',' '\\n'",
                "`tr` 把逗号换成换行，便于阅读。").Then("每行一个挂载选项，其中有 `lowerdir=/new_root/etc`、`upperdir=/new_root/var/lib/overlays/etc/upper`、`workdir=...`。"),
            Step.Change(
                "准备实验目录",
                "mkdir -p /tmp/ovl/{lower,upper,work,merged} && echo '下层原文件' > /tmp/ovl/lower/a.txt",
                "`{a,b}` 是 bash 的花括号展开，一次创建多个目录。").Then("没有输出。`/tmp/ovl` 下出现 `lower`、`upper`、`work`、`merged` 四个目录，`lower/a.txt` 内容为「下层原文件」。").Practice(),
            Step.Change(
                "在私有命名空间里挂载 OverlayFS 并做修改",
                "unshare -rm sh -c 'mount -t overlay overlay -o lowerdir=/tmp/ovl/lower,upperdir=/tmp/ovl/upper,workdir=/tmp/ovl/work /tmp/ovl/merged && rm /tmp/ovl/merged/a.txt && echo 新文件 > /tmp/ovl/merged/b.txt && ls -l /tmp/ovl/merged'",
                "`-r` 映射为命名空间内的 root，`-m` 创建私有挂载命名空间。").Then("输出合并视图的列表：只有新建的 `b.txt`，被删除的 `a.txt` 已看不到。命令结束后挂载随命名空间一起消失。").Practice(),
            Step.Read(
                "观察各层的变化",
                "cat /tmp/ovl/lower/a.txt; ls -l /tmp/ovl/upper",
                "下层从未被修改，所有变化都记录在上层。").Then("第一行仍是「下层原文件」；`upper` 中有新文件 `b.txt`，以及一个权限为 `c---------`、大小位置显示 `0, 0` 的 `a.txt`，这就是删除标记 (whiteout)。"),
            Step.Change(
                "清理实验目录",
                "chmod -R u+rwx /tmp/ovl && rm -rf /tmp/ovl",
                "先恢复 work 目录的权限再删除，否则 `rm` 会报「权限不够」。").Then("没有输出，`/tmp/ovl` 被删除。").Practice(),
        ],

        Verify =
        [
            "1. 实验结束后 `ls /tmp/ovl/lower/a.txt` 仍然存在且内容不变，证明 OverlayFS 从不修改下层。",
            "2. `ls -l /tmp/ovl/upper/a.txt` 的第一个字符是 `c`（字符设备）且大小位置显示 `0, 0`，即 whiteout 标记。",
            "3. 在另一个终端执行 `findmnt /tmp/ovl/merged`，没有任何输出：实验中的挂载只存在于 unshare 的命名空间内。",
        ],
    };
}
