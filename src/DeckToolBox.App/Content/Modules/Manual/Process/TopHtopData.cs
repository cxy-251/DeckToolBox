using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Process;

public static class TopHtopData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-top-htop",
        Title = "top、htop、btop：实时监控",
        Keywords = "top · htop · btop · 快捷键 · 负载 · 批处理模式",
        Level = ModuleLevel.Basic,
        Overview = "top、htop、btop 都实时显示系统负载与进程列表，每隔一两秒刷新一次。top 在任何 Linux 上都有，htop 更易操作，btop 图形最丰富。三者 SteamOS 都自带。",

        Why =
        [
            "1. 顶部摘要（以 top 为例）：",
            "- `load average`：1、5、15 分钟平均负载（见「性能观测与追踪」）。",
            "- `任务`：进程总数，以及运行、睡眠、不可中断、僵尸的数量。",
            "- `%Cpu(s)`：`用` 用户程序、`系` 内核、`闲` 空闲、`等` 等待 IO（持续偏高说明磁盘是瓶颈）。",
            "- 内存与交换：总计、空闲、已用、缓冲/缓存，以及 `可用` 内存。",
            "2. 进程列表的主要列：`VIRT` 虚拟内存、`RES` 实际内存、`S` 状态、`%CPU`（单核满载为 100%，多核可超过 100%）、`TIME+` 累计 CPU 时间。",
            "3. top 的按键：`P` 按 CPU 排序、`M` 按内存排序、`1` 分别显示每个核心、`k` 结束进程（输入 PID）、`q` 退出。",
            "4. htop 的按键：方向键选择进程、`F3` 或 `/` 搜索、`F4` 过滤、`F5` 树形、`F6` 选择排序列、`F9` 发送信号、`Space` 标记多个进程、`H` 隐藏用户线程、`q` 退出。可以用鼠标或触屏点击列标题排序。",
            "5. btop：上方为 CPU、内存、网络、磁盘的历史图表，下方为进程列表；方向键选择、`f` 过滤、`k` 结束、`m` 切换显示模式、`Esc` 打开菜单、`q` 退出。",
            "6. `top -b -n 1`：批处理模式，只输出一次快照，适合保存到文件或在脚本中使用。",
        ],

        Consequences =
        [
            "• 查看本身不改变系统；在 htop/btop 中按 `F9`/`k` 会向所选进程发送信号，先确认选中的是哪个进程。",
            "• 监控工具本身也占用少量 CPU，刷新间隔越短占用越高。",
        ],

        Steps =
        [
            Step.Read(
                "输出一次 top 快照",
                "top -b -n 1 | head -n 15",
                "`-b` 批处理模式，`-n 1` 只输出一次。").Then("前 5 行是系统摘要（运行时间、负载、任务数、CPU、内存），之后是按 CPU 排序的进程表。"),
            Step.Read(
                "交互式 top，按内存排序",
                "top",
                "进入后按 `M` 按内存排序，按 `1` 展开每个核心，按 `q` 退出。").Then("全屏界面每 3 秒刷新；按 `M` 后占内存最多的进程排到最上面；按 `1` 后 `%Cpu` 一行变为 `%Cpu0` 到 `%Cpu7` 八行。"),
            Step.Read(
                "htop：树形查看、搜索",
                "htop",
                "按 `F5` 切换树形，按 `F3` 输入 steam 搜索，按 `q` 退出。").Then("顶部是每个核心的彩色占用条和内存条；`F5` 后进程按父子关系缩进显示；搜索时高亮定位到第一个匹配的进程。"),
            Step.Read(
                "htop：只看某个用户、按内存排序启动",
                "htop -u deck -s PERCENT_MEM",
                "`-u` 只显示指定用户的进程，`-s` 指定排序列。").Then("只列出 deck 用户的进程，并已按内存占用降序排列。"),
            Step.Read(
                "btop：图形化监控",
                "btop",
                "按 `Esc` 打开菜单，`q` 退出。").Then("全屏显示 CPU 各核心、内存、网络上下行速度、磁盘读写的实时图表，下方是进程列表。"),
        ],

        Verify =
        [
            "1. 运行一个游戏后打开 htop，该游戏（或 `wine64-preloader`、`*.exe` 进程）出现在 CPU 占用的前几位。",
        ],
    };
}
