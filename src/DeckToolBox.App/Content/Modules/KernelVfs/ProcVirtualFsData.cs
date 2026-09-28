using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.KernelVfs;

public static class ProcVirtualFsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "procfs",
        Title = "/proc 进程与内核参数",
        Keywords = "/proc · sysctl",
        Level = ModuleLevel.Advanced,
        Overview = "深入剖析 /proc 内存伪文件系统，通过纯文件读写直接洞察进程内部运行态、文件描述符、内存映射与内核全局参数。",

        Why =
        [
            "1. 纯内存虚拟文件系统（零磁盘开销）：/proc 不占用任何闪存或磁盘空间。你看到的每一个文件，本质上都是一段指向 Linux 内核内部数据结构的 Hook。读取它就是直接向内核态索取瞬时数据。",
            "2. 每一个进程的“全面体检报告”：/proc/<PID>/ 目录下存放了对应进程的所有内幕：",
            "• /proc/<PID>/cmdline：进程实际启动的完整命令行参数（以 \\0 字符分隔）。",
            "• /proc/<PID>/environ：注入到该进程的真实环境变量快照（验证启动参数是否生效的最佳证据）。",
            "• /proc/<PID>/fd/：该进程当前持有并打开的所有文件描述符、网络 Socket 套接字与管道（排查句柄泄漏的终极利器）。",
            "• /proc/<PID>/maps & smaps：进程虚拟内存空间分布图，能精确看出每一块动态链接库、堆栈、匿名内存占用了多少物理内存 (PSS / RSS)。",
            "3. 内核运行时动态调优中枢 (/proc/sys/)：无需重新编译内核，通过 echo 写入即可实时改变内核行为（如内存淘汰激进度、网络连接追踪表大小等）。",
        ],

        Consequences =
        [
            "• 纯只读探测绝对安全：在 /proc 下执行 cat、ls、grep 等命令纯属读取内核统计数据，完全不会对系统产生任何副作用或性能波动。",
            "• /proc/sys 写入即时生效但重启复原：直接向 /proc/sys 写入的参数在当前系统立即起效，但在设备重启后会自动还原（若想永久生效，需要在 /etc/sysctl.d/ 下编写 .conf 规则）。",
        ],

        Steps =
        [
            Step.Read(
                "查看指定进程启动时的完整真实环境变量快照",
                "cat /proc/$$/environ | tr '\\0' '\\n' | head -n 20",
                "`$$` 是当前 shell 的 PID；`environ` 中各变量以空字符分隔，`tr` 转成换行。").Then("每行一个 `名称=值`，如 `HOME=/home/deck`、`LANG=...`，显示前 20 个。"),
            Step.Read(
                "排查进程当前打开的所有文件描述符与网络链接",
                "ls -l /proc/$$/fd",
                "`fd/` 目录下每个数字是一个打开的文件描述符。").Then("列出 `0 -> /dev/pts/N`、`1 -> ...`、`2 -> ...`（标准输入、输出、错误），以及 shell 打开的其他文件；套接字显示为 `socket:[编号]`。"),
            Step.Read(
                "查看 CPU 核心微架构与当前运行主频",
                "cat /proc/cpuinfo | grep -E \"(model name|cpu MHz|cache size)\" | head -n 12",
                "Steam Deck 的 APU 为 AMD Zen 2 架构，4 核 8 线程。").Then("重复多组 `model name : AMD Custom APU 0932`、`cpu MHz : 2424.000`、`cache size : 512 KB`，每个逻辑核心一组。"),
            Step.Read(
                "查看物理内存各细分段（可用、脏页、活跃缓存）全景",
                "cat /proc/meminfo | head -n 16",
                "`MemAvailable` 才是可以给新程序用的内存估计值。").Then("16 行「名称: 数值 kB」，如 `MemTotal`、`MemFree`、`MemAvailable`、`Buffers`、`Cached`、`SwapCached`、`Active`、`Inactive`。"),
            Step.Read(
                "查看与调节内核 Swap 换页积极度 (Swappiness)",
                "cat /proc/sys/vm/swappiness",
                "取值 0~200；越大内核越倾向于把不常用的内存页换出到交换空间。").Then("输出一个数字，SteamOS 上为 `60`。"),
        ],

        Verify =
        [
            "当在终端执行 cat /proc/$$/cmdline 时，若当前为 bash，你会看到以空字符结尾的 'bash' 字样；在 /proc/meminfo 中，只要 MemAvailable 维持在健康水平，系统就绝无卡顿风险。",
        ],
    };
}
