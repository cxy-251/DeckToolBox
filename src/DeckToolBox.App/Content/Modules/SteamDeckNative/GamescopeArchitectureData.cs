using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class GamescopeArchitectureData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "gamescope",
        Title = "Gamescope 合成器与缩放",
        Keywords = "gamescope · FSR · 帧率限制",
        Level = ModuleLevel.Intermediate,
        Overview = "深入解析 Valve 专为掌机打造的 Gamescope 微型合成器，透视 DRM Lease 显卡硬件平面直通、纳秒级锁帧调度、FSR/NIS 画面缩放着色器与桌面嵌套运行。",

        Why =
        [
            "1. 摆脱传统桌面合成器的开销 (DRM Lease 独占直通)：",
            "• 在进入全屏游戏模式时，SteamOS 会彻底杀掉 KDE Plasma 桌面会话，由 Valve 自研的微型合成器 **Gamescope** 全权接管。",
            "• Gamescope 通过 Vulkan 扩展 `VK_EXT_acquire_drm_display` 直接向 Linux 内核 DRM 子系统索取显卡输出平面的独占租借权 (DRM Lease)。它不是一个画在窗口里的应用，它就是当前的“显卡硬件直连者”，延迟损耗降为 0！",
            "2. 锁帧黑科技：为什么 Gamescope 锁 40/45/60 帧输入延迟极低？",
            "• 很多游戏内置的 V-Sync（垂直同步）通过阻塞渲染线程来对齐刷新率，往往会引入 2~3 帧（33ms~50ms）的严重操控粘滞感与输入延迟。",
            "• Gamescope 在显示流水线末端通过控制内核级 KMS 硬件翻页定时 (`drmModePageFlip`) 与 Vulkan 呈现队列，实现精准的 **帧步调平滑 (Frame Pacing)**。以 40Hz 刷新率为例，帧生成时间平稳锁定在 25ms，比 30fps 流畅得多，却比 60fps 节省 30% 以上的 APU 功耗与发热！",
            "3. 驱动级通用超分辨率缩放 (FSR / NIS / Integer)：",
            "• 游戏只需以较低原生分辨率（如 960x600 或 720p）渲染，Gamescope 在最后一步利用 GPU 计算着色器，一视同仁地对画面进行 **AMD FSR 1.0 (空间超分辨率)**、**Nvidia Image Scaling (NIS)** 或像素级整数缩放，大幅提升老游戏或大作的掌机续航与帧率稳定度。",
        ],

        Consequences =
        [
            "• 桌面嵌套沙盒安全无害：在桌面模式下使用 `gamescope -w 1280 -h 800 -- <程序>` 仅会弹出一个独立的 Wayland 虚拟显示器窗口，用于测试游戏或工具在掌机分辨率下的 UI 适配，关闭窗口即安全退出，对系统毫无破坏性。",
            "• 启动项包装规范：在 Steam 启动项中配置 `gamescope` 时，需遵循严格的参数顺序（如 `gamescope -e -f -F fsr -- %command%`），其中的 `--` 必须置于游戏命令 `%command%` 之前。",
        ],

        Steps =
        [
            Step.Read(
                "查看 Gamescope 合成器支持的完整命令行控制选项 (CLI Reference)",
                "gamescope --help 2>&1 | head -n 30",
                "掌握核心参数：-w/-h (游戏渲染分辨率), -W/-H (最终输出显示分辨率), -F (放大算法: fsr/nis/integer/linear), -r (限制刷新率), -e (启用原生 Steam 快捷菜单集成)。"),
            Step.Read(
                "查看当前系统所有 DRM 显示连接器与输出管道状态",
                "ls -l /sys/class/drm/ | grep -E \"(card[0-9]-|render)\"",
                "查看物理屏幕连接器标识（如 eDP-1 为掌机内置显示屏，DP-1/HDMI-A-1 为外接扩展坞屏幕）。"),
            Step.Read(
                "查看内置显示屏的硬件物理分辨率、工作时钟与色彩深度",
                "cat /sys/class/drm/card*-eDP-1/modes 2>/dev/null || cat /sys/class/drm/card*-eDP-2/modes 2>/dev/null",
                "直接从内核读取掌机屏幕的物理 EDID 规格（LCD 版 1280x800 60Hz；OLED 版 1280x800 90Hz HDR）。"),
            Step.Read(
                "实战掌握在桌面模式下嵌套运行掌机沙盒的调试命令",
                "echo '测试嵌套运行: gamescope -w 1280 -h 800 -e -- glxgears'",
                "在桌面弹出独立 1280x800 窗口，使用掌机环境与分辨率无侵入调试任意软件。"),
            Step.Change(
                "实战掌握 Steam 启动项注入 FSR 超分辨率着色器的标准语法",
                "echo '启动项模板: gamescope -W 1280 -H 800 -w 960 -h 600 -F fsr --fsr-sharpness 2 -f -- %command%'",
                "以 960x600 极低负载渲染并由 GPU 硬件拉伸至 800p，在保证清晰度的同时大幅提升掌机续航与帧率稳定度。"),
        ],

        Verify =
        [
            "1. 在全屏游戏模式中，按下右侧【...】快速访问键打开性能侧边栏，将性能浮层级别调至 4 级 (MangoHud)。若开启了 FSR，界面会直接显示 `FSR: ON`，并且绿色帧生成时间线条呈一条水平直线，证实 Gamescope 帧步调平滑机制完美生效。",
            "2. 桌面模式进程审计：执行 `pgrep -a gamescope`，可清晰观察当前正在被 gamescope 包装运行的子进程树与分辨率配置。",
        ],
    };
}
