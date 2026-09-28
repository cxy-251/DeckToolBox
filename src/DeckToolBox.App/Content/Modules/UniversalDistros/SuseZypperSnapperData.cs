using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class SuseZypperSnapperData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "opensuse-zypper",
        Title = "openSUSE：Zypper 与快照",
        Keywords = "zypper · patterns · snapper · btrfs",
        Level = ModuleLevel.Intermediate,
        Overview = "openSUSE（Tumbleweed 滚动版、Leap 稳定版）使用 RPM 包和 Zypper 包管理器。它最有特色的是与 Btrfs 快照的整合：每次安装、升级前后自动打快照，出问题可以从启动菜单直接回到升级前。",

        Why =
        [
            "1. Zypper 与 libsolv：openSUSE 开发的 libsolv 是最早把 SAT 求解器用于包依赖的实现之一，后来也被 Fedora 的 DNF 采用。遇到依赖冲突时，Zypper 会列出几种可选的解决方案让你选择。",
            "2. Patterns（模式）：一组相关软件的集合，例如某个桌面环境或开发工具集，一条命令整体安装。",
            "3. Snapper + Btrfs：",
            "• openSUSE 默认根分区为 Btrfs。每次 `zypper` 事务前后，Snapper 自动创建一对 `pre`/`post` 快照，记录这次操作改了什么。",
            "• 快照创建几乎瞬间完成，初始不额外占用空间（写时复制，之后只记录变化的数据块）。",
            "• 系统升级后无法启动时，可在 GRUB 启动菜单中选择旧快照启动，确认无误后执行 `snapper rollback` 把它设为新的默认。",
            "4. Tumbleweed 升级用 `zypper dup`（distribution upgrade），而不是 `zypper up`。",
        ],

        Consequences =
        [
            "• 查询类命令只读。",
            "• 快照会随时间累积占用空间：旧快照保留着已被修改的数据块。`snapper-cleanup.timer` 会按策略自动清理。",
            "• `snapper rollback` 会改变默认启动的子卷，应在确认要回到的快照后执行，并重启生效。",
            "• 在 Distrobox 容器里可以练习 zypper，但**容器中没有 Btrfs 快照**，snapper 相关命令只能在真实的 openSUSE 系统上操作。",
        ],

        Steps =
        [
            Step.Change(
                "在 Deck 上创建一个 openSUSE Tumbleweed 练习容器并进入",
                "distrobox create --name tumbleweed -i registry.opensuse.org/opensuse/tumbleweed:latest && distrobox enter tumbleweed",
                "之后的 zypper 步骤都在容器里执行。"),
            Step.Read(
                "查看软件源及其优先级",
                "zypper lr -d",
                "`-d` 显示 URI、优先级等详情。").On(Hosts.OpenSuse),
            Step.Read(
                "搜索软件包与模式",
                "zypper se <关键词>; zypper patterns",
                "`se` 是 `search` 的简写。").On(Hosts.OpenSuse),
            Step.Root(
                "安装软件包",
                "sudo zypper in <包名>",
                "`in` 是 `install` 的简写。").On(Hosts.OpenSuse),
            Step.Root(
                "Tumbleweed 整体升级",
                "sudo zypper ref && sudo zypper dup",
                "先刷新源，再做发行版级升级。").On(Hosts.OpenSuse),
            Step.Root(
                "列出快照",
                "sudo snapper list",
                "每行一个快照，`Description` 列会写明是哪条 zypper 命令触发的。只能在真实的 openSUSE 系统上执行。").On(Hosts.OpenSuse),
            Step.Root(
                "对比两个快照之间改动了哪些文件",
                "sudo snapper status <旧快照号>..<新快照号>",
                "`snapper diff` 可进一步查看具体文件内容的差异。").On(Hosts.OpenSuse),
            Step.Danger(
                "回滚到指定快照",
                "sudo snapper rollback <快照号>",
                "把该快照设为新的默认根子卷，重启后生效。通常先从 GRUB 菜单用该快照启动验证，再执行此命令。").On(Hosts.OpenSuse),
        ],

        Verify =
        [
            "1. 每执行一次 `zypper in`，`snapper list` 末尾都会多出一对 pre / post 快照。",
            "2. `systemctl status snapper-cleanup.timer` 为 active，说明旧快照会被自动清理。",
        ],
    };
}
