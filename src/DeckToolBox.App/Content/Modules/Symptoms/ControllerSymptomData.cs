using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class ControllerSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-controller",
        Title = "按键失灵、摇杆漂移、陀螺仪不准",
        Keywords = "手柄 · 按键失灵 · 摇杆漂移 · 陀螺仪 · 触控板 · Steam Input · 固件",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏里某个键没反应、角色自己往一个方向走、陀螺仪瞄准慢慢偏移。先分清是硬件还是配置：系统能不能看到控制器、固件是否正常；硬件层面的读数可以用 evtest 直接观察，配置层面多数问题出在 Steam Input 的按键布局上。",

        Why =
        [
            "1. **Steam Input 布局问题**：最常见。游戏使用了自定义或社区布局，某些键被映射成了别的功能。在游戏中按 STEAM 键 → 控制器设置，换回默认布局即可判断。",
            "2. **摇杆漂移**：摇杆在静止时仍有偏移读数。可以先在「设置 → 控制器」中校准、调大死区；硬件磨损则需要维修。",
            "3. **陀螺仪漂移**：校准数据不准，在控制器设置中重新校准陀螺仪（保持机器静止放在桌面上）。",
            "4. **控制器没有被正常接管**：Steam 未运行或崩溃时，控制器处于蜥蜴模式（触控板当鼠标），游戏中的手柄输入不起作用。",
        ],

        Consequences =
        [
            "• 下面的检查只读。",
            "• `evtest` 会独占读取输入设备，测试期间游戏收不到该设备的输入，按 `Ctrl+C` 结束即恢复。",
        ],

        Steps =
        [
            Step.Read(
                "系统能否看到内置控制器",
                "grep -l 'Steam Controller' /sys/class/hidraw/*/device/uevent",
                "内置控制器以几个 HID 接口的形式出现。").Then("输出几行 `/sys/class/hidraw/hidrawN/device/uevent`。**没有任何输出** → 控制器没有被识别，重启一次；仍然没有则可能是硬件问题。"),
            Step.Read(
                "控制器固件是否正常",
                "jupiter-controller-update --check | tail -n 2",
                "只检查不刷写。").Then("如 `NO UPDATE: Type 3 Device is running latest build ...` 与 `needs_update=0`。**`needs_update=1`** → 有固件更新，系统更新时会自动刷写，确认已安装最新的系统更新。"),
            Step.Read(
                "陀螺仪的校准数据",
                "cat ~/.local/share/Steam/config/*_gyro.vdf",
                "零点漂移 (`gyro_drift_per_sample_*`) 的绝对值明显偏大时，重新校准通常能改善。").Then("一个 `\"gyro_data\"` 块，含三个轴的漂移值与噪声容限。"),
            Step.Read(
                "直接观察摇杆的原始读数",
                "evtest",
                "列出输入设备后输入 `Steam Deck` 对应的编号回车，然后松开摇杆观察；按 `Ctrl+C` 结束。").Manual().Then("先列出所有输入设备，选择后持续滚动事件。**松手时 `ABS_X`/`ABS_Y` 等摇杆数值仍在大幅变化** → 摇杆漂移是硬件层面的；只在很小范围内跳动属于正常，用死区即可消除。"),
        ],

        Verify =
        [
            "• 在游戏中切换到默认布局后按键正常，说明原来的问题出在布局上。",
            "• 摇杆校准、陀螺仪校准之后静止时不再有漂移。",
        ],

        Related = ["deck-input", "man-evtest", "steamos-commands"],
    };
}
