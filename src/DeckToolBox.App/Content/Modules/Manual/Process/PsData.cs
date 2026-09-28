using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Process;

public static class PsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-ps",
        Title = "ps：列出进程",
        Keywords = "ps aux · -ef · -eo · --sort · --forest · STAT",
        Level = ModuleLevel.Basic,
        Overview = "ps 输出一次进程快照：有哪些进程、谁启动的、占多少 CPU 和内存、运行了多久、完整的启动命令。与交互式的 top/htop 不同，ps 的输出适合保存或交给 grep、sort 继续处理。",

        Why =
        [
            "1. 两种常见写法（选项风格不同，结果相近）：",
            "- `ps aux`：BSD 风格，列出所有进程，含 `%CPU`、`%MEM`、`VSZ`、`RSS`、`STAT`、`START`、`TIME`、`COMMAND`。",
            "- `ps -ef`：System V 风格，含 `UID`、`PID`、`PPID`（父进程）、`STIME`、`CMD`。",
            "2. 自定义列：`ps -eo 列1,列2,...`，常用列名：",
            "- `pid`、`ppid`、`user`、`comm`（程序名）、`cmd`/`args`（完整命令行）。",
            "- `%cpu`、`%mem`、`rss`（实际内存，KB）、`vsz`（虚拟内存）。",
            "- `etime`（已运行时长）、`stat`（状态）、`ni`（nice 值）、`nlwp`（线程数）。",
            "3. 排序与树形：`--sort=-%cpu`（按 CPU 降序，`-` 表示降序）；`--forest` 以树形显示父子关系。",
            "4. STAT 列：首字母为进程状态（R 运行、S 睡眠、D 不可中断、Z 僵尸、T 暂停），后面的附加字符：`s` 会话首进程、`l` 多线程、`+` 前台进程组、`<` 高优先级、`N` 低优先级。",
            "5. `%CPU` 是进程生命周期内的平均值，不是此刻的瞬时占用；看实时占用用 top/htop。",
        ],

        Consequences =
        [
            "• ps 只读取 `/proc`，不影响任何进程。",
            "• 命令行过长时会被截断到终端宽度，加 `ww`（如 `ps auxww`）显示完整内容，或用 `cut` 自己截取。",
        ],

        Steps =
        [
            Step.Read(
                "列出所有进程（BSD 风格）",
                "ps aux | head -n 5",
                "只看前 5 行。").Then("第一行是表头 `USER PID %CPU %MEM VSZ RSS TTY STAT START TIME COMMAND`，下面第一个进程是 PID 1 的 `/usr/lib/systemd/systemd`。"),
            Step.Read(
                "按 CPU 占用排序，自定义列",
                "ps -eo pid,user,%cpu,%mem,etime,comm --sort=-%cpu | head -n 8",
                "`etime` 显示已运行时长，格式为 `天-时:分:秒`。").Then("按平均 CPU 占用从高到低列出 7 个进程，如 `550696 deck 44.4 9.0 1-08:48:55 chrome`。"),
            Step.Read(
                "按内存占用排序",
                "ps -eo pid,rss,comm --sort=-rss | head -n 6 | awk 'NR==1{print; next}{printf \"%7s %7.0f MB  %s\\n\", $1, $2/1024, $3}'",
                "`rss` 单位是 KB，用 awk 换算成 MB。").Then("列出占用物理内存最多的 5 个进程及其 MB 数。"),
            Step.Read(
                "以树形查看某个程序的子进程",
                "ps -ef --forest | grep -A 3 '[s]team$' | cut -c1-120",
                "`[s]team` 的写法避免匹配到 grep 自己。").Then("显示 steam 主进程及其下方以 `\\_` 连接的子进程，如 `steam-runtime-launcher-service`。"),
            Step.Read(
                "查看单个进程的详细信息",
                "ps -o pid,ppid,stat,ni,nlwp,etime,args -p $(pgrep -n steam)",
                "`-p` 指定 PID；`pgrep -n` 取最新的一个 steam 进程。").Then("一行信息：PID、父 PID、状态（如 `Sl`）、nice 值、线程数、运行时长和完整命令行。"),
            Step.Read(
                "统计进程和线程总数",
                "echo \"进程 $(ps -e --no-headers | wc -l) 个，线程 $(ps -eL --no-headers | wc -l) 个\"",
                "`-L` 每个线程一行。").Then("输出如 `进程 361 个，线程 1690 个`。"),
        ],

        Verify =
        [
            "1. `ps -o comm= -p 1` 输出 `systemd`：PID 1 始终是 systemd。",
        ],
    };
}
