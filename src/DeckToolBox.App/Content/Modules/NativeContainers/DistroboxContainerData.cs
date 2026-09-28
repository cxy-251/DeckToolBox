using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NativeContainers;

public static class DistroboxContainerData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "distrobox_container",
        Title = "Distrobox 用户容器系统 (SteamOS 必备开发沙箱)",
        Subtitle = "原生容器与运行时",
        CategoryName = "原生容器与运行时",
        Badge = "杀手级工具",
        Overview = "Valve 官方原生预装的轻量容器利器，数据 100% 驻留 /home 分区，彻底解决只读根与装包冲突，实现无缝穿透宿主机桌面与硬件。",

        WhyCard = new InfoCardItem(
            "📌",
            "为什么 Distrobox 是 SteamOS 玩家与开发者的终极解法？",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 根治只读系统（Immutable OS）被冲刷的痛点：",
                "• SteamOS 的根分区 (/) 默认只读。如果强行关闭写保护执行 pacman -S 装编译工具或软件，一旦 Valve 推送系统 OTA 升级，A/B 分区会被全盘覆盖，你装的所有工具全部灰飞烟灭！",
                "• Distrobox 底层调用原生预装的 Podman (/usr/bin/podman)。容器的所有镜像、安装包和数据 100% 保存在 ~/.local/share/containers/storage/（位于持久化的 /home 分区），系统经历无论多少次更新，你的开发环境永远安全完好！",

                "2. 为什么它比传统 Docker / 虚拟机好用 100 倍？（六大深度无缝穿透）：",
                "• 共享家目录：进容器后，你的 ~/ 就是宿主机的 /home/deck，所有代码与游戏存档直接可见、直接编辑，无需挂载卷搬运。",
                "• 共享显示（Wayland/X11）：容器里运行的图形程序（如 VSCode, GIMP, 终端工具），窗口直接无缝弹在宿主机 KDE 桌面！",
                "• 共享硬件与 GPU：/dev/dri 节点直接穿透，容器内享有与宿主机同等的 Vulkan / OpenGL 硬件渲染与硬件视频解码加速！",
                "• 共享声音与输入：PipeWire 声音、蓝牙手柄、游戏控制器无缝识别。",

                "3. 杀手级特性：distrobox-export（内外无感互通）：",
                "• 导出命令行：在容器内输入 distrobox-export --bin /usr/bin/btop -ep ~/.local/bin，在宿主机终端敲 btop 即可直接运行容器内的工具！",
                "• 导出图形应用：输入 distrobox-export --app gimp，宿主机的 KDE 开始菜单和 Steam 库里立刻出现对应图标，点开直接在后台自动拉起容器运行，就像原生装在宿主机一样！"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "操作后果与影响边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 纯用户空间运行：Distrobox 使用非特权 Podman 容器，完全运行在普通用户权限下，不需要任何 root 密码，绝对不可能损坏 SteamOS 只读根分区。",
                "• 磁盘开销透明：基础 Arch Linux 或 Alpine 容器初始仅占 100MB~300MB 空间，存储随用随涨，完全可控。",
                "• 秒级彻底卸载：若不再需要某个容器，只需一条 distrobox rm 命令，几十秒内彻底清除全部数据，不留任何系统垃圾。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "查看系统自带 Distrobox 版本与已有容器清单", "distrobox list", "检查当前设备上已经创建的容器（如系统中可能已有的开发环境）。"),
            new(2, "一键创建标准的 Arch Linux 开发容器", "distrobox create -n arch-dev -i docker.io/library/archlinux:latest", "免 root 自动拉取镜像并完成宿主机用户身份穿透绑定。"),
            new(3, "进入容器（尽情享受不受限制的 Linux 开发环境）", "distrobox enter arch-dev", "进入后终端提示符切换，可自由执行 sudo pacman -Syu 安装 gcc、python、git 等工具，不怕抹盘！"),
            new(4, "导出容器内的命令行工具到宿主机", "distrobox-export --bin /usr/bin/btop -ep ~/.local/bin", "在容器内执行此指令，宿主机便可直接运行该命令。"),
            new(5, "导出容器内的桌面图形应用至 KDE 菜单与游戏模式", "distrobox-export --app mpv", "自动在宿主机 ~/.local/share/applications/ 生成干净规范的入口。"),
            new(6, "停止并销毁容器（完全清理）", "distrobox stop arch-dev && distrobox rm arch-dev -f", "一条指令彻底回收该容器占用的所有磁盘空间。")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "结果验证与容器状态确认",
            "#238636",
            "#3FB950",
            new[]
            {
                "在宿主机终端输入 distrobox list，查看容器状态是否为 running；",
                "在宿主机终端输入 which btop（如果之前导出了），会看到它直接指向 ~/.local/bin/btop，并且运行后无缝拉起容器内程序！"
            }
        )
    };
}
