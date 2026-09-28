using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class SuseZypperSnapperData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "opensuse-zypper",
        Title = "openSUSE：Zypper 与快照",
        Keywords = "zypper · snapper · btrfs",
        Level = ModuleLevel.Intermediate,
        Overview = "剖析欧洲工业界首选 openSUSE 体系，解析搭载业界最强 libzypp SAT 依赖引擎的 Zypper、场景化功能套件 Patterns、以及与 Btrfs 无缝联动的 Snapper 开机原子回滚机制。",

        Why =
        [
            "1. 数学逻辑推导的先行者：libzypp SAT 求解器：",
            "• openSUSE 是整个 Linux 领域最早将离散数学中的“布尔可满足性 (SAT) 算法”引入包管理器的先驱。其底层核心库 `libzypp` 逻辑极其严谨，甚至后来被 Red Hat 借用并催生了 DNF。",
            "• 当仓库出现严重依赖冲突时，Zypper 绝不会盲目罢工，而是会清晰列出数种经过逻辑演算的可行方案（如：方案 1 降级 A，方案 2 卸载 B，方案 3 保持现状），供系统管理员做最理性的决策。",
            "2. 场景化打包哲学：Patterns (模式套件)：",
            "• 传统包管理往往零散罗列上千个包，而 openSUSE 提供了独创的 **Patterns**（如 `patterns-openSUSE-kde` 或 `patterns-server-lamp`）。安装一个 Pattern 即可一揽子交付整个业务场景所需的所有关联组件与防火墙预设。",
            "3. 终极系统防翻车神器：Snapper 与 Btrfs 原生快照联动：",
            "• 在 openSUSE 中，包管理器与文件系统形成了梦幻般的深度共生：",
            "  - 每当你执行一次 `zypper install` 或 `zypper update` 时，**Snapper 工具会在事务开始前 1 毫秒瞬间打下一个 Btrfs `pre` 快照**；",
            "  - 事务执行完成后，立即自动打下一个 `post` 快照，精确记录该次安装引发的全盘文件变化！",
            "• **真正的不死系统**：即使某次内核更新后机器彻底黑屏无法进系统，只需在开机 GRUB 引导界面直接选择历史快照启动，进入系统后执行 `snapper rollback`，整台系统瞬间倒流回升级前的一秒钟！",
        ],

        Consequences =
        [
            "• Btrfs 快照累积预警：虽然单个 CoW 快照初始占用空间极小，但如果长期频繁写入大文件而不清理历史快照，随着被覆盖的数据块增多，磁盘空间可能被暗中耗尽。",
            "• 依赖交互方案选择：在使用 zypper 遇到冲突选项时，务必看清方案说明，避免误选“破坏依赖强制忽略 (break dependency)”导致系统动态库失谐。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前系统所有已启用的软件源镜像与优先级 (zypper repos)",
                "zypper lr -d",
                "查看 openSUSE 软件源列表、URI 路径、优先级 Priority 与自动刷新状态。").On(Hosts.OpenSuse),
            Step.Read(
                "查看全量场景化 Patterns 套件清单与安装状态",
                "zypper patterns",
                "一览桌面环境、开发工具箱与服务器角色的场景化 Patterns 功能套件。").On(Hosts.OpenSuse),
            Step.Read(
                "查看全盘所有由事务自动触发的 Btrfs 快照时间线 (snapper list)",
                "snapper list",
                "查看每个快照的 ID、类型 (pre/post)、关联 zypper 运维命令与时间戳。").On(Hosts.OpenSuse),
            Step.Read(
                "比对任意两个快照之间发生变动的文件差异树 (snapper diff)",
                "snapper diff 15..16",
                "比对两次快照之间的全盘文件树差异，精准透视软件安装对系统的每一处改动。").On(Hosts.OpenSuse),
            Step.Danger(
                "实战掌握一键原子级撤销系统修改并回滚至指定快照的操作",
                "sudo snapper rollback <快照ID>",
                "将根文件系统原子还原至目标快照，开机后自动倒流回升级前一刻，系统不倒翁神器。").On(Hosts.OpenSuse),
        ],

        Verify =
        [
            "1. 快照自动清理机制：openSUSE 后台运行 `snapper-cleanup.timer` 定时器，默认会保留最近数个重要快照并自动清理无用的旧快照，执行 `systemctl status snapper-cleanup.timer` 确认服务活跃。",
            "2. 存储空间复核：执行 `btrfs filesystem usage /`，确认未分配空间维持在健康水平。",
        ],
    };
}
