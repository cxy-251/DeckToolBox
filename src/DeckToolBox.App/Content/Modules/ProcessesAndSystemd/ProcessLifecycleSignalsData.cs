using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.ProcessesAndSystemd;

public static class ProcessLifecycleSignalsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "processes-signals",
        Title = "进程状态与信号",
        Keywords = "ps · kill · nice",
        Level = ModuleLevel.Intermediate,
        Overview = "进程在运行、等待、退出之间切换状态，系统和用户通过「信号」通知进程暂停、退出或重新加载配置。理解状态与信号，才能判断一个程序是在忙、在等，还是已经卡死，以及应该怎样结束它。",

        Why =
        [
            "1. 进程状态（`ps` 的 `STAT` 列首字母）：",
            "- `R`：正在运行或等待 CPU。",
            "- `S`：可中断睡眠，在等待输入、网络或定时器，大多数进程大部分时间处于此状态。",
            "- `D`：不可中断睡眠，通常在等待磁盘或驱动完成 IO。此时进程不响应任何信号，包括 `SIGKILL`，只能等 IO 结束。大量 D 状态进程往往意味着存储设备很慢或出了故障。",
            "- `Z`：僵尸进程。进程已经退出、资源已释放，只剩进程表中的一项，等待父进程读取其退出码。它不占 CPU 和内存。",
            "2. 常用信号：",
            "- `SIGTERM`（15）：请求退出，`kill` 的默认信号。程序可以捕获它，先保存数据再退出。",
            "- `SIGKILL`（9）：由内核直接终止进程，程序无法捕获或忽略，也就没有机会保存数据。",
            "- `SIGINT`（2）：终端中按 `Ctrl+C` 发送的信号。",
            "- `SIGHUP`（1）：终端关闭时发送；不少守护进程约定用它重新加载配置。",
            "3. 调度优先级 (nice)：取值 -20 到 19，数值越大越「谦让」，只在 CPU 繁忙时才体现差别。普通用户只能调大自己进程的 nice 值。",
        ],

        Consequences =
        [
            "• 查看进程和信号列表是只读操作。",
            "• 结束进程时先用 `kill <PID>`（SIGTERM），等几秒无效再用 `kill -9`。直接 `-9` 可能导致程序的数据库、配置文件写到一半而损坏。",
            "• 僵尸进程无法被 kill，也没有必要：结束它的父进程后，它会被 systemd 回收。少量僵尸进程无害。",
            "• 调大 nice 值后普通用户无法再调回去，只能重新启动该程序。",
        ],

        Steps =
        [
            Step.Read(
                "以树形查看进程的父子关系",
                "pstree -p -u | head -n 30",
                "`-p` 显示 PID，`-u` 显示用户切换。").Then("以树形显示前 30 行：最顶端是 `systemd(1)`，下面分支是各个服务和你的会话，括号中是 PID，部分分支带有 `,用户名` 表示以该用户运行。"),
            Step.Read(
                "检查是否有 D 状态或僵尸进程",
                "ps axo stat,pid,ppid,comm | grep -E \"^(D|Z)\" || echo \"恭喜：当前系统无僵尸与死锁进程\"",
                "`D` 表示不可中断的等待（多为磁盘 IO），`Z` 表示已退出但未被父进程回收。").Then("通常输出「恭喜：当前系统无僵尸与死锁进程」；若有，每行显示状态、PID、父进程 PID 和进程名。"),
            Step.Read(
                "列出所有信号的编号与名称",
                "kill -l",
                "`kill -<编号>` 或 `kill -<名称>` 发送指定信号。").Then("输出多列表格：`1) SIGHUP  2) SIGINT ... 9) SIGKILL ... 15) SIGTERM`，以及 `SIGRTMIN` 开始的实时信号。"),
            Step.Change(
                "以较低优先级启动一个子 shell",
                "nice -n 10 bash",
                "普通用户只能调低优先级（增大 nice 值），调高需要 root。").Then("看起来什么也没发生，但你已进入一个新的 bash；此后在其中启动的程序 nice 值都是 10。执行 `ps -o pid,ni,comm -p $$` 可以看到 `NI` 列为 10。`exit` 返回原 shell。"),
        ],

        Verify =
        [
            "1. 在 `nice -n 10 bash` 启动的 shell 中执行 `ps -o pid,ni,comm -p $$`，`NI` 列为 10。",
            "2. 运行 `sleep 300`，在另一个终端执行 `pkill -INT sleep`，第一个终端中的 sleep 立即结束，效果与按 `Ctrl+C` 相同。",
        ],
    };
}
