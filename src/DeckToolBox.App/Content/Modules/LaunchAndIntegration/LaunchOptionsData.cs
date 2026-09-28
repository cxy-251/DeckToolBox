using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.LaunchAndIntegration;

public static class LaunchOptionsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "launch-options",
        Title = "Steam 启动选项",
        Keywords = "%command% · 环境变量 · 包装器",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏「属性 → 通用 → 启动选项」是调整游戏运行方式的入口。掌握 %command% 的三种写法，就能看懂网上各种启动选项的含义，而不是照抄。",

        Why =
        [
            "1. `%command%` 是什么：Steam 原本要执行的完整命令（包括 Steam Runtime 容器、Proton 和游戏本体）。启动选项就是围绕它拼出最终命令行。",
            "2. 三种写法：",
            "- `变量=值 %command%`：在前面设置**环境变量**，传给整条命令链，例如 `PROTON_LOG=1 %command%`。",
            "- `程序 %command%`：用一个**包装器**启动游戏，例如 `mangohud %command%`，由 MangoHud 先启动再拉起游戏。",
            "- `%command% 参数`：写在后面的是**传给游戏本体的参数**，例如 `%command% -dx11`；游戏是否认识该参数取决于游戏本身。",
            "3. 三种写法可以组合：`变量=值 包装器 %command% 参数`。",
            "4. 如果启动选项里没有 `%command%`，Steam 会把整段内容当作追加在末尾的游戏参数。",
        ],

        Consequences =
        [
            "• 启动选项只影响这一个游戏，清空输入框即可恢复默认。",
            "• 很多启动选项只对特定引擎或特定游戏有效，照抄前先确认它适用于你的游戏（ProtonDB 的玩家报告是好的参考）。",
            "• 调试用的选项（如 `PROTON_LOG=1`）排查完要删掉，否则每次启动都会写大量日志。",
        ],

        Steps =
        [
            Step.Change(
                "用 MangoHud 显示帧率、温度与功耗",
                "mangohud %command%",
                "SteamOS 自带 MangoHud。在 Deck 游戏模式中，快捷菜单的「性能浮层」就是 MangoHud，因此这条主要用于桌面模式。").On(Hosts.SteamLaunchOptions).Then("游戏画面左上角出现半透明的信息浮层，实时显示 FPS、帧时间曲线、CPU/GPU 占用与温度。删掉启动选项即恢复。"),
            Step.Change(
                "禁用 Steam 覆盖层的注入",
                "LD_PRELOAD=\"\" %command%",
                "`LD_PRELOAD` 是预先加载的库列表，设为空即不注入任何库。个别原生 Linux 程序与覆盖层冲突时使用。").On(Hosts.SteamLaunchOptions).Then("游戏启动时不再加载 Steam 覆盖层：按 `Shift+Tab` 不会弹出覆盖层界面，由它引起的黑屏或崩溃随之消失。"),
            Step.Change(
                "让 DXVK 限制帧率",
                "DXVK_FRAME_RATE=40 %command%",
                "只对 D3D9~11 游戏生效。在 Deck 上通常用快捷菜单限帧即可。").On(Hosts.SteamLaunchOptions).Then("游戏帧率被限制在 40 FPS 以内，GPU 负载与功耗随之下降；对不走 DXVK 的游戏（D3D12、原生 Linux 游戏）没有效果。"),
            Step.Change(
                "强制游戏使用 DirectX 11 渲染路径",
                "%command% -dx11",
                "常见于提供 D3D11/D3D12 双路径的游戏（如部分 Unreal 引擎游戏），在 D3D12 下不稳定时尝试。").On(Hosts.SteamLaunchOptions).Then("支持该参数的游戏会跳过 D3D12，改用 D3D11 渲染（此时经由 DXVK 翻译）；不支持的游戏忽略这个参数，照常启动。"),
            Step.Read(
                "查看正在运行的游戏实际收到的环境变量",
                "tr '\\0' '\\n' < /proc/$(pgrep -n -f '<游戏exe名>')/environ | grep -E 'PROTON|DXVK|LD_PRELOAD'",
                "`pgrep -n -f` 找到名字匹配的最新进程，`/proc/<PID>/environ` 是它的环境变量（以空字符分隔，`tr` 转成换行）。游戏运行时在桌面模式终端执行。").Then("每行一个变量，例如 `DXVK_FRAME_RATE=40`；没有输出说明游戏进程里没有这些变量，启动选项可能没生效，或者 exe 名写错了。"),
        ],

        Verify =
        [
            "1. 启动带 `mangohud` 的游戏后画面左上角出现浮层。",
            "2. 最后一步能看到你设置的变量，例如 `DXVK_FRAME_RATE=40`。",
        ],
    };
}
