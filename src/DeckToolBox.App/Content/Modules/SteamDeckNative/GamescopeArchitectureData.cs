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
            "5. HDR 与色彩管理：",
            "• 游戏通过 Gamescope 的 Vulkan WSI 层提交 HDR 画面（HDR10 PQ 或 scRGB）及亮度元数据。",
            "• Gamescope 从屏幕的 EDID 读取它支持的 HDR 能力（OLED 版内置屏的 EDID 中有 HDR 静态元数据块，最高亮度约 600 cd/m²）。屏幕支持时按 HDR 输出；不支持或未开启时，HDR 画面被色调映射成 SDR。",
            "• 在 HDR 输出下，普通 SDR 内容按多亮显示由 `--hdr-sdr-content-nits` 决定（默认 400 nits）；`--hdr-itm-enabled` 可以把 SDR 画面反向映射成 HDR。",
            "6. 游戏模式会话的几个细节：",
            "- 游戏模式由用户服务 `gamescope-session.service` 运行脚本 `/usr/lib/steamos/gamescope-session`；脚本把当时的全部环境变量保存到 `$XDG_RUNTIME_DIR/gamescope-environment`（即 `/run/user/1000/`），其中 `STEAM_GAMESCOPE_*` 告诉 Steam 这台设备支持哪些功能（VRR、HDR、撕裂、NIS 等），`STEAM_DISPLAY_REFRESH_LIMITS` 是内置屏允许的刷新率范围。",
            "- 会话结束后，drop-in 配置 `drm_janitor.conf` 通过 `ExecStopPost` 运行 `/usr/bin/drm_janitor`，清理 Gamescope 留下的显示状态，再交给桌面模式或下一次会话。",
        ],

        Consequences =
        [
            "• 查看 DRM 连接器、屏幕模式与 EDID 均为只读操作，不会改变硬件状态。",
            "• 在桌面模式用 `gamescope` 嵌套运行程序，只会打开一个独立窗口，关闭即结束，对系统没有影响，适合测试程序在 1280×800 下的显示效果。",
            "• 在游戏模式中给游戏的启动选项再套一层 `gamescope`，会形成嵌套合成器，通常没有必要，缩放和限帧请直接用快捷菜单。启动选项里的 gamescope 主要用于桌面模式或其他 Linux 电脑。",
            "• 启动选项里 `--` 分隔 gamescope 参数和要运行的命令，必须放在 `%command%` 之前。",
        ],

        Steps =
        [
            Step.Read(
                "查看 gamescope 的参数说明",
                "gamescope --help 2>&1 | head -n 40",
                "常用：`-w/-h` 游戏渲染分辨率，`-W/-H` 输出分辨率，`-r` 刷新率，`-F fsr|nis|integer|linear` 缩放滤镜，`-f` 全屏。").Then("输出前 40 行帮助，按「Options」「Nested mode options」「Embedded mode options」分组列出参数。只显示帮助，不会启动合成器。"),
            Step.Read(
                "查看显示连接器及其是否接有屏幕",
                "for c in /sys/class/drm/card*-*/; do echo \"$(basename $c): $(cat $c/status)\"; done",
                "`eDP-1` 是内置屏，`DP-1` 是 USB-C 外接显示器。").Then("每行一个接口，如 `card0-eDP-1: connected`、`card0-DP-1: disconnected`。"),
            Step.Read(
                "查看内置屏支持的模式",
                "cat /sys/class/drm/card*-eDP-1/modes",
                "系统在显示时把它旋转成横屏。").Then("输出 `800x1280`：宽 800、高 1280，说明面板物理上是竖屏。"),
            Step.Read(
                "查看屏幕 EDID 色深与 HDR 元数据",
                "edid-decode /sys/class/drm/card0-eDP-1/edid | grep -iE 'Manufacturer|Bits per primary|HDR Static|luminance'",
                "EDID 是屏幕自己报告的能力表，`edid-decode` 把它解码成可读文本。外接显示器把 `eDP-1` 换成 `DP-1`。").Then("OLED 版输出如 `Manufacturer: VLV`、`Bits per primary color channel: 8`、`HDR Static Metadata Data Block:`，以及 `Desired content max luminance: 115 (603.666 cd/m^2)` 等三行亮度；LCD 版没有 HDR 相关的行。"),
            Step.Read(
                "查看 gamescope 的 HDR 与色彩管理参数",
                "gamescope --help 2>&1 | grep -i hdr",
                "这些参数主要用于桌面模式或其他电脑上手动运行 gamescope；游戏模式中的 HDR 由 Steam 设置控制。").Then("十行左右的参数说明，如 `--hdr-enabled  enable HDR output ...`、`--hdr-sdr-content-nits  ... Default: 400 nits.`、`--hdr-itm-enabled  enable SDR->HDR inverse tone mapping`。"),
            Step.Read(
                "查看当前是否有 gamescope 在运行",
                "pgrep -a gamescope || echo '当前没有运行 gamescope（桌面模式下属正常）'",
                "`||` 表示前一条命令没找到时才执行后一条。").Then("游戏模式下输出 gamescope 的 PID 和完整启动参数；桌面模式下输出「当前没有运行 gamescope（桌面模式下属正常）」。"),
            Step.Change(
                "在桌面模式用 gamescope 窗口运行一个测试程序",
                "gamescope -W 1280 -H 800 -w 640 -h 400 -F fsr -- vkcube",
                "vkcube 以 640×400 渲染并用 FSR 放大到 1280×800 窗口。").Then("桌面上弹出一个 1280×800 的窗口，里面是一个旋转的彩色立方体。关闭窗口或在终端按 `Ctrl+C` 结束。"),
            Step.Change(
                "启动选项模板：以 720p 渲染并用 FSR 放大、限制 40 FPS",
                "gamescope -W 1280 -H 800 -w 1152 -h 720 -F fsr -r 40 -f -- %command%",
                "用于桌面模式或其他 Linux 电脑上的 Steam。在 Deck 的游戏模式中请改用快捷菜单的缩放和帧率设置。").On(Hosts.SteamLaunchOptions).Then("游戏在一个全屏的 gamescope 会话中运行：以 1152×720 渲染、放大到 1280×800，帧率不超过 40。退出游戏时 gamescope 一并退出。"),
            Step.Read(
                "查看 Gamescope 动态特性环境表",
                "grep -E 'STEAM_GAMESCOPE|STEAM_DISPLAY|VRS' \"$XDG_RUNTIME_DIR/gamescope-environment\"",
                "进入过一次游戏模式后才会有这个文件，回到桌面模式后仍然保留。").Then("十几行 `变量=值`，如 `STEAM_GAMESCOPE_VRR_SUPPORTED=1`、`STEAM_GAMESCOPE_HDR_SUPPORTED=1`、`STEAM_GAMESCOPE_TEARING_SUPPORTED=1`、`STEAM_DISPLAY_REFRESH_LIMITS=45,90`（OLED 版内置屏 45~90 Hz）、`STEAM_USE_DYNAMIC_VRS=1`。"),
            Step.Read(
                "查看游戏模式会话的服务配置",
                "systemctl --user cat gamescope-session.service --no-pager | grep -E '^# /|^Exec'",
                "`systemctl cat` 显示服务文件及其所有 drop-in 片段，`# /` 开头的行标出每个片段的路径。").Then("几行结果：服务文件路径、`ExecStart=/usr/lib/steamos/gamescope-session`，以及 `drm_janitor.conf` 片段中的 `ExecStopPost=-/usr/bin/drm_janitor`。"),
        ],

        Verify =
        [
            "1. 执行嵌套示例后应弹出一个 1280×800 的窗口，其中方块旋转显示，边缘因 FSR 锐化而清晰。",
            "2. 游戏模式中：快捷菜单 → 性能 → 打开「性能浮层」，把帧率限制设为 40，浮层中的帧率稳定在 40、帧时间曲线接近一条直线，说明限帧生效。",
        ],
    };
}
