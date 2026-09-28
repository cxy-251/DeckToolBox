using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.MemoryAndCgroups;

public static class CgroupsResourceControlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "cgroups_resource_control",
        Title = "cgroups v2 进程控制组与 Linux Capabilities 细化特权",
        Subtitle = "内存模型与资源限制",
        CategoryName = "内存模型与资源限制",
        Badge = "资源管控",
        Overview = "深入现代容器基石 cgroups v2，掌握 CPU/内存配额硬限制；告别粗暴的全局 root，掌握 Linux 细粒度权能分权安全体系。",

        WhyCard = new InfoCardItem(
            "📌",
            "现代 Linux 资源隔离与细粒度安全底座 (Resource & Security)",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. cgroups v2 (Control Groups 统一层次结构)：",
                "• 现代容器与 systemd 的物理引擎：挂载于 /sys/fs/cgroup/。Linux 内核把资源限制做到了极致简单——在 /sys/fs/cgroup 下 mkdir 一个文件夹，内核就自动为你创建一个新的控制组！",
                "• 核心资源控制器：",
                "  - cpu.max：硬性锁定进程组能消耗的最大 CPU 百分比（如限制后台编译最多吃 2 核，绝不卡顿前台游戏）。",
                "  - memory.max：内存硬上限。当该组内进程内存超标时，内核只在该组内定向触发局部 OOM，绝不牵连系统其他核心程序！",
                "  - io.max / io.weight：限制磁盘最大读写 IOPS 与吞吐量，防止后台下载把磁盘 I/O 挤爆导致系统假死。",
                "2. Linux Capabilities (告别危险的全局 root 时代)：",
                "• 传统模式的巨大隐患：以前程序需要绑定 80/443 端口，必须赋予 root 权限；一旦程序有漏洞，黑客直接拿到全系统最高控制权。",
                "• 权能细分革命：Linux 将 root 的无限神权拆分为几十个细粒度 Capability（如 CAP_NET_BIND_SERVICE 仅允许绑端口，CAP_SYS_PTRACE 仅允许调试跟踪）。应用程序只需单点赋权，从根本上锁死提权漏洞攻击面！"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "资源隔离对系统稳定性的战略价值 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 游戏与生产力的安全隔离：通过 cgroups，你可以让耗时数小时的后台编译、媒体转码或抓取任务在极低资源配额组中静默运行，前台 Steam 掌机游戏依然帧率如丝顺滑。",
                "• 无需依赖繁琐的虚拟化：这是纯粹的 Linux 内核原生原生机制，调度损耗为 0。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "查看当前系统支持的全部 cgroups v2 控制器清单", "cat /sys/fs/cgroup/cgroup.controllers", "确认内核已开启 cpu、memory、io、pids、cpuset 等全套资源控制能力。"),
            new(2, "查看当前 Shell 进程所归属的 cgroup 树状切片 (Slice)", "cat /proc/$$/cgroup", "观察该进程是属于 systemd 的 user.slice 还是 app.slice。"),
            new(3, "查看系统中通过 Capabilities 特权细分赋权的安全二进制文件", "getcap -r /usr/bin/ 2>/dev/null | head -n 15", "查看如 ping (仅拥有 cap_net_raw) 这种无需全局 root 也能工作的典型范例。"),
            new(4, "查看当前进程拥有的全部 Linux Capabilities 掩码集合", "getpcaps $$", "了解当前用户在内核权限模型中真正具备的能力边界。")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "结果验证与状态检查",
            "#238636",
            "#3FB950",
            new[]
            {
                "在终端执行 cat /sys/fs/cgroup/cgroup.controllers，如果返回 'cpuset cpu io memory hugetlb pids misc'，表明系统已经 100% 运行在最现代的 cgroups v2 统一层级架构下！"
            }
        )
    };
}
