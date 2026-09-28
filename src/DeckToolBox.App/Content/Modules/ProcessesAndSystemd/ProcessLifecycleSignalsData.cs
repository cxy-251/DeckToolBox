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
        Overview = "深入剖析 Linux 进程状态机、僵尸进程成因、内核信号体系 (SIGTERM/SIGKILL) 与 Nice 调度优先级。",

        Why =
        [
            "1. 进程核心状态机 (Process States)：",
            "• R (Running/Runnable)：正在 CPU 上执行，或在就绪调度队列中等待时间片分配。",
            "• S (Interruptible Sleep)：可中断睡眠。进程等待特定 I/O、按键或套接字事件，收到信号能立即唤醒。",
            "• D (Uninterruptible Sleep)：不可中断深度睡眠。通常在等待磁盘或驱动底层硬件响应。极其特殊：处于 D 状态的进程连 kill -9 都无法杀死！",
            "• Z (Zombie)：僵尸进程。进程代码和资源已全部释放，但父进程尚未调用 waitpid() 回收退出码，仅在内核进程表中占用一个 PID 卡位。",
            "2. 异步信号捕获体系 (Signals)：",
            "• SIGTERM (15)：优雅退出请求。应用可以捕获此信号并执行清理数据库、保存游戏存档、关闭网络连接后平稳退出。",
            "• SIGKILL (9)：内核硬终结。不可被捕获、不可被忽略！内核直接强行抹杀该进程并强行回收其所有物理内存与文件描述符。",
            "• SIGHUP (1)：挂断信号。终端关闭时发送；许多守护进程约定用它作为“零停机热重载配置文件”的触发机制。",
            "3. 调度优先级与 Nice 值：",
            "• 范围从 -20 (极高优先级，抢占 CPU) 到 19 (最谦让优先级，仅吃闲置 CPU)。",
        ],

        Consequences =
        [
            "• 切忌无脑 kill -9：优先使用 kill (默认发送 SIGTERM 15)，给程序保存状态与清理锁文件的机会。动辄直接 -9 极易导致 SQLite 数据库损坏或临时锁文件遗留引发下次启动失败。",
            "• 僵尸进程无法被杀：杀僵尸进程必须杀死其父进程（PPID），由 PID 1 自动接管并瞬间收割回收。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统层级清晰的进程树状继承关系",
                "pstree -p -u | head -n 30",
                "`-p` 显示 PID，`-u` 显示用户切换。").Then("以树形显示前 30 行：最顶端是 `systemd(1)`，下面分支是各个服务和你的会话，括号中是 PID，部分分支带有 `,用户名` 表示以该用户运行。"),
            Step.Read(
                "排查系统当前是否存在僵尸进程 (Z) 或死锁进程 (D)",
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
            "执行 ps -o pid,ni,comm -p $$，你会看到当前进程的 NI (Nice) 列已变为 10，证明 Linux CFS 调度器已成功接受优先级变更。",
        ],
    };
}
