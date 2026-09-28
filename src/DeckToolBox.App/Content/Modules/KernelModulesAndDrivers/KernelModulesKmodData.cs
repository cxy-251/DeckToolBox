using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.KernelModulesAndDrivers;

public static class KernelModulesKmodData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "kernel_modules_kmod",
        Title = "可加载内核模块机制与硬件驱动生命周期 (kmod / lsmod / modprobe / dmesg)",
        Subtitle = "内核模块与硬件抽象",
        CategoryName = "内核模块与硬件抽象",
        Badge = "内核扩展",
        Overview = "深入解析 Linux 可加载内核模块 (LKM) 动态链接原理、/lib/modules 驱动依赖树、符号版本控制与环形日志 (dmesg) 硬件诊断。",

        WhyCard = new InfoCardItem(
            "📌",
            "宏内核下的动态性：Linux 可加载内核模块 (LKM) 的底层机制",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
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
                "• 当你插入 USB 手柄或扩展坞时，内核检测到硬件的 Vendor ID 与 Product ID (如 VID:1145, PID:0004)，udev 会在 `modules.alias` 中快速检索该硬件指纹对应的驱动模块名，并自动通知 modprobe 完成热插拔驱动挂载。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "模块加载、卸载与黑名单的风险边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 探测与查询绝对安全：执行 `lsmod`、`modinfo`、`dmesg` 仅读取内核现存数据结构，对系统运行毫无影响。",
                "• 模块引用计数保护卸载安全：使用 `modprobe -r <mod>` 卸载模块时，内核会自动核查该模块的引用计数 (Used by)。只要有其他驱动或用户进程在使用该模块，内核会拒绝卸载并安全返回 `Resource temporarily unavailable` (-EBUSY)。切勿使用 `-f` (Force) 强行卸载关键驱动，否则会导致硬件无响应甚至系统死机。",
                "• 黑名单规则仅对后续生效：在 `/etc/modprobe.d/` 中添加 `blacklist <module>` 只会阻止 udev 自动加载，若要彻底禁止任何程序（包括 root）加载该模块，需配置 `install <module> /bin/true`。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看当前内核已加载的全部内核模块清单与依赖拓扑",
                "lsmod | head -n 25",
                "查看 Module 名称、所占内核内存大小 (Size)、以及当前正在使用该模块的计数与依赖方列表 (Used by)。"
            ),
            new(
                2,
                "深度探测核心驱动模块的元数据、作者与导出参数 (modinfo)",
                "modinfo amdgpu | head -n 30",
                "查看 AMD 掌机核显 GPU 驱动的 vermagic、驱动别名 (alias)、依赖关系 (depends) 以及支持的动态调节参数 (parm)。"
            ),
            new(
                3,
                "查看内核模块向 sysfs 暴露的可调运行参数",
                "ls -l /sys/module/amdgpu/parameters/ 2>/dev/null | head -n 20",
                "通过直接读取 sysfs 文件，洞悉 GPU 驱动的电源管理模式、显存调度策略等微观配置。"
            ),
            new(
                4,
                "检索内核环形缓冲区 (dmesg) 中的硬件加载与报错日志",
                "dmesg -T --level=err,warn | tail -n 20",
                "参数解析：-T 将内核微秒时间戳转换为可读的人类日期时间，--level=err,warn 精准过滤掉冗余信息，直击硬件报错核心。"
            ),
            new(
                5,
                "查看引导加载程序 (Bootloader) 传递给内核的启动命令行参数",
                "cat /proc/cmdline",
                "查看内核启动时固化的全局引导选项（如 root 分区 UUID、audit 状态、控制台输出终端与内存安全选项）。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "模块状态验证与系统完整性判定 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 模块存活验证：执行 `lsmod | grep -E \"^(amdgpu|btrfs|overlay)\"`，若能正常打印出条目，证实核心显卡驱动与文件系统驱动在 Ring 0 空间稳定运行。",
                "2. 硬件链路验证：若新插入的外接设备无法识别，执行 `dmesg -T | tail -n 30`，观察内核是否抛出 `device descriptor read/64, error` 或 `firmware: failed to load`，即可秒级定界是硬件供电不足、线缆接触不良还是固件缺失。"
            }
        )
    };
}
