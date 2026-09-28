using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Shell;

public static class DateCalcData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-date-calc",
        Title = "date、cal 与 qalc：日期和计算",
        Keywords = "date + 格式 · -d · 时间戳 · cal · qalc · 单位换算 · 汇率",
        Level = ModuleLevel.Basic,
        Overview = "date 显示和计算日期（脚本中给文件加日期后缀最常用），cal 显示日历，qalc 是 SteamOS 自带的命令行计算器：支持小数、百分比、单位换算、进制转换和汇率。",

        Why =
        [
            "1. date 的格式：`date +格式`，常用占位符：",
            "- `%Y` 年、`%m` 月、`%d` 日、`%H` 时、`%M` 分、`%S` 秒。",
            "- `%F` = `%Y-%m-%d`，`%T` = `%H:%M:%S`。",
            "- `%s`：Unix 时间戳（1970-01-01 以来的秒数）。",
            "- `%A` 星期几、`%j` 一年中的第几天。",
            "2. date 的时间计算：`-d` 描述一个时间，如 `-d tomorrow`、`-d '+3 days'`、`-d 'last monday'`、`-d '2026-12-31'`、`-d @时间戳`。",
            "3. cal：`cal` 本月、`cal 2027` 全年、`cal -3` 前后三个月。",
            "4. qalc（Qalculate!）：",
            "- 普通计算支持小数：`qalc '1920/1080'`。bash 的 `$(( ))` 只能算整数。",
            "- 百分比：`qalc '200 * 15%'`。",
            "- 单位换算：用 `to` 连接，如 `'5 GiB to MB'`、`'60 km/h to m/s'`、`'(1.5 h + 45 min) to min'`。",
            "- 进制：`'255 to hex'`、`'0x1F'`。",
            "- 汇率：`'100 USD to CNY'`。不加 `-e` 使用本地保存的旧汇率；`qalc -e` 先联网更新汇率再计算。",
            "- `-t` 只输出结果；不带表达式直接运行 `qalc` 进入交互模式，输入 `quit` 退出。",
        ],

        Consequences =
        [
            "• 这些命令只读，不改变系统时间。修改系统时间或时区用 `timedatectl`（见「查看硬件与系统信息」）。",
            "• qalc 首次运行会在 `~/.config/qalculate/` 创建配置文件；`-e` 会联网下载汇率数据。",
            "• qalc 按单位语义计算，写法不规范时可能得到意外的单位组合（如 `s·min`）；换算时把表达式用括号括起来，并用 `to` 指定目标单位。",
        ],

        Steps =
        [
            Step.Read(
                "按指定格式显示当前时间",
                "date '+%Y-%m-%d %H:%M:%S'; date +%F; date +%s",
                "格式以 `+` 开头。").Then("三行：`2026-09-28 20:06:00`、`2026-09-28`，以及一个十位数的时间戳。"),
            Step.Read(
                "计算相对日期",
                "date -d tomorrow +%F; date -d '+30 days' '+%F %A'; date -d 'last monday' +%F",
                "`-d` 描述一个时间点。").Then("依次输出明天的日期、30 天后的日期与星期几、上周一的日期。"),
            Step.Read(
                "时间戳与日期互转",
                "date -d @0; date -d '2027-01-01' +%s",
                "`@` 后跟时间戳。").Then("第一行 `1970年 01月 01日 星期四 08:00:00 CST`（北京时间），第二行是 2027 年元旦对应的时间戳。"),
            Step.Change(
                "给备份文件加上日期时间后缀",
                "cp ~/.bashrc /tmp/bashrc-$(date +%Y%m%d-%H%M%S).bak && ls /tmp/bashrc-*.bak",
                "脚本中最常见的用法。").Then("列出 `/tmp/bashrc-20260928-200600.bak` 这样的文件名。"),
            Step.Read(
                "查看日历",
                "cal -3",
                "显示上月、本月、下月。").Then("三个月的日历并排显示，今天的日期以反色高亮。"),
            Step.Read(
                "小数与百分比计算",
                "qalc -t '1920/1080'; qalc -t '200 * 15%'; qalc -t 'sqrt(2)'",
                "`-t` 只输出结果。").Then("依次输出 `1.777777778`、`30`、`1.414213562`。"),
            Step.Read(
                "单位换算",
                "qalc -t '5 GiB to MB'; qalc -t '60 km/h to m/s'; qalc -t '(1.5 h + 45 min) to min'; qalc -t '10 GB / 50 MB/s'",
                "最后一个计算的是按 50 MB/s 传输 10 GB 需要多久。").Then("依次输出 `5368.70912 MB`、`16.66666667 m/s`、`135 min`、`3 min + 20 s`。"),
            Step.Read(
                "进制转换",
                "qalc -t '255 to hex'; qalc -t '0x1F'; qalc -t '0b1010'",
                "`0x` 十六进制、`0b` 二进制。").Then("依次输出 `0xFF`、`31`、`10`。"),
            Step.Read(
                "按最新汇率换算",
                "qalc -e -t '100 USD to CNY'",
                "`-e` 先联网更新汇率。").Then("联网片刻后输出 `CNY 6xx.xx`；不加 `-e` 时结果基于本地保存的旧汇率，可能差别较大。"),
        ],

        Verify =
        [
            "1. `date -d @$(date +%s)` 输出的就是当前时间。",
            "2. `qalc -t '1 d to s'` 输出 `86400 s`。",
        ],
    };
}
