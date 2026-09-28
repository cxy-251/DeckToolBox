using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Systemd;

public static class JournalctlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-journalctl",
        Title = "journalctl：查询日志",
        Keywords = "-u · -b · -p · --since · -f · -g · -o · --disk-usage",
        Level = ModuleLevel.Basic,
        Overview = "journalctl 查询 systemd 收集的全部日志：内核、系统服务、用户程序。它能按服务、时间、级别、关键词组合过滤，比在一堆日志文件里翻找高效得多。",

        Why =
        [
            "1. 按来源过滤：",
            "- `-u 服务`：某个系统服务；`--user -u 服务`：某个用户服务。",
            "- `-k`：只看内核消息（相当于 dmesg）。",
            "- `_PID=1234`、`_COMM=程序名`：按进程过滤。",
            "2. 按时间过滤：",
            "- `-b`：本次开机；`-b -1`：上一次开机；`--list-boots` 列出所有保留的开机记录。",
            "- `--since '2026-09-28 10:00'`、`--since '1 hour ago'`、`--since today`、`--until`。",
            "3. 按级别过滤：`-p err` 只看错误及更严重的；级别从严重到轻微为 `emerg`、`alert`、`crit`、`err`、`warning`、`notice`、`info`、`debug`。",
            "4. 按内容过滤：`-g 正则`（`--grep`）只显示消息内容匹配的条目。",
            "5. 显示方式：",
            "- `-n 50`：最后 50 条；`-r`：倒序（最新的在前）；`-f`：持续跟踪新日志。",
            "- `--no-pager`：直接输出；不加时进入分页界面（与 less 相同的按键）。",
            "- `-o short-iso`：ISO 格式时间；`-o cat`：只有消息内容；`-o json-pretty`：显示每条日志的全部字段。",
            "- `-q`：不显示提示信息。",
            "6. 空间管理：`--disk-usage` 查看占用；`sudo journalctl --vacuum-size=200M` 或 `--vacuum-time=2weeks` 清理旧日志。",
        ],

        Consequences =
        [
            "• 查询是只读的。",
            "• 普通用户只能看到自己的日志和部分系统日志，加 sudo 才能看全部。",
            "• 清理 (`--vacuum-*`) 删除的日志无法恢复。",
        ],

        Steps =
        [
            Step.Read(
                "最近 10 分钟的日志",
                "journalctl --since '10 min ago' --no-pager | tail -n 20",
                "时间可以写成自然语言。").Then("每行格式为 `月 日 时:分:秒 主机名 程序[PID]: 消息`，显示最后 20 条。"),
            Step.Read(
                "某个服务的日志",
                "journalctl -u NetworkManager -n 10 --no-pager",
                "`-u` 指定服务，`-n` 条数。").Then("只列出 NetworkManager 的最后 10 条日志，如 `state change`、`Activation: successful` 等连接过程。"),
            Step.Read(
                "某个用户服务的日志",
                "journalctl --user -u pipewire -n 10 --no-pager",
                "用户服务要加 `--user`。").Then("显示 PipeWire 音频服务的最近日志；若没有异常，可能只有很少几条。"),
            Step.Read(
                "本次开机的错误",
                "journalctl -b -p err -q --no-pager | tail -n 20",
                "`-q` 省略提示行。").Then("显示本次开机以来最后 20 条错误级别日志。"),
            Step.Read(
                "按关键词搜索",
                "journalctl -b -g 'wlan0' -n 10 --no-pager",
                "`-g` 在消息内容中按正则搜索。").Then("只显示提到 wlan0 的日志，如无线网卡的连接、断开事件。"),
            Step.Read(
                "查看一条日志的全部字段",
                "journalctl -n 1 -o json-pretty --no-pager",
                "每条日志除了消息，还带有进程、用户、单元等元数据。").Then("以 JSON 格式列出最新一条日志的所有字段，如 `_PID`、`_EXE`、`_SYSTEMD_UNIT`、`MESSAGE`；这些字段名都可以用作过滤条件（如 `_EXE=/usr/bin/xxx`）。"),
            Step.Read(
                "实时跟踪某个服务",
                "journalctl -f -u bluetooth",
                "此时连接或断开蓝牙设备，观察新日志；`Ctrl+C` 退出。").Then("先显示最近几条，然后停住等待；每次蓝牙状态变化都会立即出现新的日志行。"),
            Step.Read(
                "查看保留了哪些开机记录与日志占用",
                "journalctl --list-boots --no-pager | tail -n 5; journalctl --disk-usage",
                "`IDX` 为 0 的是本次开机，-1 是上一次。").Then("列出最近几次开机的编号、ID 和时间范围，最后一行是日志占用的磁盘空间。"),
        ],

        Verify =
        [
            "1. `journalctl -u NetworkManager -n 1 -o cat` 只输出一条纯文本消息，没有时间和主机名。",
        ],
    };
}
