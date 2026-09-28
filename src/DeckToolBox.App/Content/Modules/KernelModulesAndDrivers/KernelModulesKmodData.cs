using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.KernelModulesAndDrivers;

public static class KernelModulesKmodData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "kernel-modules",
        Title = "内核模块与驱动",
        Keywords = "lsmod · modinfo · journalctl -k",
        Level = ModuleLevel.Advanced,
        Overview = "Linux 的大部分硬件驱动都是可以按需加载的内核模块 (.ko)。学会查看已加载的模块、模块参数，以及从内核日志里找到驱动加载失败的原因。",

        Why =
        [
            "1. 宏内核 (Monolithic) 与微内核 (Microkernel) 的折衷产物：",
            "• 微内核架构（如 Minix、Fuchsia）将硬件驱动全部放在用户空间，各组件通过 IPC 消息传递，安全性高但上下文切换与通信开销巨大，游戏与高吞吐 IO 性能受限。",
            "• Linux 坚持宏内核体系：所有驱动和文件系统都在 CPU Ring 0 特权级以函数直调方式极速运行。为了避免每次新增驱动都要重新编译整个内核，Linux 发明了 **LKM (Loadable Kernel Module)** 机制，允许将网卡、GPU、文件系统编译为独立的 `.ko` (Kernel Object) 动态库，在系统运行时按需加载进内核内存空间。",
            "2. 严格的内核符号校验 (Vermagic 与 Module CRC)：",
            "• 为什么其他 Linux 发行版的模块不能直接拷到 SteamOS 使用？",
            "• 模块在加载时，内核的模块装载器会严格比对该模块的 `vermagic`（内核版本号、SMP 多核支持、PREEMPT 抢占模型）以及内核导出函数 (`EXPORT_SYMBOL_GPL`) 的 CRC 校验和。只要有一处不一致，内核就会直接拒绝加载 (`-ENOEXEC`)，防止未经验证的二进制破坏内核地址空间导致 Kernel Panic 死机。",
            "3. 模块调度器：`modprobe` vs `insmod` 与 `depmod` 依赖树：",
            "• `insmod` 仅是对 `init_module` / `finit_module` 系统调用的简单包装，它完全不知道模块依赖什么父级模块。",
            "• `modprobe` 则是现代智能加载工具：它会读取 `/lib/modules/$(uname -r)/modules.dep.bin`（由 `depmod` 生成的有向无环依赖图 DAG），自动按依赖拓扑顺序把所有前置模块逐一装载入内核。",
            "4. 硬件即插即用绑定机制 (Hardware IDs 与 modules.alias)：",
            "• 插入 USB 设备时，内核读出它的厂商 ID 与产品 ID（例如 Deck 内置控制器为 `28de:1205`），udev 在 `modules.alias` 中查到匹配的驱动模块名，自动调用 modprobe 加载。",
        ],

        Consequences =
        [
            "• `lsmod`、`modinfo`、`journalctl -k` 都只读取数据，不影响系统运行。",
            "• 模块引用计数保护卸载安全：使用 `modprobe -r <mod>` 卸载模块时，内核会自动核查该模块的引用计数 (Used by)。只要有其他驱动或用户进程在使用该模块，内核会拒绝卸载并安全返回 `Resource temporarily unavailable` (-EBUSY)。切勿使用 `-f` (Force) 强行卸载关键驱动，否则会导致硬件无响应甚至系统死机。",
            "• 黑名单规则仅对后续生效：在 `/etc/modprobe.d/` 中添加 `blacklist <module>` 只会阻止 udev 自动加载，若要彻底禁止任何程序（包括 root）加载该模块，需配置 `install <module> /bin/true`。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前内核已加载的全部内核模块清单与依赖拓扑",
                "lsmod | head -n 25",
                "查看 Module 名称、所占内核内存大小 (Size)、以及当前正在使用该模块的计数与依赖方列表 (Used by)。"),
            Step.Read(
                "深度探测核心驱动模块的元数据、作者与导出参数 (modinfo)",
                "modinfo amdgpu | head -n 30",
                "查看 AMD 掌机核显 GPU 驱动的 vermagic、驱动别名 (alias)、依赖关系 (depends) 以及支持的动态调节参数 (parm)。"),
            Step.Read(
                "查看内核模块向 sysfs 暴露的可调运行参数",
                "ls -l /sys/module/amdgpu/parameters/ 2>/dev/null | head -n 20",
                "通过直接读取 sysfs 文件，洞悉 GPU 驱动的电源管理模式、显存调度策略等微观配置。"),
            Step.Read(
                "查看本次开机的内核错误与警告",
                "journalctl -k -b -p warning --no-pager | tail -n 30",
                "参数解析：-T 将内核微秒时间戳转换为可读的人类日期时间，--level=err,warn 精准过滤掉冗余信息，直击硬件报错核心。"),
            Step.Read(
                "查看引导加载程序 (Bootloader) 传递给内核的启动命令行参数",
                "cat /proc/cmdline",
                "查看内核启动时固化的全局引导选项（如 root 分区 UUID、audit 状态、控制台输出终端与内存安全选项）。"),
        ],

        Verify =
        [
            "1. 模块存活验证：执行 `lsmod | grep -E \"^(amdgpu|btrfs|overlay)\"`，若能正常打印出条目，证实核心显卡驱动与文件系统驱动在 Ring 0 空间稳定运行。",
            "2. 外接设备无法识别时，插上设备后立即执行 `journalctl -k -n 30 --no-pager`，查看是否有 `device descriptor read/64, error`（供电或线缆问题）或 `failed to load firmware`（缺少固件）等信息。",
        ],
    };
}
