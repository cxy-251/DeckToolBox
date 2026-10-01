using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class GameWontStartSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-game-wont-start",
        Title = "游戏打不开或启动后闪退",
        Keywords = "闪退 · 打不开 · 黑屏 · 崩溃 · PROTON_LOG · 前缀 · 换 Proton",
        Level = ModuleLevel.Intermediate,
        Overview = "点击「开始游戏」后什么也没出现、黑屏一下就回到库，或者进游戏不久就退出。先确认游戏是不是真的被启动了、用的是哪个 Proton、有没有崩溃记录，再决定是换 Proton 版本、修复前缀还是看 Proton 日志。",

        Why =
        [
            "常见原因：",
            "1. **Proton 版本不合适**：新游戏可能需要 Proton Experimental，老游戏可能在新版本上出问题。这是最常见、也最容易尝试的一项。",
            "2. **前缀损坏或缺少运行库**：换过多个 Proton 版本、强制关机、Mod 改动过前缀之后容易出现。",
            "3. **游戏文件不完整**：更新中断或磁盘出错。Steam 的「验证游戏文件完整性」可以修复。",
            "4. **反作弊或 DRM 不支持 Linux**：这种情况换版本、改设置都无法解决，可以在 ProtonDB 查看其他玩家的反馈。",
            "5. **启动选项写错**：多余的包装器或变量会让游戏起不来，先清空启动选项排除。",
        ],

        Consequences =
        [
            "• 第 1~3 步只读。",
            "• 第 4 步只影响下一次启动，测试完记得删掉启动选项。",
            "• 删除前缀会丢失只保存在本地的存档，修复前缀前先看「Proton 前缀与运行库修复」中的备份步骤。",
        ],

        Steps =
        [
            Step.Read(
                "找到游戏的 AppID",
                "grep -H '\"name\"' ~/.local/share/Steam/steamapps/appmanifest_*.acf /run/media/deck/*/steamapps/appmanifest_*.acf 2>/dev/null | grep -i '<游戏名关键词>'",
                "`<游戏名关键词>` 换成游戏英文名的一部分。后面的步骤都要用到 AppID。").Then("一行，如 `.../appmanifest_1227690.acf:\t\"name\"\t\t\"Severed Steel\"`，文件名中的数字就是 AppID。"),
            Step.Read(
                "游戏是否真的被启动、用了哪个 Proton",
                "grep -A1 'StartSession: appID <AppID>' ~/.local/share/Steam/logs/compat_log.txt | tail -n 2; grep 'Game process .*AppID <AppID>' ~/.local/share/Steam/logs/console_log.txt | tail -n 2 | cut -c1-120",
                "先启动一次游戏再执行。前一条显示最近一次启动使用的兼容工具；后一条显示游戏进程何时开始、何时结束。").Then("如 `Tool 4628710 \"Proton 11.0-2\" has a dependency on tool 4183110.`，以及 `Game process added : AppID ...` 与几秒后的 `Game process removed`。**添加与移除只隔几秒** → 游戏启动后立即退出，继续第 3、4 步找原因。**根本没有记录** → 问题在启动之前（文件缺失、启动选项错误）。"),
            Step.Read(
                "有没有崩溃记录",
                "coredumpctl list --no-pager --since today | tail -n 5",
                "Proton 游戏的崩溃不一定都会产生记录，没有记录不代表没崩溃。").Then("若有与这次启动时间吻合、路径中含 `wine`、`proton` 或游戏 exe 的行，用 `coredumpctl info <PID>` 查看调用栈（见「gdb 与 coredumpctl」）。"),
            Step.Change(
                "让 Proton 写出详细日志",
                "PROTON_LOG=1 %command%",
                "填入游戏「属性 → 通用 → 启动选项」后启动一次游戏。日志写在家目录的 `steam-<AppID>.log`，查看末尾：`tail -n 40 ~/steam-<AppID>.log`。").On(Hosts.SteamLaunchOptions).Then("游戏照常启动（或照常失败），家目录出现 `steam-<AppID>.log`，可能有几十 MB。末尾的 `err:` 行通常指出缺少的 DLL、不支持的功能或崩溃位置；把这些行附在求助帖中最有用。测试完删掉启动选项。"),
        ],

        Verify =
        [
            "• 换一个 Proton 版本（游戏「属性 → 兼容性」）后再启动，第 2 步中的工具名随之改变，游戏能进入。",
            "• 仍不行时依次尝试：验证游戏文件完整性 → 备份存档后删除前缀重建 → 在 ProtonDB 查看该游戏的已知问题。",
        ],

        Related = ["wine-proton", "proton-versions", "proton-prefix", "launch-options", "man-gdb", "pressure-vessel"],
    };
}
