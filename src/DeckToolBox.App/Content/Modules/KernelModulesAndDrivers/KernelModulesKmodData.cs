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
            "1. 宏内核与模块：",
            "• Linux 是宏内核：驱动和文件系统都在内核空间运行，彼此直接调用函数，效率高。",
            "• 为了不必为每个新驱动重新编译整个内核，驱动可以编译成独立的可加载模块 (`.ko` 文件)，在需要时加载进内核。",
            "2. 版本匹配：",
            "• 模块必须为正在运行的这个内核版本编译。加载时，内核会核对模块的 `vermagic`（内核版本与编译选项）以及所用内核符号的校验值，不匹配就拒绝加载，以免破坏内核。",
            "• 因此不能把其他发行版或其他内核版本的 `.ko` 直接拿来用。",
            "3. 加载工具：",
            "• `modprobe` 会根据 `modules.dep` 先加载被依赖的模块，是加载模块的正确方式；`insmod` 只加载单个文件，不处理依赖。",
            "• 插入 USB 设备时，内核读出它的厂商 ID 与产品 ID（例如 Deck 内置控制器为 `28de:1205`），udev 在 `modules.alias` 中查到匹配的驱动模块名，自动调用 modprobe 加载。",
            "4. 内核日志：驱动加载、硬件初始化、固件加载失败等信息都写在内核日志中，SteamOS 上用 `journalctl -k` 查看（`dmesg` 对普通用户受限）。",
        ],

        Consequences =
        [
            "• `lsmod`、`modinfo`、`journalctl -k` 都只读取数据，不影响系统运行。",
            "• `sudo modprobe -r <模块>` 卸载模块时，若模块正在被使用，内核会拒绝并提示 `Module is in use`。不要用 `rmmod -f` 强制卸载，可能导致设备失去响应或系统崩溃。",
            "• 在 `/etc/modprobe.d/*.conf` 中写 `blacklist <模块>` 只阻止自动加载，其他模块依赖它时仍会被加载；要完全禁止，写 `install <模块> /bin/false`。",
        ],

        Steps =
        [
            Step.Read(
                "列出已加载的内核模块",
                "lsmod | head -n 25",
                "显示前 25 行。").Then("表格 `Module  Size  Used by`：模块名、占用内存字节数、被引用次数及引用它的模块。"),
            Step.Read(
                "查看一个模块的信息与参数",
                "modinfo amdgpu | head -n 30",
                "`modinfo <模块名>` 适用于任何模块。").Then("显示 `filename`（.ko 文件路径）、`license`、`description`、许多 `alias:` 行（它支持的硬件 ID）、`depends`、`vermagic`，以及 `parm:` 开头的可调参数。"),
            Step.Read(
                "查看模块当前的参数值",
                "ls -l /sys/module/amdgpu/parameters/ 2>/dev/null | head -n 20",
                "只有模块声明为可读的参数才会出现在这里。").Then("每行一个参数文件，如 `audio`、`dpm`、`gpu_recovery`；用 `cat` 读取可以看到当前值。"),
            Step.Read(
                "查看本次开机的内核错误与警告",
                "journalctl -k -b -p warning --no-pager | tail -n 30",
                "`-k` 只看内核消息，`-b` 本次开机，`-p warning` 只显示警告及以上级别。").Then("每行一条内核消息：时间、`kernel:`、内容；显示最后 30 条警告及更严重的消息。很多警告是无害的，重点关注与出问题设备相关的行。"),
            Step.Read(
                "查看内核启动参数",
                "cat /proc/cmdline",
                "这些参数在开机时由引导程序传入，决定内核的部分行为。").Then("输出一长行启动参数，如 `root=...`、`rd.systemd...`、`amd_iommu=off` 等。"),
        ],

        Verify =
        [
            "1. `lsmod | grep -E '^(amdgpu|btrfs|overlay)'` 能列出显卡驱动与文件系统模块。",
            "2. 外接设备无法识别时，插上设备后立即执行 `journalctl -k -n 30 --no-pager`，查看是否有 `device descriptor read/64, error`（供电或线缆问题）或 `failed to load firmware`（缺少固件）等信息。",
        ],
    };
}
