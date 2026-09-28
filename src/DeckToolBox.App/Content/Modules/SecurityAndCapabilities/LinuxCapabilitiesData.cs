using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SecurityAndCapabilities;

public static class LinuxCapabilitiesData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "linux_capabilities",
        Title = "Linux 权能体系与最小特权模型 (POSIX Capabilities & setcap)",
        Subtitle = "安全体系与特权细分",
        CategoryName = "安全体系与特权细分",
        Badge = "安全底石",
        Overview = "彻底打破传统 UNIX 粗暴的 root/非 root 二元权限模型，深入解析 Linux POSIX Capabilities 权能分割机制、线程五大权能集与生产环境无 SUID 提权实践。",

        WhyCard = new InfoCardItem(
            "📌",
            "为什么传统 SUID-root 是万恶之源？Capabilities 如何实现特权细分？",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 传统 UNIX 模型的重大安全隐患 (All-or-Nothing)：",
                "• 在传统 UNIX/Linux 体系中，权限判断仅由有效用户 ID (EUID == 0) 决定。一个程序要么是普通用户（权限处处受限），要么是 root（拥有修改内核、格式化硬盘、窥探任意进程内存的绝对无上特权）。",
                "• 过去如果普通程序需要执行特权操作（例如 ping 需要发送 ICMP 原始套接字，或者 web 服务器需要绑定 80/443 端口），必须将二进制文件设置为 SUID-root (`chmod +s`)。一旦该程序存在缓冲区溢出漏洞，攻击者就能瞬间夺取系统最高 root 权限！",

                "2. Linux Capabilities 的细粒度权能解耦：",
                "• 从 Linux 2.2 开始，内核将传统超级用户的所有特权划分为近 40 种独立的权能 (Capabilities)：",
                "  - `CAP_NET_BIND_SERVICE`：仅允许绑定 < 1024 特权网络端口，剥夺其他任何破坏能力。",
                "  - `CAP_NET_RAW`：仅允许构造 RAW / PACKET 原始套接字（给 ping 或 wireshark 使用）。",
                "  - `CAP_SYS_NICE`：仅允许调整进程 nice 优先级与实时调度策略 (SCHED_FIFO / SCHED_RR)。在掌机上，Gamescope 和音频合成服务正是依靠该权能获得纳秒级抗卡顿调度，无需以 root 身份运行！",
                "  - `CAP_SYS_PTRACE`：允许通过 ptrace 系统调用调试或追踪其他进程（gdb / strace 核心权能）。",
                "  - `CAP_SYS_ADMIN`：被称为“新一代 root”，涵盖挂载文件系统、配置网络命名空间等广泛底层操作，容器运行时重点防护对象。",

                "3. 进程的五大权能集合 (Capability Sets)：",
                "• 内核在 `task_struct` 中为每个线程维护了 5 组 64-bit 位图：",
                "  - **Permitted (P)**：该进程能使用的最大权能超集。",
                "  - **Effective (E)**：内核在做权限检查时实际判定的权能生效位。",
                "  - **Inheritable (I)**：通过 execve 系统调用执行新程序时可以继承的权能位。",
                "  - **Bounding (B)**：权能边界集，限制进程及其子进程能获得的权能上限（只能收紧，不可扩张）。",
                "  - **Ambient (A)**：Linux 4.3 引入，允许非 root 用户启动的子进程无缝继承已授予的特权，是现代非特权容器的关键支撑。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "权能检查与赋予的风险边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 探测权能绝对安全：使用 `getpcaps`、`getcap` 或解析 `/proc/<PID>/status` 中的 Cap 位图完全是只读操作，不会改变任何系统状态。",
                "• 谨慎向二进制赋予 Capabilities：通过 `setcap` 赋予二进制文件文件权能（存储在文件系统的 `security.capability` 扩展属性 xattr 中），相当于部分放权。绝不可向解释器（如 bash、python）赋予特权权能，否则任何执行该解释器的脚本都能继承该特权！",
                "• 清除二进制权能即时失效：通过 `setcap -r <file>` 可随时擦除文件权能，立即恢复普通非特权执行状态。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看当前 Shell 进程所持有的五大 Capabilities 权能位图",
                "cat /proc/$$/status | grep -E \"^Cap\"",
                "查看 CapInh (继承), CapPrm (允许), CapEff (生效), CapBnd (边界), CapAmb (环境) 的 16 进制 64 位掩码。"
            ),
            new(
                2,
                "使用系统工具将 16 进制 Cap 掩码解码为人类可读权能名",
                "capsh --decode=$(grep CapEff /proc/$$/status | awk '{print $2}')",
                "调用 libcap 工具 capsh 实时解码掩码，直观呈现当前进程实际启用的权限列表。"
            ),
            new(
                3,
                "扫描全盘所有被赋予文件扩展权能 (File Capabilities) 的关键系统程序",
                "getcap -r /usr/bin 2>/dev/null | head -n 25",
                "审计掌机或宿主机系统，查看例如 ping (cap_net_raw)、systemd-resolve (cap_net_bind_service) 等如何安全剥离 SUID。"
            ),
            new(
                4,
                "使用 getpcaps 查看任意目标进程（如 Gamescope 或 Steam）的实时权能",
                "getpcaps $(pgrep -f gamescope | head -n 1)",
                "直接探测运行中的掌机合成器进程，验证其是否成功以普通用户身份持有 cap_sys_nice 权限。"
            ),
            new(
                5,
                "实验安全非 root 端口绑定能力（语法演示与撤销）",
                "echo 'setcap cap_net_bind_service=+ep <目标二进制> 可免 root 绑定 80 端口；setcap -r <目标二进制> 可随时移除'",
                "学习 setcap 语法规范：+ep 代表将该权能加入 Effective 和 Permitted 集合，实现最小特权赋能。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "权能验证与安全基线检查 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 非特权基线验证：普通用户执行 `getpcaps $$` 输出应为空（即没有任何多余特权），符合最小权限原则。",
                "2. SUID 文件替代验证：执行 `ls -la /usr/bin/ping`，现代安全系统上其权限位应为正常的 `-rwxr-xr-x`（非 `-rwsr-xr-x`），且 `getcap /usr/bin/ping` 输出明确包含 `cap_net_raw+ep`，标志着系统已成功脱离脆弱的 SUID 时代。"
            }
        )
    };
}
