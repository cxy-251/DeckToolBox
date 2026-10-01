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
        Overview = "系统变慢时，只看 CPU 占用率往往找不到原因。本模块介绍一套分析思路（USE 方法），以及三类工具：/proc/loadavg 等基础指标、跟踪系统调用的 strace、读取 CPU 硬件计数器的 perf。",

        Why =
        [
            "1. USE 方法（Brendan Gregg 提出）：对每种资源（CPU、内存、磁盘、网络）依次检查：",
            "- 使用率 (Utilization)：资源忙碌的时间比例。",
            "- 饱和度 (Saturation)：排队等待该资源的工作量。",
            "- 错误 (Errors)：出错的次数。",
            "2. 平均负载 (load average)：",
            "• Linux 的平均负载不仅统计正在运行和等待 CPU 的任务，还包括处于 D 状态（不可中断地等待 IO）的任务。",
            "• 所以 CPU 占用很低、负载却很高时，瓶颈通常在磁盘，例如向很慢的 SD 卡大量写入。",
            "3. strace：通过 `ptrace` 拦截进程的每一次系统调用（打开文件、连接网络、等待锁等），记录参数和返回值。适合排查「程序启动失败但没有提示」「在找哪个配置文件」「卡在哪个调用上」。",
            "4. perf 与硬件计数器：CPU 内置性能监控单元 (PMU)，能统计执行了多少指令、消耗了多少时钟周期、缓存未命中多少次。`perf stat` 汇总这些数据：",
            "- IPC（每周期指令数）：数值低（如 0.5 以下）通常说明 CPU 大部分时间在等内存数据；数值高说明计算密集且顺畅。",
            "5. eBPF：在内核中安全地运行小程序，统计各种事件而不必修改内核。加载前由内核的验证器检查，但仍需要 root 权限；bpftrace 等前端工具 SteamOS 没有自带。",
            "6. SteamOS 的调度器参数：开机时 `steamos-cfs-debugfs-tunings.service` 运行 `/usr/lib/steamos/steamos-cfs-debugfs-settings`，向 `/sys/kernel/debug/sched/` 写入一组 CFS 参数（`latency_ns`=3 ms、`min_granularity_ns`=0.3 ms、`wakeup_granularity_ns`、`migration_cost_ns`、`nr_migrate`），用更短的时间片换取更快的响应。注意：内核 6.6 起默认调度器换成了 EEVDF，前三个参数已不存在，脚本会跳过不存在的文件，在当前内核上实际生效的只有 `migration_cost_ns` 与 `nr_migrate`。",
        ],

        Consequences =
        [
            "• 读取 `/proc/loadavg` 和 `perf stat` 的开销很小。",
            "• strace 会显著拖慢被追踪的程序（每次系统调用都要暂停它），只在排查时对单个程序短时间使用，不要用于正在玩的游戏。",
            "• SteamOS 默认 `perf_event_paranoid=2`：普通用户只能统计自己进程在用户态的事件；系统级分析需要 sudo。",
        ],

        Steps =
        [
            Step.Read(
                "查看平均负载",
                "cat /proc/loadavg",
                "前三个数是 1、5、15 分钟平均负载；`2/1280` 为正在运行 / 总线程数；最后是最近创建的进程 PID。负载除以 CPU 线程数（Deck 为 8）小于 1 表示不排队。").Then("输出一行，如 `1.20 0.95 0.80 2/1280 865432`。"),
            Step.Read(
                "统计一个命令的系统调用",
                "strace -c ls -la /usr/bin > /dev/null",
                "`-c` 只统计不逐条打印；统计结果写在标准错误，所以不会被 `> /dev/null` 丢掉。").Then("`ls` 的正常输出被丢弃；strace 在结束时输出一张表：各系统调用的耗时占比、次数、出错次数和名称，如 `getdents64`、`openat`、`statx`。"),
            Step.Read(
                "跟踪一个命令打开了哪些文件",
                "strace -e trace=openat ls /tmp 2>&1 | head -n 20",
                "`-e trace=openat` 只看打开文件的调用。排查「程序找不到配置文件或库」时最有用。").Then("每行一个 `openat(...)` 调用及其返回值，能看到 `ls` 依次打开了哪些库文件（`libc.so.6` 等）和目录；失败的调用显示 `= -1 ENOENT`。"),
            Step.Read(
                "用硬件计数器统计一个命令",
                "perf stat ls -la /usr/lib 2>&1 | tail -n 20",
                "SteamOS 默认只允许普通用户统计自己进程在用户态的事件（显示为 `:u`）。").Then("命令结束后输出计数表：`task-clock`、`context-switches`、`page-faults`、`cycles`、`instructions`，以及 `insn per cycle`（IPC）和总耗时。"),
            Step.Root(
                "列出内核的追踪点分类",
                "sudo ls /sys/kernel/tracing/events/ | head -n 25",
                "每个目录下是内核中该子系统预置的追踪点。").Then("输入 sudo 密码后列出前 25 个子系统目录名，如 `block`、`sched`、`syscalls`、`net`。"),
            Step.Read(
                "查看内核调度微调配置脚本",
                "cat /usr/lib/steamos/steamos-cfs-debugfs-settings",
                "脚本很短，可以直接阅读；`set_sched_dbgfs` 函数在目标文件不存在时只记一个错误，不会中断。").Then("输出整个 bash 脚本，末尾是五行 `set_sched_dbgfs \"latency_ns\" \"3000000\"`、`\"min_granularity_ns\" \"300000\"`、`\"wakeup_granularity_ns\" \"500000\"`、`\"migration_cost_ns\" \"50000\"`、`\"nr_migrate\" \"128\"`。"),
        ],

        Verify =
        [
            "1. 用 `nproc` 查看 CPU 线程数（Deck 为 8），用平均负载除以它：小于 1 表示没有排队，持续大于 2 表示明显过载。",
            "2. `perf stat` 输出中能看到 `insn per cycle`，即 IPC。",
        ],
    };
}
