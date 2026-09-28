using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.MemoryAndCgroups;

public static class MemoryAndOomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "memory-oom",
        Title = "内存、ZRAM 与 OOM",
        Keywords = "free · zramctl · swap · earlyoom",
        Level = ModuleLevel.Intermediate,
        Overview = "「内存快满了」往往是误读：Linux 会把空闲内存拿来做磁盘缓存。真正该看的是 available、交换空间的使用量，以及有没有进程因内存不足被杀掉。",

        Why =
        [
            "1. 空闲内存会被用作缓存：",
            "• 内核把读过的文件内容留在内存里（Page Cache），下次读取无需访问硬盘。程序需要内存时，干净的缓存可以立即丢弃让出。",
            "• 所以 `free` 的 `free` 列很小是正常的，判断内存是否紧张要看 `available` 列：它估算的是不换出任何东西就能提供给新程序的内存。",
            "2. 进程内存的几个指标：",
            "- `VIRT`：进程申请的虚拟地址空间，可能远大于实际使用，参考价值低。",
            "- `RES` / `RSS`：实际占用的物理内存，排查「谁占内存」主要看它。",
            "- `SHR`：其中与其他进程共享的部分（共享库等）。",
            "3. ZRAM 压缩交换：",
            "• ZRAM 在内存里创建一个压缩块设备当作交换分区。不常用的内存页被压缩后放进去，SteamOS 使用 zstd 算法，常见压缩比 2~3 倍。",
            "• 比写入 SSD 的交换文件快得多，也不磨损闪存。SteamOS 同时还有一个低优先级的 `/home/swapfile`，ZRAM 用满后才会用到它。",
            "4. 内存耗尽时谁来收拾：",
            "• 内核 OOM Killer：真正无内存可用时，按 `oom_score` 选一个进程发送 SIGKILL。",
            "• SteamOS 预装了 **earlyoom** 服务：在可用内存和交换空间都低于阈值时提前介入，杀掉占用最多的进程，避免系统先卡死很久。",
            "• 所以「游戏突然消失、没有任何报错」时，应先检查是否是内存不足导致的强制结束。",
        ],

        Consequences =
        [
            "• 本模块的命令全部只读。",
            "• 不要用 `echo 3 > /proc/sys/vm/drop_caches` 「清理内存」：它只是丢掉缓存，之后读取文件变慢，并不会让程序多出可用内存。",
            "• 交换空间使用率高本身不是问题；如果 `available` 同时也很低，并且系统频繁卡顿，才说明内存确实不够，应减少同时运行的程序（例如关掉后台浏览器）。",
        ],

        Steps =
        [
            Step.Read(
                "查看内存总览",
                "free -h",
                "重点看 `available`；`Swap` 一行显示 ZRAM 与交换文件的合计使用量。"),
            Step.Read(
                "查看 ZRAM 的压缩效果",
                "zramctl",
                "`DATA` 是存入的原始数据量，`COMPR` 是压缩后大小，`TOTAL` 是实际占用内存。DATA ÷ TOTAL 即压缩比。"),
            Step.Read(
                "查看所有交换设备及优先级",
                "swapon --show",
                "`PRIO` 越大越优先使用，ZRAM 的优先级高于交换文件。"),
            Step.Read(
                "按实际内存占用列出前 15 个进程",
                "ps -eo pid,rss,comm --sort=-rss | head -n 16 | awk 'NR==1{print; next} {printf \"%-8s %8.0f MB  %s\\n\", $1, $2/1024, $3}'",
                "`rss` 单位是 KB，这里换算成 MB。交互式查看可以用 `htop`，按 `F6` 选择排序列。"),
            Step.Read(
                "查看最可能被 OOM 选中的进程",
                "for p in /proc/[0-9]*; do s=$(cat $p/oom_score 2>/dev/null) && echo \"$s ${p#/proc/} $(cat $p/comm 2>/dev/null)\"; done | sort -rn | head -n 10",
                "三列依次为 oom_score、PID、进程名，分数越高越先被杀。直接读取每个进程的 `/proc/<PID>/oom_score`。"),
            Step.Read(
                "查看 earlyoom 是否杀过进程",
                "journalctl -u earlyoom --since '7 days ago' --no-pager | grep -iE 'sending|killing' | tail -n 10",
                "有输出说明近期发生过内存不足导致的强制结束。"),
            Step.Read(
                "查看内核 OOM Killer 的记录",
                "journalctl -k --since '7 days ago' --no-pager | grep -iE 'out of memory|killed process' | tail -n 10",
                "SteamOS 限制普通用户直接运行 `dmesg`，用 `journalctl -k` 查看内核日志即可。"),
        ],

        Verify =
        [
            "1. `free -h` 中 `available` 明显大于 `free`，说明大部分「已用」内存其实是可释放的缓存。",
            "2. `zramctl` 的 DATA 是 COMPR 的 2 倍以上，说明压缩交换正在有效「扩容」内存。",
            "3. 两条日志查询都没有输出，说明近期没有进程因内存不足被结束。",
        ],
    };
}
