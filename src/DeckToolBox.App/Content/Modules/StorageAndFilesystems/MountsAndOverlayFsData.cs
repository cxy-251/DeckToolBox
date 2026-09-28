using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.StorageAndFilesystems;

public static class MountsAndOverlayFsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "mounts_and_overlayfs",
        Title = "VFS 挂载架构、Bind Mount 与 OverlayFS 联合文件系统",
        Subtitle = "存储体系与联合挂载",
        CategoryName = "存储体系与联合挂载",
        Badge = "存储核心",
        Overview = "深入 Linux VFS 单一树状挂载架构，掌握只读保护机制、Bind Mount 目录穿透与 OverlayFS 四层联合堆叠原理。",

        WhyCard = new InfoCardItem(
            "📌",
            "Linux 统一目录树、挂载机制与 OverlayFS 全景 (Mounting Architecture)",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. Linux 虚拟文件系统 (VFS) 与单一根树：",
                "• Linux 抛弃了 Windows 时代割裂的“C盘、D盘”盘符概念。整个系统只有唯一的一棵根目录树 (/)。所有物理 NVMe 固态硬盘、SD 卡、RAM 内存盘或网络共享，都通过挂载 (Mount) 融入这棵大树的枝干中。",
                "2. 核心挂载属性 (Mount Flags)：",
                "• ro (只读)：底层内核直接拦截写入，这是不可变操作系统抵御误操作、防病毒修改的底层安全防线。",
                "• rw (可读写)：允许持久化修改。",
                "• nodev / noexec：安全防护参数，禁止在该挂载点内执行二进制或创建设备节点。",
                "3. Bind Mount (目录绑定挂载)：",
                "• 允许把一个已经存在的目录直接无缝“映射穿透”到另一个路径。这是 Distrobox、Pressure-Vessel 和 Docker 实现容器共享宿主机文件的基石技术！",
                "4. OverlayFS (联合文件系统核心模型)：",
                "• lowerdir (只读底包)：基础镜像，永不被篡改。",
                "• upperdir (读写层)：所有写入、新增的文件全部落在此层。",
                "• merged (合并视图)：给用户和应用程序呈现的统一目录视角。",
                "• 写时复制 (CoW) 与白斑文件 (Whiteout)：修改只读文件时自动拷贝到上层修改；删除只读文件时在上层打上特殊白斑标记伪装已删除，下层底包永远安全！"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "挂载机制对系统理解的决定性意义 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 明白只读模式的真相：开启 steamos-readonly 时，系统并未关死一切，而是将根 / 以 ro 挂载，而将 /home 以 rw 独立挂载，因此家目录永远具有完全读写权。",
                "• 彻底看懂容器：容器不是虚拟机，容器本质上就是利用 Mount 命名空间结合 Bind Mount 和 OverlayFS 拼凑出的定制化文件系统视图。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "查看系统层级清晰的树状挂载拓扑 (findmnt 经典视图)", "findmnt --real", "清晰列出根 /、持久化 /home、EFI 分区与 SD 卡在整棵树上的挂载位置与挂载选项。"),
            new(2, "查看系统核心根分区与家分区的只读/读写属性", "mount | grep -E \" on / (type|home )\"", "观察 / 上的 ro 只读标志与 /home 上的 rw 读写标志。"),
            new(3, "动手实验：在 /tmp 下零成本亲手搭建一个 OverlayFS 四层联合系统", "mkdir -p /tmp/ovl/{lower,upper,work,merged}\necho \"我是只读底包文件\" > /tmp/ovl/lower/test.txt\n# 挂载联合层 (若有权限可测试，无权限可查看目录结构规范)\nls -la /tmp/ovl/", "亲身理解 lowerdir, upperdir, workdir, merged 的标准目录拓扑。")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "结果验证与状态检查",
            "#238636",
            "#3FB950",
            new[]
            {
                "执行 findmnt，观察根路径 / 的 OPTIONS 列：你会清晰看到 (ro,relatime...)，而 /home 则是 (rw,relatime...)，这是现代不可变系统的最核心设计体现。"
            }
        )
    };
}
