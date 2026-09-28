using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Process;

public static class PerProcessIoData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-nethogs-iotop",
        Title = "nethogs、iotop、powertop",
        Keywords = "按进程看网速 · 磁盘读写 · 耗电",
        Level = ModuleLevel.Intermediate,
        Overview = "top 告诉你谁在占 CPU，但「谁在偷偷占网速」「谁在疯狂写硬盘」「谁让电池掉得快」需要另外三个工具：nethogs 按进程统计网速，iotop 按进程统计磁盘读写，powertop 估算各程序与设备的耗电。三者 SteamOS 都自带。",

        Why =
        [
            "1. nethogs：按进程显示网络上传、下载速度。",
            "• SteamOS 为它设置了文件权能（`cap_net_admin`、`cap_net_raw` 等），普通用户可以直接运行。",
            "• 交互界面中 `m` 切换单位（KB/s、KB、B、MB），`r` 按接收排序、`s` 按发送排序，`q` 退出。",
            "• `-t` 以文本模式持续输出，`-c N` 刷新 N 次后退出，便于记录。",
            "2. iotop：按进程显示磁盘读写速度，需要 root（`sudo iotop`）。",
            "• `-o` 只显示正在读写的进程，`-a` 显示累计量而不是速度，`-b -n 3` 批处理输出 3 次。",
            "• Steam 下载游戏、着色器编译、索引服务都可能在后台大量写盘。",
            "3. powertop：估算各进程、设备的耗电和唤醒次数，需要 root。",
            "• 仅在**使用电池**时估算才有意义，并且需要运行几十秒采集数据。",
            "• `Tab` 切换页面：概览（各进程耗电估计）、空闲状态、频率、设备统计、可调项 (Tunables)。",
            "• `--html=报告.html` 生成一份可以用浏览器查看的报告。",
        ],

        Consequences =
        [
            "• 三者都只做统计，不改变系统设置。",
            "• powertop 的「Tunables」页面可以按回车切换省电设置，SteamOS 的电源管理由系统服务负责，不建议在这里修改；另外 `powertop --auto-tune` 会一次性应用所有省电设置，可能影响 USB 设备、Wi-Fi 的稳定性，不要随意运行。",
        ],

        Steps =
        [
            Step.Read(
                "实时查看各进程的网速",
                "nethogs wlan0",
                "按 `m` 切换单位，`q` 退出。").Then("全屏表格每秒刷新：`PID`、`USER`、`PROGRAM`、`DEV`、`SENT`、`RECEIVED`，正在上传或下载的程序排在最前，底部是总计。"),
            Step.Read(
                "以文本方式记录几次采样",
                "timeout 8 nethogs -t -c 3 wlan0",
                "`-t` 文本输出，`-c 3` 刷新 3 次后退出。").Then("每次采样输出 `Refreshing:` 和若干行「程序/PID/UID	发送KB/s	接收KB/s」。"),
            Step.Root(
                "查看正在读写磁盘的进程",
                "sudo iotop -o",
                "`-o` 只显示有读写活动的进程，`q` 退出。").Then("顶部显示总读写速度，下方列出进程的 `DISK READ`、`DISK WRITE` 和 `IO>` 百分比（等待 IO 的时间占比）。"),
            Step.Root(
                "统计一段时间内的磁盘读写总量",
                "sudo iotop -o -a -b -n 5 -d 2",
                "`-a` 累计量，`-b` 批处理，`-n 5 -d 2` 每 2 秒一次共 5 次。").Then("连续输出 5 次快照，累计读写量最大的进程排在前面，适合找出持续写盘的后台程序。"),
            Step.Root(
                "估算耗电（拔掉电源后运行）",
                "sudo powertop",
                "等待约 30 秒让数据稳定，`Tab` 切换页面，`q` 退出。").Then("概览页顶部显示放电功率，下面按「估计功耗」列出进程、中断和设备，唤醒次数多的程序通常更耗电。"),
            Step.Root(
                "生成耗电报告",
                "sudo powertop --time=30 --html=/tmp/powertop.html && xdg-open /tmp/powertop.html",
                "采集 30 秒后生成 HTML 报告并用默认浏览器打开。").Then("采集结束后浏览器打开报告，各项数据以表格形式分页展示。"),
        ],

        Verify =
        [
            "1. 在 Steam 中开始下载，`nethogs` 中 `steam` 相关进程的 RECEIVED 列明显增大。",
        ],
    };
}
