using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class SteamWontStartSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-steam-wont-start",
        Title = "Steam 打不开或反复重启",
        Keywords = "Steam 打不开 · 白屏 · 黑屏 · 反复重启 · 闪退 · 修复 · 日志",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏模式里一直转圈或黑屏、反复回到启动画面，桌面模式里点 Steam 没反应或界面空白。SteamOS 在 Steam 连续启动失败时会自动修复它；不起作用时，从日志找原因，或手动触发修复。",

        Why =
        [
            "1. **客户端文件损坏**：更新中断、磁盘写满时最常见。SteamOS 的自动修复（连续 3 次在 60 秒内退出后触发）就是为此设计的。",
            "2. **界面进程 (steamwebhelper) 出错**：主进程在运行但界面空白或黑屏，通常与显卡驱动、缓存有关。",
            "3. **磁盘空间不足**：Steam 无法写入配置与更新文件，见「存储空间不够了」。",
            "4. **网络问题**：启动时卡在更新或登录，见「连着 Wi-Fi 却上不了网」。",
        ],

        Consequences =
        [
            "• 第 1~3 步只读。",
            "• 第 4 步会让 Steam 重新下载客户端更新，并需要重新登录；游戏与存档不受影响。",
        ],

        Steps =
        [
            Step.Read(
                "Steam 进程是否在运行",
                "pgrep -a -x steam; pgrep -c steamwebhelper",
                "第一条找主进程，第二条统计界面进程数量。").Then("**两条都没有输出 / 输出 0** → Steam 没有运行或已经退出，看第 2 步日志。**主进程在、`steamwebhelper` 为 0** → 界面进程起不来，常见于界面黑屏或空白。"),
            Step.Read(
                "启动与更新日志的最后几行",
                "tail -n 8 ~/.local/share/Steam/logs/bootstrap_log.txt; echo ---; tail -n 8 ~/.local/share/Steam/logs/console_log.txt | cut -c1-150",
                "`bootstrap_log` 记录启动与自更新，`console_log` 是主日志。").Then("正常时 bootstrap 以 `Nothing to do` 或更新完成的记录结束。**最后几行是错误**（如磁盘写入失败、校验失败、下载失败）→ 据此判断是空间、文件损坏还是网络问题。"),
            Step.Read(
                "最近有没有连续启动失败",
                "cat /tmp/steamos-short-session-tracker 2>/dev/null | wc -l; ls -d ~/dot-steam.bak.* 2>/dev/null || echo '（没有自动修复的记录）'",
                "第一条是最近连续「60 秒内就退出」的次数，达到 3 次时下次启动会自动修复；第二条列出以往自动修复留下的备份。").Then("第一行是一个数字，通常为 `0`；第二部分列出备份目录或显示「没有自动修复的记录」。"),
            Step.Change(
                "手动触发 SteamOS 的修复",
                "steam -shutdown; sleep 10; /usr/lib/steamos/steam-short-session-tracker --repair-now",
                "在桌面模式中执行。备份 `~/.steam` 后，从系统自带的引导程序重新解压一份干净的 Steam。").Then("输出 `steam-short-session-tracker: Re-bootstrapping steam from OS copy.`。再次打开 Steam 时会先自我更新，然后要求重新登录。"),
        ],

        Verify =
        [
            "• Steam 正常进入库界面，`pgrep -c steamwebhelper` 输出大于 1。",
        ],

        Related = ["steam-architecture-dev", "sym-disk-full", "sym-no-internet", "steam-directories"],
    };
}
