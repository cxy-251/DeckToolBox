using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class DiskFullSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-disk-full",
        Title = "存储空间不够了",
        Keywords = "空间不足 · 磁盘满 · 清理 · df · du · 着色器缓存 · 回收站",
        Level = ModuleLevel.Basic,
        Overview = "游戏下载失败、提示空间不足，或者录制、更新突然停止。先确认是哪个分区满了，再找出是什么占了空间。Deck 上几乎所有数据都在 `/home` 分区，所以通常要看的是它。",

        Why =
        [
            "常见的占用来源：",
            "1. **游戏本体**：`steamapps/common/` 下的游戏，几十 GB 一个很常见。",
            "2. **可以安全清理的缓存**：着色器缓存 (`shadercache`)、回收站、`~/.cache` 中各程序的缓存。",
            "3. **已卸载游戏留下的 Proton 前缀** (`compatdata`)：其中可能有本地存档，删除前要确认。",
            "4. **游戏录制、Distrobox/podman 容器与镜像、下载目录中的安装包**。",
            "5. 系统分区 `/` 满了的情况很少见，通常是解除只读后往 `/usr` 装了东西，或者系统更新出错。",
        ],

        Consequences =
        [
            "• 下面的检查都是只读的。",
            "• 清理时先删可重建的缓存；`compatdata` 与游戏录制删除前确认里面没有需要保留的东西。",
        ],

        Steps =
        [
            Step.Read(
                "哪个分区快满了",
                "df -h /home / /var | sed 1d",
                "`/home` 是数据分区，`/` 是只读的系统分区，`/var` 是很小的系统数据分区。").Then("三行，每行依次是设备、总容量、已用、可用、使用率、挂载点。**`/home` 的使用率在 90% 以上** → 继续下面的步骤。**`/` 或 `/var` 满了** → 见「只读系统与 A/B 更新」与「缓存、日志与存储清理」中关于系统日志的部分。"),
            Step.Read(
                "几个常见的大户各占多少",
                "du -sh ~/.local/share/Steam/steamapps/{common,shadercache,compatdata} ~/.local/share/Trash ~/.cache ~/.local/share/containers ~/Downloads 2>/dev/null | sort -h",
                "依次是游戏本体、着色器缓存、Proton 前缀、回收站、程序缓存、Distrobox/podman 容器、下载目录。统计可能需要十几秒。").Then("每行「大小  路径」，从小到大排列。**回收站、`.cache` 或 `shadercache` 占了几个 GB** → 可以放心清理，见「缓存、日志与存储清理」。**`containers` 很大** → 见「podman：管理容器与镜像空间」。**`common` 最大** → 卸载不玩的游戏，或把游戏移到 SD 卡。"),
            Step.Read(
                "已卸载的游戏是否留下了前缀",
                "cd ~/.local/share/Steam/steamapps && for d in compatdata/*/; do id=$(basename \"$d\"); [ -n \"$(ls ~/.local/share/Steam/steamapps/appmanifest_$id.acf /run/media/deck/*/steamapps/appmanifest_$id.acf 2>/dev/null)\" ] || du -sh \"$d\"; done 2>/dev/null | sort -h | tail -n 10",
                "在内置存储和所有 SD 卡库中都找不到安装清单的前缀才会列出。非 Steam 程序的前缀（10 位数字）也会列出，它们并不是残留，不要误删。").Then("每行「大小  compatdata/<AppID>/」。数字较短的大多是已卸载游戏的残留；删除前先看「Wine 与 Proton 原理」中的存档位置，把需要的存档备份出来。"),
        ],

        Verify =
        [
            "• 清理后再执行第 1 步，`/home` 的可用空间增加。",
            "• Steam「设置 → 存储空间」中显示的可用空间同步变化。",
        ],

        Related = ["storage-cleanup", "steam-directories", "steam-runtime-internals", "man-tree-du", "man-podman", "steam-network-cloud"],
    };
}
