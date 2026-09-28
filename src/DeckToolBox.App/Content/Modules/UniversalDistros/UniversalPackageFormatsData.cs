using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class UniversalPackageFormatsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "universal_package_formats",
        Title = "全平台通用沙箱与打包格式深度横评 (Flatpak vs Snap vs AppImage)",
        Subtitle = "广域 Linux 发行版宇宙",
        CategoryName = "通用沙箱与跨发行版打包",
        Badge = "全景横评",
        Overview = "全面深度对比现代 Linux 跨发行版分发格式三剑客 (Flatpak, Snap, AppImage)，从内核隔离层、挂载分发模型、权限安全门户到生命周期性能开销进行终极剖析。",

        WhyCard = new InfoCardItem(
            "📌",
            "三大通用格式的技术流派全景对比与底层架构本质",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 统一分发迫切需求：从“一软件打十种包”到“跨发行版通吃”：",
                "• 过去软件作者要为 Debian 11/12、Ubuntu 22/24、Fedora、Arch 分别维护独立的编译链，因 glibc 版本和依赖库微小差异苦不堪言。由此诞生了三大通用打包流派：",

                "2. Flatpak (Red Hat / FreeDesktop 主导的桌面沙箱联盟)：",
                "• **技术底座**：采用类似 Git 的 **OSTree** 分层块存储 + 内核 **Bubblewrap (`bwrap`)** 用户命名空间隔离。",
                "• **设计取向**：专攻图形桌面应用。所有硬件交互（打开文件、使用麦克风、摄像头）均必须走 D-Bus XDG Desktop Portals 权限门户。应用共享统一的 Platform 运行时。",
                "• **优劣势**：安全性极高、运行时去重好；缺点是初次拉取运行时体积庞大 (数 GB)，且天然无法胜任命令行与无界面的系统后台守护进程。",

                "3. Snap (Canonical / Ubuntu 官方主导的集中式生态)：",
                "• **技术底座**：**SquashFS** 压缩镜像 + Linux 内核 **Loop 虚拟块设备挂载** + **AppArmor** 强制访问控制 (MAC)。",
                "• **设计取向**：全能型选手（兼顾桌面软件与命令行/系统后台守护服务）。支持自启与静默差分后台自动热更新。",
                "• **优劣势**：支持系统级服务；缺点是每个 Snap 包都要挂载一个独立的 `/dev/loop*` 设备导致 `lsblk` 极其杂乱、冷启动稍慢、且后端软件商店由 Canonical 闭源独家垄断。",

                "4. AppImage (开源社区极客钟爱的单文件免安装绿色流派)：",
                "• **技术底座**：将二进制与所有动态库打包为单个 ISO9660/SquashFS 镜像，运行时通过 **FUSE (用户态文件系统)** 瞬时自解压挂载为只读环境执行。",
                "• **设计取向**：纯粹绿色免安装。双击即跑，删掉单个文件即彻底卸载，0 依赖、0 守护进程、0 权限污染。",
                "• **优劣势**：最受开发者与高级玩家推崇；缺点是不同 AppImage 无法共享通用运行时（各自打包 libc/Qt，多装浪费闪存）、默认无沙箱防护（可直接读写 $HOME）。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "选型标准与系统资源消耗边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 掌机/便携环境最佳实践：开发工具、Mod 补丁器优先选用 **AppImage**（直接放入 `~/Applications`，完全免 root，OTA 升级绝不受影响）；",
                "• 隔离浏览器/聊天工具优先选用 **Flatpak**（防恶意脚本渗透主机）；",
                "• 避免在只读系统强行折腾 Snap：Snapd 深度依赖 systemd 根服务与 AppArmor 策略注入，在不可变系统上安装难度极高且开销过大。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看当前系统已安装的 Flatpak 应用与运行时所占空间分布",
                "flatpak list --columns=application,runtime,size 2>/dev/null || echo \"当前未安装 Flatpak 环境\"",
                "查看每个应用实际绑定的底层 Runtime 平台版本号与物理占用。"
            ),
            new(
                2,
                "排查系统当前被挂载为只读镜像的 Loop 块设备列表 (Snap / 镜像检测)",
                "lsblk -o NAME,TYPE,FSTYPE,MOUNTPOINT | grep loop | head -n 15",
                "直观观测 Snap 或沙箱环境如何在内核中挂载虚拟循环设备。"
            ),
            new(
                3,
                "实战掌握在无 FUSE 驱动环境下强制提取 AppImage 内容的救急命令",
                "echo 'AppImage 提取指令: ./App.AppImage --appimage-extract'",
                "在无 root 与无 FUSE 环境下强行解压出 AppImage 的完整 AppDir 可执行目录。"
            ),
            new(
                4,
                "实战掌握覆盖 Flatpak 沙箱文件权限使其穿透读取宿主目录的命令",
                "echo 'Flatpak 穿透赋权: flatpak override --user --filesystem=~/Games <AppID>'",
                "覆盖 Flatpak 沙箱文件系统权限，彻底解决沙箱软件找不到本地游戏目录的顽疾。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "技术选型对比矩阵总结 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 隔离级别：Flatpak (Bubblewrap 命名空间隔离) ≈ Snap (AppArmor MAC) > AppImage (默认无沙箱直通宿主)。",
                "2. 便携与自闭环能力：AppImage (单文件随处拷跑，100% 便携) > Flatpak (依赖 flathub 仓库) > Snap (强绑定 Canonical 官方中心服务)。"
            }
        )
    };
}
