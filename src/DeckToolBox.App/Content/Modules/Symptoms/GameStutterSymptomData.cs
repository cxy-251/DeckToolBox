using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class GameStutterSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-game-stutter",
        Title = "游戏卡顿、掉帧",
        Keywords = "卡顿 · 掉帧 · 帧率低 · 着色器 · TDP · 温度 · 降频 · PSI",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏帧率比预期低，或者平时流畅、偶尔卡一下。先分清是「持续偏低」还是「偶发卡顿」：前者多与功耗上限、温度和画质设置有关，后者常见于着色器编译、内存不足或后台任务。下面的检查在游戏运行时于桌面模式终端执行（或通过 SSH 从另一台电脑执行）。",

        Why =
        [
            "1. **功耗上限 (TDP) 或 GPU 频率被限制**：快捷菜单里把 TDP 或 GPU 频率调低后忘了改回来。",
            "2. **温度过高导致降频**：散热口被挡住、环境温度高时，APU 会降低频率。",
            "3. **着色器编译**：首次进入新场景时现场编译着色器，造成短暂卡顿；关闭着色器预缓存或缓存被删后更明显。",
            "4. **内存不足**：游戏与后台程序（浏览器等）合计用满 16 GB 内存后，系统开始频繁换页。",
            "5. **帧率限制与刷新率**：快捷菜单里设了帧率上限，或刷新率设得较低。",
        ],

        Consequences =
        [
            "• 下面都是只读检查，不改变任何设置。",
        ],

        Steps =
        [
            Step.Read(
                "当前功耗上限与 GPU 频率设置",
                "steamosctl get-tdp-limit; steamosctl get-gpu-performance-level",
                "两项都可以在快捷菜单「性能」中调整。").Then("如 `TDP limit: 15`、`GPU performance level: auto`。**TDP 明显低于 15，或 GPU 级别是 `low`/`manual`** → 很可能就是帧率低的原因，在快捷菜单中改回默认。"),
            Step.Read(
                "温度与实际功耗",
                "sensors | grep -E 'edge|PPT|Fan|temp1'",
                "`edge` 是 APU 温度，`slowPPT`/`fastPPT` 是当前功耗与上限，`Fan` 是风扇转速。").Then("游戏中 `edge` 在 90 °C 以下、风扇几千转属于正常。**温度接近 100 °C 且帧率下降** → 散热问题，检查出风口是否被挡住。"),
            Step.Read(
                "系统是否在等内存、CPU 或磁盘",
                "grep some /proc/pressure/{cpu,memory,io}",
                "压力停顿信息 (PSI)：最近 10 秒内任务因等待资源而停顿的时间比例。").Then("三行，如 `/proc/pressure/memory:some avg10=1.43 avg60=0.42 ...`。**`memory` 的 `avg10` 超过 10** → 内存不够，关掉后台程序。**`io` 很高** → 在大量读写磁盘（如下载、着色器处理）。CPU 高是玩游戏时的正常现象。"),
            Step.Read(
                "是否正在后台编译着色器",
                "pgrep -af fossilize_replay | cut -c1-150",
                "Steam 在后台编译着色器时会占用 CPU。").Then("**有输出** → 正在编译，等它完成后卡顿会减少。没有输出则排除这一项。"),
        ],

        Verify =
        [
            "• 恢复默认 TDP 与 GPU 设置、温度正常后，用性能浮层（快捷菜单 → 性能 → 性能浮层级别）观察帧率与帧时间曲线是否平稳。",
        ],

        Related = ["deck-hardware-ec", "steamosctl", "steam-runtime-internals", "gamescope", "memory-oom", "man-mangohud", "perf-tracing"],
    };
}
