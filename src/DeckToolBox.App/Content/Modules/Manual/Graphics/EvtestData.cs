using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Graphics;

public static class EvtestData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-evtest",
        Title = "evtest：测试手柄与输入设备",
        Keywords = "evtest · 按键码 · 摇杆轴 · 漂移 · 虚拟手柄",
        Level = ModuleLevel.Intermediate,
        Overview = "外接手柄某个键没反应、摇杆疑似漂移、背键到底发出了什么信号——evtest 直接读取内核输入事件，把每次按键、每个轴的数值实时打印出来，不经过 Steam 或游戏，可以判断问题出在硬件、驱动还是映射。",

        Why =
        [
            "1. 输入事件：每个输入设备对应 `/dev/input/eventN`。按下或松开一个键产生一个 `EV_KEY` 事件（值 1 按下、0 松开、2 长按重复）；摇杆、扳机产生 `EV_ABS` 事件，带有当前数值。",
            "2. `evtest` 不带参数时列出设备并让你选择编号；`evtest /dev/input/eventN` 直接测试指定设备。",
            "3. 输出开头列出该设备支持的所有事件类型、按键码与轴的取值范围（最小值、最大值、死区 `Flat`），随后实时打印事件。",
            "4. 权限：",
            "• 普通用户只能打开通过 `uaccess` 授权给你的设备（通常是手柄、游戏控制器）；evtest 启动时会提示「Not running as root, no devices may be available」。",
            "• 键盘等设备需要 `sudo evtest`。",
            "5. 在 Deck 上：Steam 运行时，内置手柄的信号经 Steam Input 处理后，由虚拟设备 `Microsoft X-Box 360 pad` 发出。测试它看到的是 Steam 映射后的结果；想看原始硬件信号，需要在桌面模式退出 Steam 后测试 hid-steam 驱动创建的设备。",
        ],

        Consequences =
        [
            "• evtest 只读取事件，不影响设备；`--grab` 选项会独占设备，其他程序在此期间收不到它的输入，不要随意使用。",
            "• 按 `Ctrl+C` 退出。",
        ],

        Steps =
        [
            Step.Read(
                "列出可访问的输入设备",
                "timeout 3 evtest < /dev/null",
                "`< /dev/null` 让它列出设备后不等待输入选择。").Then("先提示 `Not running as root, no devices may be available.`，然后列出你有权限访问的设备，如 `/dev/input/event10: Microsoft X-Box 360 pad 0`（Steam 的虚拟手柄）以及外接的蓝牙键鼠、手柄。列表之后会出现 `Select the device event number` 和一段 `USAGE` 用法说明，这是因为没有输入编号，属正常现象。"),
            Step.Read(
                "测试 Steam 虚拟手柄",
                "evtest /dev/input/$(grep -l 'X-Box 360 pad' /sys/class/input/event*/device/name | head -n 1 | xargs dirname | xargs dirname | xargs basename)",
                "自动找到虚拟手柄的 event 节点。按手柄上的键、推动摇杆观察输出，`Ctrl+C` 退出。").Then("先打印设备支持的按键与轴（如 `ABS_X` 范围 -32768~32767），然后每次操作打印一行，如 `type 1 (EV_KEY), code 304 (BTN_SOUTH), value 1`（按下 A 键）。"),
            Step.Read(
                "测试外接手柄（示例）",
                "evtest /dev/input/<事件节点>",
                "`<事件节点>` 如 `event18`，取自第一步的列表。").Then("操作手柄时打印对应事件；某个键按下时没有任何输出，说明信号在驱动层就没有到达。"),
            Step.Read(
                "检查摇杆是否漂移",
                "evtest /dev/input/<事件节点> | grep --line-buffered -E 'ABS_(X|Y|RX|RY)'",
                "测试开始后不要碰摇杆，观察输出。").Then("松手静止时若仍持续滚动大幅变化的 `ABS_X`/`ABS_Y` 数值，说明摇杆有漂移；只在死区内轻微跳动或完全不输出则正常。"),
            Step.Root(
                "测试键盘等需要 root 的设备",
                "sudo evtest",
                "列出全部设备后输入编号选择。").Then("列出所有输入设备（包括电源键、触摸屏、键盘），选择后实时打印按键事件及扫描码。"),
        ],

        Verify =
        [
            "1. 按下手柄 A 键时出现 `value 1`，松开时出现 `value 0`，说明该键工作正常。",
        ],
    };
}
