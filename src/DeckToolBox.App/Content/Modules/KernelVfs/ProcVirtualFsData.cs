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
                "查看交换倾向参数 swappiness",
                "cat /proc/sys/vm/swappiness",
                "取值 0~200；越大内核越倾向于把不常用的内存页换出到交换空间。").Then("输出一个数字，SteamOS 上为 `60`。"),
        ],

        Verify =
        [
            "1. `cat /proc/$$/cmdline | tr '\\0' ' '` 输出 `bash` 或当前 shell 的启动命令。",
            "2. `sysctl vm.swappiness` 与 `cat /proc/sys/vm/swappiness` 输出相同的值。",
        ],
    };
}
