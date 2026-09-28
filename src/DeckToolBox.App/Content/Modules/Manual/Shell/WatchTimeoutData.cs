using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class WatchTimeoutData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-watch-timeout",
        Title = "watch、timeout、sleep 与 time",
        Keywords = "watch -n · -d · timeout · sleep · time",
        Level = ModuleLevel.Basic,
        Overview = "四个与「时间」有关的小工具：watch 每隔几秒重复执行一条命令并刷新屏幕，适合盯着温度、下载进度；timeout 给命令设置最长运行时间；sleep 等待一段时间；time 测量一条命令花了多久。",

        Why =
        [
            "1. watch：",
            "- `watch -n 2 命令`：每 2 秒执行一次并全屏刷新显示，`Ctrl+C` 退出。",
            "- `-d`：高亮显示与上一次相比发生变化的字符。",
            "- 命令中含管道或引号时，把整条命令放在单引号里：`watch -n1 'sensors | grep edge'`。",
            "2. timeout：`timeout 10 命令`，10 秒后仍未结束就发送 SIGTERM 终止它；`-k 5` 再过 5 秒仍未退出则发送 SIGKILL。时间可写 `30s`、`5m`、`1h`。被超时终止时退出码为 124。",
            "3. sleep：`sleep 5`、`sleep 0.5`、`sleep 2m`。在脚本中用于等待，在命令行中常与 `;` 组合（如 `sleep 2; kscreen-doctor --dpms off`）。",
            "4. time：放在命令前面，命令结束后输出三个时间：",
            "- `real`：实际经过的时间（墙钟时间）。",
            "- `user`：CPU 在用户态执行该程序的时间。",
            "- `sys`：CPU 为它执行内核操作的时间。",
            "• `user + sys` 明显大于 `real`，说明程序用了多个 CPU 核心并行计算；`real` 远大于 `user + sys`，说明时间主要花在等待（磁盘、网络、sleep）上。",
        ],

        Consequences =
        [
            "• watch 会反复执行命令，不要用它重复执行有副作用的命令（例如写文件、发网络请求）。",
            "• timeout 终止的程序没有机会完成工作，可能留下不完整的输出文件。",
        ],

        Steps =
        [
            Step.Read(
                "每秒刷新 APU 温度与功耗",
                "watch -n 1 'sensors | grep -E \"edge|PPT|Fan\"'",
                "`Ctrl+C` 退出。").Then("终端清屏，顶部显示 `每 1.0s: sensors | ...` 与当前时间，下方三四行数值每秒刷新一次。"),
            Step.Read(
                "高亮变化：观察内存变化",
                "watch -d -n 2 free -h",
                "`-d` 高亮与上次不同的地方。").Then("free 的表格每 2 秒刷新，发生变化的数字以反色高亮显示。"),
            Step.Read(
                "限制命令的最长运行时间",
                "timeout 3 ping 223.5.5.5; echo \"退出码 $?\"",
                "ping 默认会一直运行，这里 3 秒后被终止。").Then("显示约 3 行 ping 结果后停止，最后输出 `退出码 124`，表示是被 timeout 终止的。"),
            Step.Read(
                "测量命令耗时",
                "time find /usr/share -name '*.png' 2>/dev/null | wc -l",
                "`time` 测量的是整条管道。").Then("先输出找到的 png 数量，然后是 `real 0m0.5s`、`user ...`、`sys ...` 三行耗时。"),
            Step.Read(
                "比较并行与串行的耗时",
                "time (sleep 1 & sleep 1 & wait)",
                "两个 sleep 同时在后台运行，`wait` 等待它们都结束。").Then("`real` 约为 1 秒而不是 2 秒；`user` 与 `sys` 几乎为 0，说明时间都花在等待上。"),
        ],

        Verify =
        [
            "1. `timeout 1 sleep 5; echo $?` 输出 `124`；`timeout 5 sleep 1; echo $?` 输出 `0`。",
        ],
    };
}
