using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamNetworkAndCloudData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steam-network-cloud",
        Title = "云存档、局域网传输与游戏录制",
        Keywords = "Steam Cloud · cloud_log · remotestorage · 27036 · 局域网传输 · Remote Play · gamerecordings",
        Level = ModuleLevel.Intermediate,
        Overview = "存档没同步、提示云冲突、局域网里另一台电脑明明装着游戏 Deck 却还在从外网下载、游戏录制突然停止……这些都能在 Steam 的日志和几个固定位置找到线索。本模块说明云同步的过程、在哪里看同步记录、怎样从网页下载云端存档，以及局域网传输、串流与录制相关的端口和文件。",

        Why =
        [
            "1. 云同步的过程：",
            "- 启动游戏前：Steam 比较本地的 `remotecache.vdf` 与云端的变更号，云端更新时先下载，再让游戏启动。",
            "- 退出游戏后：Steam 扫描存档文件，把有变化的上传。两边都有改动、无法判断以哪边为准时，弹出「云同步冲突」对话框让你选择。",
            "- 过程记录在 `~/.local/share/Steam/logs/cloud_log.txt`，每行带 `[AppID xxx]`：`Starting sync`、`File is in sync`（与云端一致）、`Persisting file`（扫描到需要同步的文件）、`Skipping un-modified file`（未改动，跳过）、`Upload complete`（上传完成）、`Successfully synced to ChangeNumber`（同步完成）等。",
            "2. 网页上的云存档：在浏览器中登录 `store.steampowered.com/account/remotestorage`，能看到每个游戏保存在云端的文件并单独下载。本地存档损坏、又不想让损坏的版本覆盖云端时，可以从这里取回。",
            "3. 局域网游戏传输：在「设置 → 下载」中允许局域网传输后，同一局域网内登录同一账号（或家庭组）且装有该游戏的电脑，可以直接把游戏文件传给 Deck，不经过外网。Steam 用 27036 端口在局域网内发现其他客户端，传输本身另行建立连接。",
            "4. Remote Play 串流：Deck 作为接收端时，解码由 APU 的视频单元完成；串流中按快捷键或在串流设置里打开性能信息，可以看到实时的延迟与带宽。Steam 用 UDP 27031~27036 与 TCP 27036 传输串流数据。",
            "5. 游戏录制：Steam 的录制功能默认把录像存放在 `userdata/<账号ID>/gamerecordings/`，录制与串流的事件记在 `logs/streaming_log.txt`。磁盘空间不足时录制会自动停止，日志中有 `Not enough disk space, stopping recording`。",
        ],

        Consequences =
        [
            "• 查看日志、端口与录制目录都是只读的。",
            "• 用网页下载的存档覆盖本地之前，先备份现有的存档目录；覆盖后启动游戏时 Steam 可能报告云冲突，此时选择保留本地版本。",
            "• 游戏录制会持续占用空间，`/home` 空间紧张时应在录制设置中限制占用或关闭后台录制。",
        ],

        Steps =
        [
            Step.Read(
                "查看某个游戏最近的云同步记录",
                "grep -h '\\[AppID <AppID>\\]' ~/.local/share/Steam/logs/cloud_log.txt | tail -n 15",
                "`<AppID>` 为游戏的数字编号（查法见「Steam 目录结构与关键文件」）。方括号前的反斜杠让 grep 按字面匹配。").Then("最近 15 条记录，如 `[AppID 413150] File is in sync StardewValley/Saves/...`、`Eval complete`、`YldWriteCacheDirectoryToFile - saved to .../remotecache.vdf`；上传过的会有 `Upload complete`，失败时出现带 `Failed` 的行。"),
            Step.Read(
                "统计云同步日志里出现过的问题",
                "grep -iE 'conflict|fail|error' ~/.local/share/Steam/logs/cloud_log.txt | tail -n 10",
                "只挑出含冲突、失败、错误字样的行。").Then("没有输出说明最近的同步都正常；否则列出出错的记录，如 `[AppID 2371090] BYieldingBuildFileListToSync Failed for 'down,'`，据此找到出问题的游戏。"),
            Step.Read(
                "打开 Steam 云存档网页",
                "xdg-open 'https://store.steampowered.com/account/remotestorage'",
                "在默认浏览器中打开，需要登录 Steam 账号。").Then("浏览器打开「Steam 云」页面，列出每个使用云存档的游戏；点「显示文件」可看到文件名、大小、修改时间，并逐个下载。"),
            Step.Read(
                "查看 Steam 在监听哪些网络端口",
                "ss -tulpn | grep -i steam",
                "`-tulpn`：TCP、UDP、监听中、显示进程、数字端口。").Then("几行结果：`udp 0.0.0.0:27036` 与 `tcp 0.0.0.0:27036`（局域网发现与串流），若干 `127.0.0.1:<随机端口>`（只供本机程序连接），以及 `steamwebhelper` 的 `224.0.0.251:5353`（局域网 mDNS）。"),
            Step.Read(
                "查看局域网传输是否被使用",
                "grep -iE 'lan peer|local network|peer' ~/.local/share/Steam/logs/content_log.txt | tail -n 10",
                "在局域网内另一台电脑开着 Steam、并装有同一游戏的情况下下载后执行。").Then("使用了局域网传输时，有从局域网地址获取内容的记录；没有输出说明下载都来自 Steam 的内容服务器（日志中的 `steamcontent.com`）。"),
            Step.Read(
                "查看游戏录制的位置、占用与最近事件",
                "du -sh ~/.local/share/Steam/userdata/*/gamerecordings; grep 'Game Recording' ~/.local/share/Steam/logs/streaming_log.txt | tail -n 5",
                "在录制设置中改过保存位置时，第一条统计的不是实际位置。").Then("先输出录制目录的大小；再是最近 5 条录制事件，如 `Game Recording - game stopped [gameid=...]`、`Game Recording - would start recording game ..., but recording for this game is disabled`，空间不足时有 `Not enough disk space, stopping recording`。"),
        ],

        Verify =
        [
            "1. 退出一个支持云存档的游戏后，`cloud_log.txt` 中该游戏的最后一条是 `Successfully synced to ChangeNumber ...`。",
            "2. `ss -tulpn | grep -c 27036` 输出不小于 1，说明 Steam 正在局域网中提供发现服务。",
        ],
    };
}
