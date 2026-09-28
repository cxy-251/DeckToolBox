using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.LaunchAndIntegration;

public static class GameModeIntegrationData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "non-steam-games",
        Title = "非 Steam 程序接入游戏模式",
        Keywords = "shortcuts.vdf · 启动脚本",
        Level = ModuleLevel.Basic,
        Overview = "将 ~/Applications 下的便携应用与脚本无缝挂载至 Steam 游戏模式，享受手柄大屏与全局 Steam Input 键位映射。",

        Why =
        [
            "1. 掌机游戏模式的独立性：SteamOS 的掌机模式不加载 KDE 桌面组件与桌面菜单。要在掌机主界面运行应用，必须注册进 Steam 客户端自身的数据库中。",
            "2. 底层存储文件：shortcuts.vdf：Steam 将所有非 Steam 快捷方式集中保存在 ~/.local/share/Steam/userdata/<用户ID>/config/shortcuts.vdf 二进制文件中。包含 AppID、路径、启动参数、图标等元信息。",
            "3. 接入游戏模式的三大红利：",
            "• 独享 Gamescope 合成器加速：享受掌机系统级的 FSR 超分辨率与锁帧。",
            "• 强大的 Steam Input：直接为任意非 Steam 软件配置触摸板模拟鼠标、背键宏和陀螺仪准星。",
            "• 快速休眠与唤醒：支持掌机电源键一键无损休眠随时随地继续。",
        ],

        Consequences =
        [
            "• 覆盖层环境变量污染：从 Steam 启动应用时，Steam 会强制向子进程注入 LD_PRELOAD (Steam Overlay 钩子) 与 Vulkan 层。对于某些 Chromium / Electron 应用（如浏览器、编辑器），极易引发窗口黑屏或无响应卡死，需要用独立脚本 unset 清理。",
        ],

        Steps =
        [
            Step.Read(
                "查看本机 shortcuts.vdf 文件位置",
                "find ~/.local/share/Steam/userdata/ -name \"shortcuts.vdf\"",
                "确认当前 Steam 用户的非 Steam 快捷方式数据库物理路径。"),
            Step.Read(
                "通过 Steam 原生客户端界面添加（最安全规范）",
                "# 在 Steam 桌面客户端操作：\n# 1. 顶部菜单【游戏】 -> 【添加非 Steam 游戏到我的库】\n# 2. 点击【浏览】选择 ~/Applications 下的可执行程序或脚本\n# 3. 勾选并确认添加",
                "Steam 会自动为其计算专属 Non-Steam AppID 并完成封装。"),
            Step.Change(
                "编写防黑屏的标准 launch.sh 启动包装脚本",
                "cat <<'EOF' > ~/Applications/MyTool/launch.sh\n#!/usr/bin/env bash\nunset LD_PRELOAD\nunset LD_LIBRARY_PATH\nexport DISABLE_VK_LAYER_VALVE_steam_overlay_1=1\nexec \"/home/deck/Applications/MyTool/app_binary\" \"$@\"\nEOF\nchmod +x ~/Applications/MyTool/launch.sh",
                "在启动前主动清理覆盖层与注入层，保证应用原生干净拉起。"),
        ],

        Verify =
        [
            "在 Steam 库中找到新添加的应用，双击桌面图标“返回游戏模式 (Return to Gaming Mode)”，在掌机非 Steam 分类中点击即可直接畅玩启动！",
        ],
    };
}
