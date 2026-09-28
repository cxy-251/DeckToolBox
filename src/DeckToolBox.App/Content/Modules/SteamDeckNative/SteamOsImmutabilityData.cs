using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamOsImmutabilityData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steamos-immutable",
        Title = "只读系统与 A/B 更新",
        Keywords = "steamos-readonly · RAUC · 更新通道 · 恢复",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 3.x 的系统分区是只读的，并且有 A、B 两套。理解这套设计，就知道哪些改动会在系统更新后消失、哪些会永久保留，以及系统坏了怎么救回来。",

        Why =
        [
            "1. 两套系统槽位 (A/B)：",
            "• 内置固态硬盘上，系统相关分区都成对出现：`efi-A/efi-B`、`rootfs-A/rootfs-B`（各 5 GB，btrfs）、`var-A/var-B`（各 256 MB）；最后一个大分区 `home` 存放你的全部数据和游戏。",
            "• 系统更新时，更新程序（steamos-atomupd，底层用 **RAUC**）把新系统镜像写进**当前没在用的那个槽位**，写完校验通过后才把下次启动指向它。更新过程中断电也不会弄坏正在运行的系统。",
            "• 新系统若启动失败，引导程序会回退到原来的槽位；开机时按住 `…` 键还能手动选择从哪个槽位启动。",
            "2. 只读根分区：",
            "• `/`（含 `/usr`）是 btrfs 子卷，靠 btrfs 的 `ro` 属性锁成只读，所以 `mount` 输出里看到的是 `rw`，要用 `btrfs property get / ro` 才能看到真实状态。",
            "• 用 `steamos-readonly disable` 解锁后可以用 `pacman` 装包，但**下次系统更新会换成全新的槽位镜像，装进 `/usr` 的东西全部消失**。",
            "3. 哪些改动会被保留：",
            "- `/home`：独立分区，系统更新从不触碰。游戏、Flatpak 应用、`~/.local`、`~/Applications` 都在这里。",
            "- `/etc`：是一层 overlay，你的修改保存在 `/var/lib/overlays/etc/upper`，更新后会叠加回新系统。",
            "- `/var`：随槽位成对，但 `/var/log`、`/var/lib/flatpak`、`/var/cache/pacman` 等常用目录实际绑定挂载自 `/home` 分区，所以同样会保留。",
            "4. 更新通道：`rel`（稳定版）、`beta`、`preview`、`main`。切换通道只影响之后收到的更新。",
        ],

        Consequences =
        [
            "• 查看状态（`steamos-readonly status`、`rauc status`、`findmnt`）完全只读。",
            "• 解锁只读是临时的：改动会在下次系统更新时被覆盖，甚至可能让更新出错。需要长期使用的软件应该用 Flatpak、便携应用或 Distrobox 容器。",
            "• 如果确实解锁过，操作完成后立即重新锁定，减少误操作的窗口期。",
            "• `sudo` 需要先给 deck 用户设置密码（桌面模式下在终端执行 `passwd`），SteamOS 出厂时没有密码。",
            "• 切换到 `beta` / `main` 通道可能遇到未完成的功能；切回 `rel` 后要等稳定版追上当前版本号才会收到「更新」。",
        ],

        Steps =
        [
            Step.Read(
                "查看只读保护是否开启",
                "steamos-readonly status",
                "输出 `enabled` 表示根分区处于只读保护状态。"),
            Step.Read(
                "查看根子卷的真实只读属性",
                "btrfs property get / ro",
                "`ro=true` 才是只读的真正依据；`mount` 里显示 `rw` 是正常的。"),
            Step.Read(
                "查看磁盘分区布局",
                "lsblk -o NAME,SIZE,FSTYPE,PARTLABEL,MOUNTPOINTS",
                "`PARTLABEL` 列能直接看到 rootfs-A / rootfs-B / var-A / var-B / home；挂载在 `/` 的就是当前槽位。"),
            Step.Read(
                "查看 A/B 槽位状态",
                "rauc status",
                "`Booted from` 是当前槽位，每个槽位的 `boot status: good` 表示可正常启动。"),
            Step.Read(
                "查看 /etc 的覆盖层实现",
                "findmnt /etc",
                "`upperdir=` 指向保存你修改内容的位置。"),
            Step.Read(
                "查看系统版本与当前更新通道",
                "grep -E '^(VERSION_ID|BUILD_ID)=' /etc/os-release; steamos-select-branch -c",
                "通道 `rel` 为稳定版。"),
            Step.Read(
                "检查是否有可用的系统更新",
                "steamos-update check",
                "只检查不安装；安装更新建议在「设置 → 系统」里操作。"),
            Step.Danger(
                "临时解除只读保护",
                "sudo steamos-readonly disable",
                "解锁后装进 `/usr` 的任何东西都会在下次系统更新时丢失。只在你清楚要改什么时使用。"),
            Step.Root(
                "重新开启只读保护",
                "sudo steamos-readonly enable",
                "改完立刻锁回。"),
        ],

        Verify =
        [
            "1. 保护状态：`steamos-readonly status` 为 `enabled`，且 `btrfs property get / ro` 输出 `ro=true`。",
            "2. 槽位健康：`rauc status` 中两个槽位的 `boot status` 都是 `good`。",
            "3. 救援手段：关机状态下按住 `…` 键再按电源键，可进入启动槽位选择菜单，选择上一个系统版本启动；按住「音量 −」再按电源键进入引导管理器，可从 U 盘上的恢复镜像启动。",
        ],
    };
}
