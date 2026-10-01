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
        Overview = "/proc 是内核提供的虚拟文件系统：不占磁盘空间，每次读取都由内核即时生成内容。每个进程在 /proc/<PID>/ 下有一个目录，/proc/sys/ 下是可以在运行时调整的内核参数。ps、top、free 等工具的数据都来自这里。",

        Why =
        [
            "1. 虚拟文件：`/proc` 下的文件大小显示为 0，读取时内核才生成内容，所以数据总是当前时刻的。",
            "2. 每个进程的目录 `/proc/<PID>/`：",
            "- `cmdline`：启动时的完整命令行（参数之间以空字符分隔）。",
            "- `environ`：进程启动时继承的环境变量，可用来确认启动选项是否生效。",
            "- `fd/`：进程当前打开的文件、套接字和管道，每个是一个符号链接。",
            "- `status`、`maps`、`smaps`：状态摘要、内存映射和各段内存的实际占用。",
            "• `/proc/self` 始终指向读取它的那个进程自己；`$$` 是当前 shell 的 PID。",
            "3. 系统级信息：`/proc/cpuinfo`、`/proc/meminfo`、`/proc/loadavg`、`/proc/cmdline` 等。",
            "4. 内核参数 `/proc/sys/`：可读取，root 可写入以在运行时调整，重启后恢复默认；要永久生效需在 `/etc/sysctl.d/` 下写配置文件。`sysctl` 命令是读写它们的标准方式。",
            "5. 资源压力 (PSI)：`/proc/pressure/{cpu,memory,io}` 统计任务因等待 CPU、内存或磁盘而停顿的时间比例。`some` 表示至少有一个任务在等，`full` 表示所有任务都在等；比 loadavg 更直接地反映「卡不卡」。",
            "6. `/proc/cmdline` 是引导器传给内核的启动参数（SteamOS 上有 `amdgpu.*` 显卡参数、`steamos.efi=PARTUUID=...` 等）；`/proc/net/dev` 是各网卡开机以来的收发字节数、包数、错误与丢包计数。",
            "7. `vm.max_map_count`：单个进程最多能有多少个内存映射区域。内核默认值是 65530，部分大型游戏（尤其经 Proton 运行时）会超过这个数而崩溃。SteamOS 在 `/usr/lib/sysctl.d/30-vm.conf` 中把它设为 2147483642；Fedora、Ubuntu、Arch 近年也把默认值调大了。",
            "8. `/proc/interrupts` 记录每个硬件中断在各 CPU 核心上被处理的次数；`/proc/diskstats` 记录每个块设备开机以来读写的次数、扇区数（每扇区 512 字节）与耗时。",
        ],

        Consequences =
        [
            "• 读取 `/proc` 对系统没有影响。",
            "• 其他用户进程的 `environ`、`fd` 等敏感信息只有 root 能读取；你只能看自己的进程。",
            "• 修改 `/proc/sys` 的参数立即影响整个系统，改前记下原值，不确定的参数不要改。",
        ],

        Steps =
        [
            Step.Read(
                "查看一个进程的环境变量",
                "cat /proc/$$/environ | tr '\\0' '\\n' | head -n 20",
                "`$$` 是当前 shell 的 PID；`environ` 中各变量以空字符分隔，`tr` 转成换行。").Then("每行一个 `名称=值`，如 `HOME=/home/deck`、`LANG=...`，显示前 20 个。"),
            Step.Read(
                "查看一个进程打开的文件",
                "ls -l /proc/$$/fd",
                "`fd/` 目录下每个数字是一个打开的文件描述符。").Then("列出 `0 -> /dev/pts/N`、`1 -> ...`、`2 -> ...`（标准输入、输出、错误），以及 shell 打开的其他文件；套接字显示为 `socket:[编号]`。"),
            Step.Read(
                "查看 CPU 型号与当前频率",
                "cat /proc/cpuinfo | grep -E \"(model name|cpu MHz|cache size)\" | head -n 12",
                "Steam Deck 的 APU 为 AMD Zen 2 架构，4 核 8 线程。").Then("重复多组 `model name : AMD Custom APU 0932`、`cpu MHz : 2424.000`、`cache size : 512 KB`，每个逻辑核心一组。"),
            Step.Read(
                "查看内存的详细分项",
                "cat /proc/meminfo | head -n 16",
                "`MemAvailable` 才是可以给新程序用的内存估计值。").Then("16 行「名称: 数值 kB」，如 `MemTotal`、`MemFree`、`MemAvailable`、`Buffers`、`Cached`、`SwapCached`、`Active`、`Inactive`。"),
            Step.Read(
                "查看 CPU 与内存资源阻塞压力 (PSI)",
                "cat /proc/pressure/cpu; echo ---; cat /proc/pressure/memory",
                "`avg10`/`avg60`/`avg300` 是最近 10 秒、1 分钟、5 分钟内停顿时间的百分比，`total` 是累计微秒数。").Then("每个文件两行，如 `some avg10=12.54 avg60=2.88 avg300=1.94 total=...` 与 `full avg10=0.00 ...`，中间用 `---` 隔开。游戏卡顿时 `some avg10` 明显升高。"),
            Step.Read(
                "查看交换倾向参数 swappiness",
                "cat /proc/sys/vm/swappiness",
                "取值 0~200；越大内核越倾向于把不常用的内存页换出到交换空间。").Then("输出一个数字，SteamOS 上为 `60`。"),
            Step.Read(
                "查看内核启动参数",
                "cat /proc/cmdline | tr ' ' '\\n' | grep -E 'vmlinuz|amdgpu|steamos|fsck'",
                "`tr` 把空格换成换行，每个参数一行，再挑出与内核镜像、显卡、引导分区、开机自检有关的几项。").Then("几行参数，如 `BOOT_IMAGE=/boot/vmlinuz-linux-neptune-616`（Valve 为 Deck 编译的内核）、`amdgpu.lockup_timeout=5000,10000,10000,5000`、`fsck.mode=auto`、`steamos.efi=PARTUUID=...`。"),
            Step.Read(
                "读取网络接口流量与丢包统计",
                "cat /proc/net/dev | grep -vE 'lo:' | head -n 10",
                "前两行是表头；每个网卡一行，左半部分是接收 (Receive)，右半部分是发送 (Transmit)。").Then("两行表头之后，`wlan0:` 等网卡各一行，依次是接收的 bytes、packets、errs、drop……和发送的同名各列；`errs`、`drop` 持续增长说明链路有问题。"),
            Step.Read(
                "查看虚拟内存映射上限 max_map_count",
                "cat /proc/sys/vm/max_map_count",
                "想知道某个进程用了多少：`wc -l /proc/<PID>/maps`。").Then("输出 `2147483642`。在 Distrobox 容器或其他发行版上执行，可能看到 `65530` 或 `1048576`。"),
            Step.Read(
                "查看内核文件句柄分配与最大上限",
                "cat /proc/sys/fs/file-nr",
                "整个系统范围的统计；单个进程的上限另见 `ulimit -n`。").Then("一行三个数，如 `14845  0  9223372036854775807`：已分配的文件句柄数、已分配但空闲的数量（新内核恒为 0）、系统上限（这里实际上不设限）。"),
            Step.Read(
                "查看硬件中断在 CPU 上的分布",
                "grep -E 'amdgpu|nvme|xhci|mmc' /proc/interrupts",
                "挑出显卡 (`amdgpu`)、固态硬盘 (`nvme`)、USB 控制器 (`xhci_hcd`) 与 SD 卡读卡器 (`mmc0`)。").Then("十几行，每行开头是中断号，接着 8 列是 8 个逻辑 CPU 各自处理它的累计次数，最后是设备名（如 `nvme0q1`、`xhci_hcd`、`amdgpu`）。NVMe 的 `nvme0q1`~`q8` 八个队列各由一个核心处理，所以计数分散在不同的列。"),
            Step.Read(
                "查看物理磁盘 IO 扇区读写统计",
                "cat /proc/diskstats | grep -E 'nvme0n1 |mmcblk0 ' | awk '{print $3 \" 读扇区:\" $6 \" 写扇区:\" $10}'",
                "第 3 列是设备名，第 6、10 列是读、写的扇区数，乘以 512 就是字节数。").Then("每个设备一行，如 `nvme0n1 读扇区:2784002687 写扇区:1345084642`（约读 1.3 TB、写 640 GB）；没插 SD 卡则没有 `mmcblk0` 行。"),
        ],

        Verify =
        [
            "1. `cat /proc/$$/cmdline | tr '\\0' ' '` 输出 `bash` 或当前 shell 的启动命令。",
            "2. `sysctl vm.swappiness` 与 `cat /proc/sys/vm/swappiness` 输出相同的值。",
        ],
    };
}
