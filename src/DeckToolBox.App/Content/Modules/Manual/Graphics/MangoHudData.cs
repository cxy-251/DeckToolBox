using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Graphics;

public static class MangoHudData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-mangohud",
        Title = "MangoHud、gamemode 与 vkmark",
        Keywords = "MANGOHUD_CONFIG · MangoHud.conf · 日志 · gamemoderun · vkmark",
        Level = ModuleLevel.Intermediate,
        Overview = "MangoHud 在游戏画面上叠加帧率、帧时间、温度、功耗等信息，还能把数据记录到文件用于对比；gamemode 在游戏运行期间临时调整系统设置；vkmark 是一个简单的 Vulkan 性能测试。三者 SteamOS 都自带。",

        Why =
        [
            "1. MangoHud 的两种使用场景：",
            "• 游戏模式：快捷菜单 → 性能 → 性能浮层级别 1~4，由 Steam 通过 Gamescope 控制，无需任何配置。",
            "• 桌面模式或其他 Linux：`mangohud 程序` 或 Steam 启动选项 `mangohud %command%`，显示内容由配置决定。",
            "2. 配置方式（桌面模式）：",
            "- 环境变量：`MANGOHUD_CONFIG=fps,frametime,gpu_temp,cpu_temp,ram,vram` 临时指定。",
            "- 配置文件：`~/.config/MangoHud/MangoHud.conf`，每行一个选项，对所有程序生效；`~/.config/MangoHud/<程序名>.conf` 只对某个程序生效。",
            "- 常用选项：`fps_limit=40` 限帧、`position=top-right` 位置、`font_size=20`、`toggle_hud=Shift_R+F12` 显示/隐藏快捷键。",
            "- 记录：`output_folder=~/mangohud-logs`、`log_duration=60`，运行时按 `Shift_L+F2` 开始/停止记录，得到 CSV 文件。",
            "3. gamemode：`gamemoderun 程序` 在程序运行期间请求 CPU 使用性能调速策略、提高进程优先级等，程序退出后恢复。`gamemoded -s` 查看状态。在 Deck 上 SteamOS 自身已有功耗与调度管理，效果通常不明显，在其他 Linux 电脑上更有用。",
            "4. vkmark：运行一组 Vulkan 场景并给出分数，用于对比设置改动（TDP、GPU 频率、调度器）前后的图形性能。",
        ],

        Consequences =
        [
            "• 浮层本身会占用少量性能，测量精确的帧时间时注意这一点。",
            "• 配置文件对之后启动的所有 MangoHud 程序生效，需要恢复默认时删除该文件即可。",
            "• vkmark 会让 GPU 满载运行一两分钟，机身发热、风扇加速属正常。",
        ],

        Steps =
        [
            Step.Read(
                "给一个程序加上浮层",
                "MANGOHUD_CONFIG=fps,frametime,gpu_temp,cpu_temp,gpu_power mangohud vkcube",
                "在桌面模式中运行，关闭窗口结束。").Then("弹出旋转立方体窗口，左上角浮层显示帧率、帧时间曲线、GPU 与 CPU 温度、GPU 功耗。"),
            Step.Change(
                "写一个全局 MangoHud 配置",
                "mkdir -p ~/.config/MangoHud && cat > ~/.config/MangoHud/MangoHud.conf <<'EOF'\nfps\nframetime\ngpu_temp\ncpu_temp\nram\nvram\nposition=top-right\nfps_limit=60\ntoggle_hud=Shift_R+F12\noutput_folder=/home/deck/mangohud-logs\nEOF",
                "之后所有以 mangohud 启动的程序都使用这份配置。").Then("没有输出；再运行 `mangohud vkcube`，浮层出现在右上角，帧率被限制在 60，按右 Shift+F12 可以隐藏。"),
            Step.Change(
                "记录性能数据",
                "mkdir -p ~/mangohud-logs && mangohud vkcube",
                "在窗口中按左 Shift+F2 开始记录，过一会儿再按一次停止，然后关闭窗口。").Then("`~/mangohud-logs` 中生成以程序名和时间命名的 CSV 文件，包含每一帧的帧时间、CPU/GPU 负载与温度。"),
            Step.Read(
                "查看 gamemode 状态",
                "gamemoded -s",
                "没有游戏通过 gamemode 运行时为 inactive。").Then("输出 `gamemode is inactive`；用 `gamemoderun` 启动程序期间再执行则显示 `active`。"),
            Step.Read(
                "以 gamemode 运行程序",
                "gamemoderun vkcube",
                "Steam 启动选项中写作 `gamemoderun %command%`。").Then("程序正常运行，在另一个终端执行 `gamemoded -s` 显示 `gamemode is active`，关闭程序后恢复 inactive。"),
            Step.Read(
                "运行 Vulkan 性能测试",
                "vkmark",
                "持续一两分钟，窗口中依次显示几个测试场景。").Then("每个场景结束时终端输出一行 `FPS` 与帧时间，最后一行 `vkmark Score: xxxx`。改动设置前后各跑一次即可对比。"),
        ],

        Verify =
        [
            "1. `ls ~/mangohud-logs` 能看到记录的 CSV 文件，`head` 可以查看其中的列名。",
            "2. 不再需要自定义配置时执行 `rm ~/.config/MangoHud/MangoHud.conf`。",
        ],
    };
}
