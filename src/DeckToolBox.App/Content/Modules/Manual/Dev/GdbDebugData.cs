using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Dev;

public static class GdbDebugData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-gdb",
        Title = "gdb 与 coredumpctl：程序崩溃在哪",
        Keywords = "gdb · coredumpctl info · backtrace · bt · 段错误 · SIGSEGV · ptrace_scope",
        Level = ModuleLevel.Advanced,
        Overview = "程序闪退或报「段错误 (核心已转储)」时，`coredumpctl info` 直接给出崩溃那一刻的调用栈，能看出崩溃发生在程序自身还是某个库里，附在问题报告中最有用。需要更多细节时，用 `gdb` 打开转储文件，或者让程序在 gdb 中运行，崩溃时停下来查看。",

        Why =
        [
            "1. 崩溃与核心转储：程序访问了不该访问的内存时，内核向它发送 `SIGSEGV`（段错误）；主动中止时是 `SIGABRT`。进程被终止，`systemd-coredump` 把当时的内存保存为压缩的转储文件，并记录一条摘要。",
            "2. 调用栈 (backtrace)：记录崩溃时函数一层层的调用关系，最上面一行（`#0`）是崩溃发生的位置。每行形如 `#0 0x... XInternAtom (libX11.so.6 + 0x28d55)`：函数名、所在的库及偏移。没有调试符号的程序显示 `n/a` 或 `??`，但库名仍能说明问题出在哪一块。",
            "3. SteamOS 上的两个限制：",
            "- 转储文件保存在 `/var/lib/systemd/coredump/`，权限为 root 专属，`coredumpctl list` 的 `COREFILE` 列显示 `inaccessible`。普通用户能看摘要和调用栈（`coredumpctl info`），要用 gdb 打开转储文件则需要 sudo。",
            "- `/proc/sys/kernel/yama/ptrace_scope` 为 `1`：普通用户的 gdb 只能调试自己启动的子进程，`gdb -p <PID>` 附加到已在运行的进程会报「不允许的操作」，需要 sudo。",
            "4. 常用 gdb 命令：`run` 运行程序；`bt` 显示调用栈，`bt full` 连同局部变量；`thread apply all bt` 显示所有线程的调用栈（游戏通常有几十个线程）；`info registers` 显示寄存器；`quit` 退出。",
            "5. debuginfod：gdb 启动时会询问是否从 Arch 与 SteamOS 的 debuginfod 服务器下载调试符号。下载后调用栈能显示源码文件与行号，但第一次会比较慢；下面的命令用 `-iex 'set debuginfod enabled off'` 关掉这个询问。",
        ],

        Consequences =
        [
            "• `coredumpctl list` / `info` 只读。",
            "• gdb 附加到运行中的进程时，该进程的所有线程会暂停，直到 gdb 分离或退出；不要对正在保存数据的程序这样做。",
            "• 转储文件包含程序崩溃时的全部内存，可能有密码、令牌等敏感内容，不要直接公开分享，只分享调用栈文字即可。",
        ],

        Steps =
        [
            Step.Read(
                "列出最近的崩溃记录",
                "coredumpctl list --no-pager | tail -n 10",
                "每次崩溃一行。`SIG` 列是导致崩溃的信号。").Then("每行依次是时间、PID、UID、GID、信号（如 `SIGSEGV`、`SIGABRT`）、转储状态（SteamOS 上通常为 `inaccessible`）、程序路径；没有记录时提示 `No coredumps found.`。"),
            Step.Read(
                "查看某次崩溃的摘要与调用栈",
                "coredumpctl info <PID> --no-pager",
                "`<PID>` 取上一步列表中的第二列；也可以用程序名代替，如 `coredumpctl info mangoapp`，显示该程序最近一次崩溃。").Then("先是 `PID`、`Signal: 11 (SEGV)`、`Command Line`、`Executable`、`Storage: ... (inaccessible)` 等字段，随后 `Stack trace of thread <PID>:` 下是十几行 `#0`、`#1` 开头的调用栈。"),
            Step.Read(
                "让程序在 gdb 中运行，崩溃时打印调用栈（演示）",
                "gdb -q -iex 'set debuginfod enabled off' --batch -ex run -ex bt --args sh -c 'kill -SEGV $$'",
                "这里让一个 shell 给自己发段错误信号，模拟崩溃。换成自己的程序时写成 `--args <程序> <参数>`；程序是 gdb 的子进程，所以不需要 sudo。").Then("先输出 `Program received signal SIGSEGV, Segmentation fault.`，再输出调用栈：`#0 ... in kill () from /usr/lib/libc.so.6`、`#1 ... in kill_builtin ()`……最后一行是 `main ()`。"),
            Step.Root(
                "用 gdb 打开崩溃转储",
                "sudo coredumpctl gdb <PID>",
                "转储文件只有 root 能读，所以需要 sudo。进入 gdb 后输入 `bt` 查看调用栈，`thread apply all bt` 查看所有线程，`quit` 退出。").Then("输入密码后 gdb 载入转储，显示 `Program terminated with signal SIGSEGV` 并停在崩溃位置，出现 `(gdb)` 提示符等待输入。"),
            Step.Root(
                "抓取一个卡住的进程的全部线程调用栈",
                "sudo gdb -q -iex 'set debuginfod enabled off' --batch -p <PID> -ex 'thread apply all bt'",
                "用于程序卡死但没有崩溃的情况。`<PID>` 用 `pgrep -a <程序名>` 查。`--batch` 执行完命令后自动分离，进程继续运行。").Then("输出 `Thread 1 (...)`、`Thread 2 (...)` 等每个线程一段调用栈，然后 gdb 退出，目标进程恢复运行。卡死的线程通常停在 `futex_wait`、`poll` 等等待函数中。"),
        ],

        Verify =
        [
            "1. `cat /proc/sys/kernel/yama/ptrace_scope` 输出 `1`，说明附加到其他进程需要 sudo。",
            "2. 「让程序在 gdb 中运行」一步的输出中有 `Program received signal SIGSEGV`。",
        ],
    };
}
