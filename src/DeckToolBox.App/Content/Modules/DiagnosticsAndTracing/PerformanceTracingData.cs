using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.DiagnosticsAndTracing;

public static class PerformanceTracingData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "perf-tracing",
        Title = "性能观测与追踪",
        Keywords = "loadavg · strace · perf",
        Level = ModuleLevel.Advanced,
        Overview = "掌握 Brendan Gregg 经典系统性能观测方法论 (USE)，深入系统调用拦截 (strace)、硬件性能计数器采样 (perf) 与现代 eBPF 零开销内核态观测革命。",

        Why =
        [
            "1. Brendan Gregg USE 观测法则与 Load Average 的深层真相：",
            "• 初学者往往只看 Top 的 CPU% 占用，却不知道卡顿往往源于内存带宽枯竭、IO 等待或锁竞争 (Lock Contention)。系统级排查应遵循 USE 原则：使用率 (Utilization)、饱和度 (Saturation)、错误数 (Errors)。",
            "• **`/proc/loadavg` 的底层秘密**：在大多数 UNIX 传统系统中，平均负载只统计就绪运行队列中的进程 (`TASK_RUNNING`)。但在 Linux 中，内核维护者做出了一项关键决定：**将处于不可中断睡眠状态 (`TASK_UNINTERRUPTIBLE`, 即 D 状态) 的进程也一并计入 Load！**",
            "• 这意味着即使 CPU 使用率只有 1%，只要磁盘 IO 严重堵塞（例如低速 SD 卡写入卡死），系统的 Load Average 也会飙升到几十甚至上百！",
            "2. 进程行为黑盒透视镜：系统调用拦截 (strace)：",
            "• 基于 Linux 内核的 `ptrace(PTRACE_SYSCALL)` 机制。当被追踪进程每次发起系统调用（如 `openat` 打开文件、`connect` 建立连接、`futex` 锁等待）以及从内核态返回时，内核都会暂停该进程并唤醒 strace 记录其入参和返回值。",
            "• 这是排查“程序闪退却没有任何日志输出”、“找不到到底缺少了哪个动态链接库 .so”、“卡死在哪个网络读写”的终极利器！",
            "3. 芯片级微架构度量：硬件性能计数器 (PMU) 与 perf：",
            "• CPU 硬件内置了 PMU (Performance Monitoring Unit) 硬件计数器。`perf` 工具可直接读取芯片底层的微指令执行情况：",
            "  - **Cycles (时钟周期)** 与 **Instructions (执行指令数)**：算出 **IPC (Instructions Per Cycle)**。若 IPC < 0.5，说明 CPU 绝大部分时间在空转等待内存数据返回（Cache Miss）；若 IPC > 1.5，说明代码在密集高效运算。",
            "  - **Branch-misses (分支预测失败)** 与 **Cache-misses (L1/L2/L3 缓存未命中)**：精准定位代码级性能退化根源，更是现代性能火焰图 (FlameGraph) 的原生数据来源。",
            "4. 新一代内核观测革命：eBPF (Extended Berkeley Packet Filter)：",
            "• Linux 内核内置的安全沙箱 JIT 虚拟机。无需修改内核源码或重启系统，即可将追踪探针 (kprobe 内核函数、uprobe 用户函数、tracepoint 静态打点) 注入内核，直接在内核态完成聚合运算，带来几乎为 0 开销的生产级实时可观测性。",
        ],

        Consequences =
        [
            "• 静态度量与 perf 采样完全安全：读取 `/proc/loadavg` 或使用 `perf stat` / `perf record` 采样基于硬件定时中断，系统开销极低（通常 < 2% CPU），对系统毫无破坏性。",
            "• 严禁对高频业务无节制使用 strace：由于 `ptrace` 机制每次系统调用都要引发 2 次上下文切换并暂停进程，`strace` 会导致被测程序运行速度衰减 10~50 倍！仅应在定位瞬时闪退报错、排查缺失文件时针对性使用，切勿挂在正常前台高刷游戏中。",
            "• eBPF 的安全性：内核的验证器 (verifier) 在加载前检查 eBPF 程序，拒绝可能死循环或越界访问的代码，因此它比传统内核模块安全得多；但加载 eBPF 程序仍需要 root 或相应权能。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统 1/5/15 分钟平均负载与内核运行队列状态",
                "cat /proc/loadavg",
                "前三个数值代表过去 1、5、15 分钟的平均负载；第四段形如 2/840 代表当前活跃调度线程数 / 线程总数；最后是最新创建的进程 PID。"),
            Step.Read(
                "统计分析任意命令所耗费的系统调用类型与耗时分布 (strace -c)",
                "strace -c ls -la /usr/bin > /dev/null",
                "使用 -c 参数汇总系统调用次数、错误次数 (errors)、耗时百分比以及系统调用名称（如 openat, mmap, getdents64）。"),
            Step.Read(
                "精准追踪进程的文件寻址路径与动态链接库加载细节",
                "strace -e trace=openat ls /tmp 2>&1 | head -n 20",
                "使用 -e trace=openat 仅过滤文件打开系统调用，快速定位软件启动时尝试从哪些目录搜寻配置文件或 .so 库。"),
            Step.Read(
                "测量 CPU 硬件微架构性能指标与 IPC 指令吞吐率 (perf stat)",
                "perf stat ls -la /usr/lib 2>&1 | tail -n 20",
                "读取 PMU 硬件性能计数器：查看 task-clock, context-switches, cpu-migrations, page-faults, cycles, instructions 以及 IPC 数值。"),
            Step.Root(
                "检索内核静态追踪打点分类总览 (Tracepoints)",
                "sudo ls /sys/kernel/tracing/events/ | head -n 25",
                "查看内核内建的静态追踪子系统（如 sched 进程调度、syscalls 系统调用、block 磁盘块设备 IO、net 网络数据包打点）。"),
        ],

        Verify =
        [
            "1. 负载健康度评估：将 `/proc/loadavg` 的数值除以物理 CPU 逻辑核心数（可通过 `nproc` 查看，如掌机为 8 线程）。若比值 <= 1.0，代表系统吞吐健康无排队；若比值 >= 2.0，代表有大量就绪任务在排队等待 CPU 或磁盘响应。",
            "2. IPC 算力饱和度评估：在 `perf stat` 输出中，若 `insn per cycle` (IPC) 超过 1.0~2.0，代表流水线运转极其充实；若 IPC 仅为 0.2~0.4，表明代码发生严重的内存等待或缓存颠簸。",
        ],
    };
}
