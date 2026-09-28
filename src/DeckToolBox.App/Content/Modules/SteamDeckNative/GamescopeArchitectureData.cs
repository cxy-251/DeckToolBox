using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class GamescopeArchitectureData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "gamescope",
        Title = "Gamescope 合成器与缩放",
        Keywords = "gamescope · FSR · 帧率限制 · 游戏模式",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏模式的整个画面由 Valve 的 Gamescope 合成器输出。快捷菜单里的帧率限制、刷新率、缩放滤镜（FSR / NIS / 整数缩放）都是它的功能。桌面模式下也能手动用它运行程序。",

        Why =
        [
            "1. 两种会话，两个合成器：",
            "• 桌面模式运行 KDE Plasma，合成器是 KWin（Wayland）。",
            "• 游戏模式不启动 Plasma，而是由 Gamescope 作为唯一的合成器，直接通过内核 DRM/KMS 驱动屏幕，Steam 界面和游戏都是它的客户端窗口。组件少、延迟低、行为可控。",
            "2. 帧率限制与刷新率：",
            "• Gamescope 掌握着每一帧何时送上屏幕，所以能在合成器层面平稳地限制帧率（例如 40 FPS 配合 40 Hz 刷新率，每帧恰好 25 ms），不需要游戏自己的垂直同步。",
            "• 相比游戏内的 V-Sync，这种限制通常输入延迟更低、帧间隔更均匀；把帧率压低还能显著降低功耗。",
            "3. 缩放：游戏以较低分辨率渲染（例如 960×600），由 Gamescope 用 GPU 放大到屏幕分辨率：",
            "- `FSR`：AMD FidelityFX 超分辨率 1.0，空间放大并锐化。",
            "- `NIS`：NVIDIA Image Scaling，同类算法。",
            "- 整数缩放：像素精确放大，适合像素风游戏。",
            "4. 屏幕是竖屏面板：Deck 的屏幕物理上是 800×1280 的竖屏，由系统旋转成横屏使用，所以内核报告的模式是 `800x1280`。",
        ],

        Consequences =
        [
            "• 查看 DRM 连接器与屏幕模式完全只读。",
            "• 在桌面模式用 `gamescope` 嵌套运行程序，只会打开一个独立窗口，关闭即结束，对系统没有影响，适合测试程序在 1280×800 下的显示效果。",
            "• 在游戏模式中给游戏的启动选项再套一层 `gamescope`，会形成嵌套合成器，通常没有必要，缩放和限帧请直接用快捷菜单。启动选项里的 gamescope 主要用于桌面模式或其他 Linux 电脑。",
            "• 启动选项里 `--` 分隔 gamescope 参数和要运行的命令，必须放在 `%command%` 之前。",
        ],

        Steps =
        [
            Step.Read(
                "查看 gamescope 的参数说明",
                "gamescope --help 2>&1 | head -n 40",
                "常用：`-w/-h` 游戏渲染分辨率，`-W/-H` 输出分辨率，`-r` 刷新率，`-F fsr|nis|integer|linear` 缩放滤镜，`-f` 全屏。"),
            Step.Read(
                "查看显示连接器及其是否接有屏幕",
                "for c in /sys/class/drm/card*-*/; do echo \"$(basename $c): $(cat $c/status)\"; done",
                "`eDP-1` 是内置屏，`DP-1` 是 USB-C 外接显示器。"),
            Step.Read(
                "查看内置屏支持的模式",
                "cat /sys/class/drm/card*-eDP-1/modes",
                "输出 `800x1280`，印证内置屏是竖屏面板。"),
            Step.Read(
                "查看当前是否有 gamescope 在运行",
                "pgrep -a gamescope || echo '当前没有运行 gamescope（桌面模式下属正常）'",
                "游戏模式下能看到 gamescope 及其启动参数。"),
            Step.Change(
                "在桌面模式用 gamescope 窗口运行一个测试程序",
                "gamescope -W 1280 -H 800 -w 640 -h 400 -F fsr -- vkcube",
                "vkcube 以 640×400 渲染并用 FSR 放大到 1280×800 窗口。关闭窗口即退出。"),
            Step.Change(
                "启动选项模板：以 720p 渲染并用 FSR 放大、限制 40 FPS",
                "gamescope -W 1280 -H 800 -w 1152 -h 720 -F fsr -r 40 -f -- %command%",
                "用于桌面模式或其他 Linux 电脑上的 Steam。在 Deck 的游戏模式中请改用快捷菜单的缩放和帧率设置。").On(Hosts.SteamLaunchOptions),
        ],

        Verify =
        [
            "1. 执行嵌套示例后应弹出一个 1280×800 的窗口，其中方块旋转显示，边缘因 FSR 锐化而清晰。",
            "2. 游戏模式中：快捷菜单 → 性能 → 打开「性能浮层」，把帧率限制设为 40，浮层中的帧率稳定在 40、帧时间曲线接近一条直线，说明限帧生效。",
        ],
    };
}
