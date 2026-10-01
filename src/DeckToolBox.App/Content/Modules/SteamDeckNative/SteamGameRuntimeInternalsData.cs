using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamGameRuntimeInternalsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steam-runtime-internals",
        Title = "着色器缓存与预编译",
        Keywords = "Fossilize · fossilize_replay · shadercache · .foz · mesa_shader_cache_sf · DXVK_state_cache · shader_log",
        Level = ModuleLevel.Advanced,
        Overview = "启动游戏前偶尔出现的「正在处理 Vulkan 着色器」、`shadercache` 目录为什么能占好几 GB、删掉后会怎样，都与 Steam 的着色器预缓存有关。本模块说明 Fossilize 记录、Valve 下发预编译结果、本机后台编译这几个环节，以及每个游戏的缓存目录里各个文件的用途。",

        Why =
        [
            "1. 为什么会卡顿：游戏第一次用到某个渲染效果时，显卡驱动要把着色器编译成 GPU 指令。如果在游戏进行中现场编译，就会出现短暂的卡顿 (stutter)。经 DXVK、VKD3D-Proton 转换的 Windows 游戏尤其明显。",
            "2. Fossilize 记录：Steam 给游戏加载一个 Vulkan 层（`ubuntu12_64/libVkLayer_steam_fossilize.so`），把游戏创建的每个渲染管线的描述记录到 `.foz` 文件中。记录的是「描述」而不是编译结果，因此可以在别的机器上重放。",
            "3. 共享与下发：Steam 把各玩家记录的管线汇总，按显卡与驱动分组。日志里的 `AMD RADV VANGOGH` 就是 Deck 这一组。由于所有 Deck 的硬件相同，Valve 可以直接下发已经编译好的结果，日志中记为 `Found precompiled fossilize bucket`。",
            "4. 本机编译：没有现成结果的部分，由 Steam 运行 `fossilize_replay` 重放 `.foz` 中的管线，让 Mesa 的 RADV 驱动（使用 ACO 编译器）提前编译。结果写入 Mesa 的磁盘缓存，游戏运行时直接命中，不再现场编译。启动游戏前的「正在处理 Vulkan 着色器」就是这一步；允许后台处理时，它也会在空闲时进行。",
            "5. 每个游戏的缓存目录 `steamapps/shadercache/<AppID>/`：",
            "- `fozpipelinesv6/`：管线记录。`steam_pipeline_cache.foz` 来自 Steam 下发，`steamapp_pipeline_cache.foz` 是本机记录的。",
            "- `mesa_shader_cache_sf/`：Mesa 编译出的 GPU 代码。Steam 运行游戏时把 Mesa 的缓存目录指向这里，因此每个游戏独立一份，删掉一个游戏的缓存不影响其他游戏。",
            "- `DXVK_state_cache/`：DXVK 自己的状态缓存。",
            "- `transcoded_video.foz` 或 `fozmediav1/`：部分 Proton 游戏的过场视频转码结果（见「Wine 与 Proton 原理」）。",
            "- 非 Steam 程序和桌面程序的 Mesa 缓存在 `~/.cache/mesa_shader_cache*`。",
        ],

        Consequences =
        [
            "• 查看缓存与日志都是只读的。",
            "• 删除某个游戏的 `shadercache` 目录不会损坏游戏或存档，只是下次启动前要重新下载或编译，最初一段时间可能出现卡顿。清理方法见「缓存、日志与存储清理」。",
            "• 「设置 → 下载」中关闭着色器预缓存后，Steam 不再下载和编译这些数据，缓存占用会变小，但游戏中的编译卡顿会更多。",
        ],

        Steps =
        [
            Step.Read(
                "找出占用最大的着色器缓存",
                "du -sh ~/.local/share/Steam/steamapps/shadercache/*/* 2>/dev/null | sort -h | tail -n 10",
                "按子目录统计，能看出大的是管线记录、编译结果还是转码视频。目录名中的数字是 AppID。").Then("每行「大小  路径」，如 `325M  .../shadercache/1293830/fozpipelinesv6`、`558M  .../shadercache/1293830/mesa_shader_cache_sf`、`514M  .../shadercache/1227690/transcoded_video.foz`。"),
            Step.Read(
                "查看某个游戏的缓存目录",
                "ls -la ~/.local/share/Steam/steamapps/shadercache/<AppID>/ ~/.local/share/Steam/steamapps/shadercache/<AppID>/fozpipelinesv6/",
                "`<AppID>` 取上一步路径中的数字。").Then("第一部分列出 `fozpipelinesv6`、`mesa_shader_cache_sf`、`DXVK_state_cache` 等子目录；第二部分是 `steam_pipeline_cache.foz`、`steamapp_pipeline_cache.foz`、`steam_pipeline_cache_whitelist.foz` 三个文件及其大小、日期。"),
            Step.Read(
                "查看某个游戏的着色器处理记录",
                "grep -h 'AppID <AppID>' ~/.local/share/Steam/logs/shader_log.txt | tail -n 8",
                "`shader_log.txt` 记录下载与处理着色器缓存的过程。").Then("最近几条记录，如 `[AppID 1293830] Finished Shader Swarm download job: No Error`、`Processing downloaded cache for AppID 1293830, BucketID ...`、`CProcessShaderCacheJob done for AppID 1293830.`；还有 `Found precompiled fossilize bucket for AppID AMD RADV VANGOGH / ...` 这样的预编译记录。"),
            Step.Read(
                "查看是否正在后台编译着色器",
                "pgrep -af fossilize_replay | cut -c1-200",
                "在「正在处理 Vulkan 着色器」期间，或 Steam 空闲时执行。`cut` 截断很长的命令行。").Then("正在编译时列出一个或多个 `fossilize_replay` 进程，命令行中有对应游戏的 `.foz` 文件路径，CPU 占用较高；没有编译任务时没有输出。"),
            Step.Read(
                "查看桌面程序与非 Steam 程序的 Mesa 缓存",
                "du -sh ~/.cache/mesa_shader_cache* 2>/dev/null",
                "这些缓存与 Steam 游戏无关，删除后同样会自动重建。").Then("一到三行，如 `mesa_shader_cache`、`mesa_shader_cache_db`、`mesa_shader_cache_sf` 及各自大小。"),
        ],

        Verify =
        [
            "1. `ls ~/.local/share/Steam/ubuntu12_64/ | grep -i fossil` 输出 `fossilize_replay` 与 `libVkLayer_steam_fossilize.so`。",
            "2. 启动一个最近更新过的游戏前若出现「正在处理 Vulkan 着色器」，同时执行 `pgrep -af fossilize_replay` 能看到编译进程。",
        ],
    };
}
