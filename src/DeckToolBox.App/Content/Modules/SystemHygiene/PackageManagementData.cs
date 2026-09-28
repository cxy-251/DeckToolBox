using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SystemHygiene;

public static class PackageManagementData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "pkg_management",
        Title = "包管理与沙箱治理 (Flatpak & Discover)",
        Subtitle = "系统纯净化",
        CategoryName = "系统纯净化",
        Badge = "基础必修",
        Overview = "分析 Flatpak 沙箱隔绝与运行时冗余弊端，手动清理无用包与孤儿配置，使设备恢复至可控基线状态。",

        WhyCard = new InfoCardItem(
            "📌",
            "底层原因 (Why: 为什么要清理 Flatpak 与 Discover？)",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 沙箱隔离壁垒（Bubblewrap 限制）：Flatpak 应用默认运行在受限命名空间中。浏览器、编辑器等无法直接读写游戏目录 (~/Games) 或挂载的 MicroSD 卡，开发 Mod 时会遭遇隐蔽的 EACCES 权限拒绝错误。",
                "2. 底层存储严重虚胖（运行时重复捆绑）：每个 Flatpak 应用都依赖庞大的运行环境 (如 org.kde.Platform)。哪怕装一个几十兆的小工具，也可能在后台拉取 2GB~4GB 的基础运行时，严重挤占掌机闪存。",
                "3. Discover 商店后台常驻开销：Discover 会在后台周期性拉取 AppStream 元数据索引，生成诸如“Install Firefox”之类的幽灵快捷方式，诱导用户陷入沙箱软件依赖循环。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "操作后果与影响边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 影响范围：通过 Discover 或 Flatpak 安装的应用将被移除；~/.var/app/ 下由 Flatpak 生成的历史缓存将被清空，释放数吉字节空间。",
                "• 安全边界：Steam 官方客户端、正版游戏、以及 ~/Applications 目录下的便携软件完全不受影响。无需关闭系统的只读根保护 (steamos-readonly)，无需 sudo 密码。",
                "• 可逆性：清理仅针对用户持久分区 (/home)，后续如果真有特殊需要，依然随时可以通过 flatpak install 命令单点装回。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "审计当前已安装的所有 Flatpak 应用程序", "flatpak list --app", "查看当前系统与用户空间安装的 Flatpak 应用程序清单。"),
            new(2, "全量卸载所有用户级 Flatpak 应用（按需）", "flatpak uninstall --all --user -y", "-y 自动确认，--user 仅针对普通用户空间，无需 root 提权。"),
            new(3, "清除失去应用程序依赖的孤立运行时 (Runtime)", "flatpak uninstall --unused --user -y", "卸载孤儿基础包（如 KDE/GNOME Platform 运行时），通常可释放 1GB~5GB 磁盘。"),
            new(4, "清空 ~/.var/app 下遗留的孤儿应用缓存与数据", "rm -rf ~/.var/app/*", "Flatpak 卸载默认保留应用数据，手动清空可彻底消除幽灵配置文件。"),
            new(5, "校验并修复本地 ostree 对象仓库（碎片整理）", "flatpak repair --user", "回收标记为废弃的底层对象块，彻底将存储空间归还给操作系统。")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "结果验证与状态检查 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "在终端执行检查指令：flatpak list && ls -la ~/.var/app/",
                "预期输出：flatpak list 输出为空；~/.var/app/ 仅剩下 . 与 ..，系统完全恢复干净。"
            }
        )
    };
}
