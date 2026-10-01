using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamDirectoryHierarchyData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steam-directories",
        Title = "Steam 目录结构与关键文件",
        Keywords = "steamapps · appmanifest · StateFlags · libraryfolders.vdf · userdata · remotecache · workshop",
        Level = ModuleLevel.Basic,
        Overview = "Steam 的全部数据都在 `~/.local/share/Steam` 下。知道游戏本体、安装清单、Proton 前缀、着色器缓存、云存档、创意工坊和日志各放在哪里、由哪个文件记录，就能自己判断「游戏显示未安装」「更新卡住」「存档在哪」这类问题，也知道哪些可以删、哪些不能动。",

        Why =
        [
            "1. 根目录 `~/.local/share/Steam`（`~/.steam/root` 是指向它的链接）：",
            "- `steamapps/common/<游戏目录>/`：游戏本体。",
            "- `steamapps/appmanifest_<AppID>.acf`：每个已安装游戏一个清单，记录名称、安装目录名 (`installdir`)、版本号 (`buildid`)、占用空间与状态。Steam 靠它判断游戏是否已安装：**清单丢了，即使文件都在，游戏也显示为未安装**。",
            "- `steamapps/compatdata/<AppID>/`：Proton 前缀；`steamapps/shadercache/<AppID>/`：着色器缓存（见「着色器缓存与预编译」）。",
            "- `steamapps/workshop/`：创意工坊。`appworkshop_<AppID>.acf` 列出该游戏已下载的物品，物品内容在 `content/<AppID>/<物品ID>/`。",
            "- `steamapps/libraryfolders.vdf`：所有游戏库的位置（内置存储、SD 卡、移动硬盘）及各自装了哪些游戏。SD 卡上的库是卡根目录下的 `steamapps/`。",
            "- `userdata/<账号ID>/`：每个登录过的账号一个目录（账号 ID 是 SteamID 的后 32 位）。其中 `<AppID>/remote/` 是云存档的本地副本，`<AppID>/remotecache.vdf` 记录每个云文件的大小、时间、SHA-1 与同步状态，`config/shortcuts.vdf` 是非 Steam 快捷方式，`gamerecordings/` 是游戏录制。",
            "- `config/`：客户端配置，如 `config.vdf`（含各游戏强制使用的兼容工具）、`loginusers.vdf`（登录过的账号）。",
            "- `logs/`：客户端日志，如下载的 `content_log.txt`、云同步的 `cloud_log.txt`、着色器的 `shader_log.txt`、启动命令所在的 `console_log.txt`。",
            "2. `StateFlags`：清单中的状态是若干二进制位相加：`4` 已完整安装，`2` 需要更新，`512` 更新已暂停，`1024` 更新已开始，`1048576` 下载中等。正常的已安装游戏是 `4`；`6` 表示已安装但有更新待下载；`1026` 表示需要更新且更新已开始（常见于排队或卡住的更新）。",
            "3. 云存档不全在 `remote/`：`remotecache.vdf` 中每个文件有一个 `root` 值，`0` 表示存在 `remote/` 下；其他值表示文件在游戏自己的位置（例如 Proton 前缀里的 `AppData` 或 `Documents`），由 Steam 的自动云同步按规则收集。",
        ],

        Consequences =
        [
            "• 查看目录和清单都是只读的。",
            "• 不要删除或手改 `appmanifest_*.acf`：游戏会变成「未安装」，需要重新下载或让 Steam 重新识别。",
            "• 删除 `userdata/<账号ID>/<AppID>/remote/` 会丢掉尚未同步到云端的存档。",
            "• `shadercache`、`logs` 可以删除（会重新生成）；`compatdata` 中有本地存档，删除前先备份。",
        ],

        Steps =
        [
            Step.Read(
                "查看 Steam 目录中各部分的占用",
                "du -sh ~/.local/share/Steam/* 2>/dev/null | sort -h | tail -n 10",
                "`sort -h` 按带单位的大小排序，`tail` 取最大的 10 项。统计整个目录需要十几秒。").Then("每行「大小  路径」，最大的通常是 `steamapps`（游戏、前缀、缓存都在里面），其次可能是 `userdata`（含录制）和 `appcache`。"),
            Step.Read(
                "列出所有游戏库的位置",
                "grep -E '\"(path|label)\"' ~/.local/share/Steam/steamapps/libraryfolders.vdf",
                "每个库有 `path`（路径）和 `label`（卷标，内置存储为空）。").Then("成对输出，如 `\"path\" \"/home/deck/.local/share/Steam\"`、`\"label\" \"\"`，以及 SD 卡的 `\"path\" \"/run/media/deck/<卷标>\"`、`\"label\" \"<卷标>\"`。"),
            Step.Read(
                "汇总所有已装游戏：AppID、状态、占用与名称",
                "for f in ~/.local/share/Steam/steamapps/appmanifest_*.acf /run/media/deck/*/steamapps/appmanifest_*.acf; do awk -F'\"' '$2==\"appid\"{a=$4} $2==\"name\"{n=$4} $2==\"StateFlags\"{s=$4} $2==\"SizeOnDisk\"{z=$4} END{printf \"%-9s %-8s %6.1f GB  %s\\n\", a, s, z/1e9, n}' \"$f\"; done 2>/dev/null | sort -k3 -n",
                "`awk -F'\"'` 以双引号分隔，第 2 段是键名、第 4 段是值；按占用从小到大排序。工具（Proton、运行时）也在列表中。").Then("每个游戏一行，如 `1293830   4         101.5 GB  Forza Horizon 4`。第二列不是 `4` 的，就是有待更新或状态异常的游戏。"),
            Step.Read(
                "查看某个游戏的安装清单",
                "grep -E '\"(appid|name|installdir|buildid|StateFlags|SizeOnDisk|LastUpdated)\"' ~/.local/share/Steam/steamapps/appmanifest_<AppID>.acf",
                "`<AppID>` 取上一步第一列。游戏装在 SD 卡上时把路径换成 `/run/media/deck/<卷标>/steamapps/`。").Then("七行左右，如 `\"name\" \"Terraria\"`、`\"installdir\" \"Terraria\"`（即 `common/` 下的目录名）、`\"buildid\" \"...\"`、`\"StateFlags\" \"4\"`、`\"SizeOnDisk\" \"...\"`（字节）。"),
            Step.Read(
                "查看某个游戏的创意工坊物品",
                "grep -E '\"(NeedsUpdate|NeedsDownload)\"' ~/.local/share/Steam/steamapps/workshop/appworkshop_<AppID>.acf; du -sh ~/.local/share/Steam/steamapps/workshop/content/<AppID>/*",
                "物品 ID 就是 `content/<AppID>/` 下的目录名，可在 `steamcommunity.com/sharedfiles/filedetails/?id=<物品ID>` 查看对应的创意工坊页面。").Then("先输出 `\"NeedsUpdate\" \"0\"`、`\"NeedsDownload\" \"0\"`（都为 0 表示已是最新）；然后每个订阅物品一行「大小  目录」。"),
            Step.Read(
                "查看某个游戏的云存档文件",
                "ls -la ~/.local/share/Steam/userdata/*/<AppID>/remote/; grep -E '^\\s*\"[^\"]+\"$|\"(root|size|syncstate)\"' ~/.local/share/Steam/userdata/*/<AppID>/remotecache.vdf | tr -s '\\t' ' ' | head -n 24",
                "第一条列出 `remote/` 下的存档；第二条从同步清单中挑出文件名、`root`、大小与同步状态。").Then("先列出 `remote/` 下的存档文件；再按文件分组输出，如 `\"default1.aoescn\"`、`\"root\" \"0\"`、`\"size\" \"161579\"`、`\"syncstate\" \"1\"`。`root` 不为 0 的文件不在 `remote/` 中，而在游戏自己的存档目录。"),
            Step.Read(
                "查看最近的下载日志",
                "grep -E 'AppID [0-9]+ ' ~/.local/share/Steam/logs/content_log.txt | tail -n 15",
                "日志中还有大量下载服务器的统计行，`grep` 只保留与具体游戏有关的。下载卡住、提示磁盘写入错误时先看这里。").Then("最近 15 条带时间戳的记录，如 `AppID 1493710 state changed : Fully Installed,Update Queued,`、`AppID 1493710 scheduler finished : removed from schedule (result No Error, state 0xc)`；出错时 `result` 后面是错误名称（如磁盘空间不足）。"),
        ],

        Verify =
        [
            "1. `readlink -f ~/.steam/root` 输出 `/home/deck/.local/share/Steam`。",
            "2. 「汇总所有已装游戏」的结果中，正常游戏的第二列都是 `4`。",
        ],
    };
}
