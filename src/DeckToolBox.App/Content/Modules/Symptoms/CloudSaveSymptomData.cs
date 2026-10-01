using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class CloudSaveSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-cloud-save",
        Title = "存档没同步或提示云冲突",
        Keywords = "云存档 · 同步失败 · 云冲突 · 存档丢失 · cloud_log · remotestorage",
        Level = ModuleLevel.Intermediate,
        Overview = "在电脑上玩过的进度在 Deck 上没有，或者启动游戏时弹出「云同步冲突」。先确认这个游戏有没有在同步、最近的同步结果如何，再决定保留哪一边的存档；必要时可以从网页下载云端的存档。",

        Why =
        [
            "1. **另一台设备上没有完成上传**：退出游戏后立即关机、断网，存档只留在那台设备上。",
            "2. **两边都有改动**：离线时在两台设备上都玩过，Steam 无法判断以哪边为准，就会提示冲突。",
            "3. **同步本身失败**：网络问题或 Steam 服务暂时异常，日志中会有 `Failed`。",
            "4. **游戏不支持 Steam 云**：游戏商店页没有「Steam 云」标识时，存档只在本地。",
        ],

        Consequences =
        [
            "• 下面的检查只读。",
            "• 处理冲突时选错一边会覆盖掉另一边的进度。不确定时先把本地存档目录复制一份再选择。",
        ],

        Steps =
        [
            Step.Read(
                "这个游戏最近的同步记录",
                "grep -h '\\[AppID <AppID>\\]' ~/.local/share/Steam/logs/cloud_log.txt | tail -n 15",
                "`<AppID>` 查法见「游戏打不开或启动后闪退」第 1 步。").Then("**完全没有记录** → 这个游戏可能不使用 Steam 云。**最后是 `Successfully synced to ChangeNumber ...`** → 同步正常，进度差异来自另一台设备没有上传。**出现 `Failed`** → 同步失败，见下一步。"),
            Step.Read(
                "最近的同步失败",
                "grep -iE 'conflict|fail' ~/.local/share/Steam/logs/cloud_log.txt | tail -n 8",
                "列出所有游戏最近的失败与冲突记录。").Then("没有输出说明最近没有失败。否则每行含 AppID 与失败环节（如 `Failed sync for 'down,'` 表示下载阶段失败），检查网络后重启 Steam 再试。"),
            Step.Read(
                "本地的云存档文件",
                "ls -la ~/.local/share/Steam/userdata/*/<AppID>/remote/",
                "这里只有一部分游戏的存档；其余游戏的存档在 Proton 前缀或游戏自己的目录中（见「Steam 目录结构与关键文件」）。").Then("列出存档文件及修改时间，可以据此判断本地存档是不是最新的。"),
        ],

        Verify =
        [
            "• 退出游戏后，第 1 步的最后一条是 `Successfully synced to ChangeNumber ...`。",
            "• 需要找回旧进度时，在浏览器中登录 `store.steampowered.com/account/remotestorage` 下载云端文件（见「云存档、局域网传输与游戏录制」）。",
        ],

        Related = ["steam-network-cloud", "steam-directories", "wine-proton", "archives-backup"],
    };
}
