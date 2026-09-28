using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamOsImmutabilityData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steamos_immutability_rauc",
        Title = "SteamOS 不可变系统与 A/B 无缝升级机制 (RAUC / 只读根分区)",
        Subtitle = "不可变系统与更新机制",
        CategoryName = "不可变系统与更新机制",
        Badge = "本机专精",
        Overview = "深入解密 SteamOS 3.x 工业级 A/B 根分区对刷机制、RAUC 控制器、只读根保护锁 (steamos-readonly) 与持久化目录 (/home /etc /var) 安全边界。",

        WhyCard = new InfoCardItem(
            "📌",
            "为什么 Steam Deck 采用不可变系统与 A/B 分区升级？(RAUC Architecture)",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 传统 Linux 滚更的脆弱性 vs 掌机高可靠性要求：",
                "• 传统 Arch/Ubuntu 在线升级包时，一旦中途断电、网络中断或脚本报错，系统极其容易挂掉（动态链接库损坏导致连 bash 都打不开）。",
                "• SteamOS 彻底摒弃传统在线升级，采用嵌入式高可靠方案 **RAUC (Robust Auto-Update Controller)**：固态硬盘中划出完全镜像对称的 `rootfs-A` (如 /dev/nvme0n1p3) 和 `rootfs-B` (如 /dev/nvme0n1p4)。",

                "2. A/B 无缝对刷与原子切换：",
                "• 当 SteamOS 收到系统更新推送时，系统是在后台将全新的只读系统镜像写入**当前非活动的那个备用分区**（例如当前运行 A，写入 B）。写入完成后进行哈希校验，完全不影响你当前正在玩的游戏！",
                "• 只有当你主动点击“重启以应用更新”时，引导加载程序 (EFI Bootloader) 才会原子级切换默认启动分区指向 B。如果 B 分区因驱动兼容性启动失败，内核健康检测机制会自动在 3 次超时后无缝回滚至原先完好的 A 分区！",

                "3. 不可变只读根分区与三大持久化“安全孤岛”：",
                "• 系统根目录 `/` 和 `/usr` 默认以 `ro` (Read-Only) 挂载，防止误删系统动态库。",
                "• **真正持久保存的三个目录**：",
                "  - `/home`：独立的用户数据分区，OTA 系统对刷绝对不会触碰！存放所有游戏、配置与便携应用。",
                "  - `/etc`：系统配置文件，升级时通过专门的合并工具保留用户自定义网络与硬件配置。",
                "  - `/var`：动态系统数据与日志。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "解锁只读保护的代价与 OTA 抹除机制 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 解锁只读保护的后果：执行 `sudo steamos-readonly disable` 允许直接对根分区执行写操作或 `pacman -S` 安装软件；**但是，一旦下次 SteamOS 推送大版本系统更新，非活动分区被重刷，你在 /usr 下安装的全部软件都将被彻底物理抹除！**",
                "• 推荐最佳实践：需要安装复杂编译依赖时，应使用原生预装的 `distrobox`，或者将工具放在 `~/Applications` 和 `~/.local/bin` 中，永久免疫系统更新抹除风险。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看当前系统只读根分区保护状态 (Read-Only Status)",
                "steamos-readonly status",
                "检查系统当前处于 enabled (只读受保护) 还是 disabled (可读写) 状态。"
            ),
            new(
                2,
                "审计系统底层 A/B 分区布局与当前激活的分区槽位 (RAUC)",
                "rauc status --detailed 2>/dev/null || lsblk -o NAME,SIZE,FSTYPE,MOUNTPOINT,LABEL",
                "查看 rootfs.0 与 rootfs.1 的槽位状态，确认当前正在挂载为 / 根目录的具体物理分区。"
            ),
            new(
                3,
                "查看系统的版本构建时间戳与系统镜像标识",
                "cat /etc/os-release | grep -E \"(BUILD_ID|VARIANT_ID|VERSION_ID)\"",
                "获取 Valve 官方发行的 SteamOS 镜像版本号、构建日期与通道分支 (Release / Beta / Main)。"
            ),
            new(
                4,
                "查看 EFI 引导项与双启动槽位顺序 (efibootmgr)",
                "efibootmgr -v 2>/dev/null || echo \"需 root 权限读取 NVRAM 引导列表，常规引导文件位于 /esp/EFI/steamos/\"",
                "查看主板 NVRAM 中注册的 SteamOS 引导顺序以及自动回滚策略。"
            ),
            new(
                5,
                "临时解锁与重新锁定只读保护的操作语法（应急必备）",
                "echo '解锁: sudo steamos-readonly disable ; 锁回: sudo steamos-readonly enable'",
                "仅在必须修改特殊底层驱动配置文件时使用，操作完毕后务必重新 enable 锁回系统。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "系统完整性与分区健康度判定 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 挂载标志验证：执行 `mount | grep \"on / type\"`，输出中必须包含 `(ro,` 字段，证明只读安全机制正常生效。",
                "2. 紧急恢复手段：若因双系统引导损坏导致无法开机，关机状态下长按【音量减 (-) + 电源键】可调出主板 EFI 快速引导菜单；长按【快速访问键 (...) + 电源键】可唤出 SteamOS 历史启动还原点选择菜单。"
            }
        )
    };
}
