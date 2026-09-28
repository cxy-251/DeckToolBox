using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.LaunchAndIntegration;

public static class LaunchOptionsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "launch_options",
        Title = "启动参数与环境变量注入 (Launch Options)",
        Subtitle = "启动与游戏模式接入",
        CategoryName = "启动与游戏模式接入",
        Badge = "调优关键",
        Overview = "玩转 Steam 属性面板的“启动选项”机制，掌握 %command% 宏语法与核心调优环境变量清单。",

        WhyCard = new InfoCardItem(
            "📌",
            "Steam 启动选项 %command% 宏语法与工作原理",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "在 Steam 游戏的【属性】 -> 【通用】面板中，最下方有一个【启动选项 (Launch Options)】输入框。这是 Linux 玩家最强大的调谐入口：",
                "1. 前置注入规则 (环境变量)：VAR=value %command%",
                "• %command% 代表 Steam 原本要执行的全部命令（包含 Proton 运行时、容器命令与主程序 exe）。写在它前面的 VAR=value 会作为进程环境变量直接注入给游戏！",
                "2. 包装器调用规则 (Wrapper)：tool %command%",
                "• 如 mangohud %command%，代表先启动监控性能工具，再由该工具拉起游戏自身。",
                "3. 后置参数规则 (应用自身参数)：%command% --flag",
                "• 写在 %command% 后面的参数，会被作为命令行传参直接送给游戏本体的 Main(string[] args)。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "💡",
            "启动选项对运行稳定性的决定性作用",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 救砖神器：很多黑屏无法启动的游戏，只需加一个环境变量（如 LD_PRELOAD=\"\" 或 PROTON_NO_ESYNC=1）就能立即起死回生。",
                "• 隔离保证：使用独立 STEAM_COMPAT_DATA_PATH 可以防止游戏更新时把自己的 Mod 补丁或特殊注册表覆盖丢掉。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "规避 Steam Overlay 干扰（解决独立软件黑屏卡死）", "LD_PRELOAD=\"\" %command%", "添加非 Steam 浏览器、网盘客户端或工具软件时，防止被覆盖层钩子注入导致崩溃。"),
            new(2, "重定向至专属独立 WINEPREFIX 虚拟 C 盘容器", "STEAM_COMPAT_DATA_PATH=\"/home/deck/.local/share/my_custom_pfx\" %command%", "多版本 Mod 测试、避免 Steam 自动重装时冲掉默认存档的最佳解法。"),
            new(3, "Electron / Chromium 类软件沙箱跳过参数", "%command% --no-sandbox --disable-gpu-sandbox", "微云、战网或基于 Web 技术的应用在 Wine/Proton 容器下黑屏时的特效解药。"),
            new(4, "Unity 游戏关闭音频子系统（极速启动与节电）", "%command% -nosound", "让 Unity 底层 FMOD 完全不初始化，避免持续流式读取磁盘音频包。")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "验证环境变量是否已生效",
            "#238636",
            "#3FB950",
            new[]
            {
                "当程序启动后，在终端中查出该进程的 PID，然后执行：",
                "cat /proc/<PID>/environ | tr '\\0' '\\n' | grep '你的变量名'",
                "即可在 Linux 内核层确认环境变量已经 100% 成功注入到目标进程！"
            }
        )
    };
}
