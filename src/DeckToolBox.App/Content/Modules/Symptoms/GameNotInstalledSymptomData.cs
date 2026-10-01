using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class GameNotInstalledSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-game-update-stuck",
        Title = "游戏显示未安装或更新卡住",
        Keywords = "未安装 · 更新卡住 · 下载失败 · appmanifest · StateFlags · content_log",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏文件明明在，库里却显示「安装」按钮；或者更新一直排队、停在某个百分比。Steam 靠每个游戏的安装清单判断状态，下载过程则记录在日志里，从这两处入手。",

        Why =
        [
            "1. **显示未安装**：游戏所在库的 `steamapps/` 下缺少 `appmanifest_<AppID>.acf`，或者这个库（如 SD 卡）当前没有挂载、没有被 Steam 识别为库。",
            "2. **更新卡住**：常见原因是磁盘空间不足、游戏或其他程序还在运行占用文件、网络中断，或下载到一半的文件损坏。",
        ],

        Consequences =
        [
            "• 下面的检查都是只读的。",
            "• 不要手动删除或编辑 `appmanifest_*.acf`；需要时用 Steam 的「验证游戏文件完整性」修复。",
        ],

        Steps =
        [
            Step.Read(
                "所有库是否都在、都被 Steam 识别",
                "grep '\"path\"' ~/.local/share/Steam/steamapps/libraryfolders.vdf; findmnt -l -o TARGET | grep /run/media",
                "前一条是 Steam 记录的库，后一条是当前挂载的外置存储。").Then("**如果某个库路径（如 `/run/media/deck/<卷标>`）在第一部分出现、第二部分却没有** → 那张卡或那块盘没有挂载，装在上面的游戏就会显示未安装，见「插上 SD 卡或移动硬盘没反应」。"),
            Step.Read(
                "哪些游戏的状态不正常",
                "for f in ~/.local/share/Steam/steamapps/appmanifest_*.acf /run/media/deck/*/steamapps/appmanifest_*.acf; do awk -F'\"' '$2==\"appid\"{a=$4} $2==\"name\"{n=$4} $2==\"StateFlags\"{s=$4} END{if (s != 4) printf \"%-9s %-8s %s\\n\", a, s, n}' \"$f\"; done 2>/dev/null",
                "只列出 `StateFlags` 不是 `4`（已完整安装）的游戏。").Then("没有输出说明所有已安装游戏状态正常。否则每行「AppID  状态值  名称」：`6` 表示有更新待下载，`1026` 表示更新已开始但没完成，其他值的含义见「Steam 目录结构与关键文件」。"),
            Step.Read(
                "下载日志里的最近状态",
                "grep -E 'AppID [0-9]+ (update|state changed|scheduler finished|update canceled)' ~/.local/share/Steam/logs/content_log.txt | tail -n 10",
                "只保留与更新状态有关的行。").Then("最近 10 条，如 `AppID 1493710 state changed : Fully Installed,Update Queued,`、`scheduler finished : removed from schedule (result No Error, ...)`。**`result` 后面不是 `No Error`**（如磁盘空间不足、文件被锁定）就是更新失败的原因。"),
        ],

        Verify =
        [
            "• 第 2 步没有输出，Steam 库中游戏显示「开始游戏」。",
        ],

        Related = ["steam-directories", "sym-disk-full", "sym-drive-not-mounted", "steamos-commands"],
    };
}
