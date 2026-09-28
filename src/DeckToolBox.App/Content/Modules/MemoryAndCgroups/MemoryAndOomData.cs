using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.MemoryAndCgroups;

public static class MemoryAndOomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "memory-oom",
        Title = "内存、ZRAM 与 OOM",
        Keywords = "free · zram · earlyoom",
        Level = ModuleLevel.Intermediate,
        Overview = "破除“空闲内存过少”认知误区，深入虚拟内存、页面缓存、ZRAM 内存压缩交换与内核 OOM Killer 杀手保活机制。",

        Why =
        [
            "1. 破除误区：为什么 Linux 总是把空闲内存“吃满”？",
            "• 核心哲学：未使用的空闲 RAM 就是对硬件算力和电量的纯粹浪费！Linux 内核极其智能，会把所有未被进程独占的闲置内存全额挪用为 Page Cache (页面缓存) 预热磁盘数据。",
            "• 当有前台游戏申请新内存时，内核在纳秒级直接释放这些只读缓存，对程序运行 0 阻塞！因此衡量内存是否充裕必须看【available】，绝不能看【free】！",
            "2. 内存指标四兄弟：",
            "• VIRT (虚拟内存总量)：进程申请的虚地址空间总和（很多大游戏会预申请几十 GB，但实际不占物理内存）。",
            "• RES / RSS (常驻物理内存)：当前真正霸占着机器硬件 RAM 颗粒的真实内存量（核心排查指标）。",
            "• SHR (共享内存)：多个进程共同引用的公共库（如 glibc、图形驱动共享段）。",
            "3. ZRAM (掌机内存的黑科技黑马)：",
            "• 传统 NVMe Swap 交换文件延迟高且严重磨损闪存颗粒。ZRAM 直接在内存中开辟一块虚拟压缩块设备，以 LZ4/ZSTD 算法提供 2:1 的压缩比。用极其微小的 CPU 算力，让 16GB 物理内存等效扩容至 22GB~24GB！",
            "4. OOM Killer (内存溢出绝杀机制)：",
            "• 当物理内存与 ZRAM 彻底耗尽时，内核激活 out_of_memory() 算法，遍历进程根据占用量与优先级计算坏分数 (oom_score)。挑出分数最高的最臃肿进程，直接发送 SIGKILL 强杀，以牺牲单个进程换取操作系统不至于死机卡死！",
        ],

        Consequences =
        [
            "• 绝不要盲目 echo 3 > /proc/sys/vm/drop_caches：强行清空 Page Cache 毫无好处，只会导致下次读取应用时重新忍受缓慢的磁盘物理 I/O！",
            "• 游戏突然无故闪退无任何弹窗？90% 的可能是触发了内核 OOM Killer 或 SteamOS 的 earlyoom 守护，通过查看 dmesg 即可捕获被杀现场记录。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统物理内存、缓存与真实可用量 (核心看板)",
                "free -h",
                "重点观察 available 列，此值才是系统当前真实能拿出的最大空闲内存。"),
            Step.Read(
                "查看系统 ZRAM 内存压缩设备的实际运行情况与压缩比",
                "zramctl 2>/dev/null || swapon --show",
                "查看真实使用了多少物理内存（DATA）存储了多少解压后的交换数据（DISKSIZE）。"),
            Step.Read(
                "查看当前系统中最容易被 OOM Killer 选为牺牲者的进程坏分数排行榜",
                "ps -eo pid,comm,oom_score --sort=-oom_score | head -n 15",
                "分数越接近 1000 的进程，一旦内存爆仓，第一个就会被内核枪决。"),
            Step.Read(
                "排查最近系统是否发生过 OOM 杀进程惨案",
                "dmesg -T 2>/dev/null | grep -i -E \"(oom-killer|killed process)\" | tail -n 10 || echo \"内核记录显示：无 OOM 发生\"",
                "检查历史闪退是否是由内存耗尽导致的。"),
        ],

        Verify =
        [
            "执行 free -h 与 zramctl，你会看到当前设备的 ZRAM 已经平稳挂载，即使运行重度 Mod 导致内存飙升，系统依然能依靠压缩缓冲保持桌面流畅不卡死。",
        ],
    };
}
