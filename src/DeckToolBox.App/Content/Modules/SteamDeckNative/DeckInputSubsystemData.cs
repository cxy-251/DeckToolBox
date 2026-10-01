using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class DeckInputSubsystemData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "deck-input",
        Title = "手柄输入链与 Steam Input",
        Keywords = "hidraw · hid-steam · uinput · 蜥蜴模式",
        Level = ModuleLevel.Advanced,
        Overview = "Deck 的按键、摇杆、触控板、背键和陀螺仪都来自同一个内置 USB 控制器。Steam 读取它的原始数据，按你的 Steam Input 配置转换后，再虚拟出一个标准手柄给游戏。",

        Why =
        [
            "1. 内置控制器是一个 USB HID 设备：",
            "• 在系统里它叫 `Valve Software Steam Controller`（厂商 ID `28de`）。按键、摇杆、触控板、4 个背键、摇杆电容触摸，以及 **六轴陀螺仪（IMU）** 的数据都通过它的 HID 报文上报。",
            "• 陀螺仪集成在控制器上，并不走内核的 IIO 传感器子系统。`/sys/bus/iio/devices/` 下能看到的 `ltrf216a` 是屏幕上方的环境光传感器，用于自动亮度。",
            "2. 三层输入链：",
            "- 第一层：内核为控制器创建 `/dev/hidraw*` 原始节点，并通过 udev 的 `uaccess` 规则授权给当前登录用户。",
            "- 第二层：Steam 客户端直接读取 hidraw 原始数据，套用 Steam Input 配置（触控板当鼠标、背键宏、陀螺仪瞄准等）。",
            "- 第三层：Steam 通过 `/dev/uinput` 创建虚拟设备（通常是 `Microsoft X-Box 360 pad`），游戏看到的是这个标准手柄。",
            "3. 内核驱动 `hid-steam`：Steam 没有运行时，它把控制器注册成普通手柄；Steam 打开原始接口后它会自动让位，避免游戏收到两份输入。",
            "4. 蜥蜴模式 (Lizard Mode)：没有程序接管控制器时（开机阶段、Steam 崩溃），控制器固件自己模拟键盘和鼠标：右触控板移动鼠标、R2 左键、L2 右键，保证桌面始终可操作。",
            "5. Steam Input 的几份配置文件：",
            "• 陀螺仪校准结果：`~/.local/share/Steam/config/<控制器序列号>_gyro.vdf`，记录三个轴的零点漂移 (`gyro_drift_per_sample_x/y/z`) 和判定「静止」的噪声容限。在控制器设置里重新校准陀螺仪会改写它；陀螺仪瞄准缓慢漂移时可以重新校准。",
            "• STEAM 键组合 (Chord)：`~/.local/share/Steam/controller_base/chord_neptune.vdf` 定义按住 STEAM 键时其他键的功能，例如长按 B 结束游戏 (`quit_application`)、X 弹出键盘、R1 截图、L1 切换放大镜。这是 Steam 自带的模板，更新客户端时会被覆盖，不要手改。",
            "6. 控制器固件：控制器有自己的微控制器和固件。`jupiter-controller-update --check` 只读取版本并与系统自带的固件比较；需要更新时，系统更新后会显示进度画面自动刷写（`/usr/lib/hwsupport/jupiter-controller-update-with-splash`）。",
            "7. 状态指示灯：Valve 的平台驱动把机身上的白色指示灯注册为 `/sys/class/leds/status:white`，udev 规则 `70-steam-jupiter-leds.rules` 在开机时把它的 `brightness` 等文件改归 deck 用户所有，所以不用 sudo 就能调节；Steam 也会根据状态改写它。",
        ],

        Consequences =
        [
            "• 查看输入设备、hidraw 节点、udev 属性与 VDF 配置文件都是只读操作。",
            "• 不要手动修改 `/dev/uinput` 或 `/dev/hidraw*` 的权限，它们由 udev 在每次开机时设置；改错会导致 Steam 无法创建虚拟手柄，游戏里手柄失灵。",
            "• 用 `evtest` 之类的工具独占（grab）某个输入设备时，其他程序会暂时收不到该设备的输入。",
        ],

        Steps =
        [
            Step.Read(
                "列出所有输入设备及其事件节点",
                "grep -E '^(N|H):' /proc/bus/input/devices",
                "`N:` 是设备名，`H:` 是对应的 `eventN`。").Then("成对的 `N: Name=\"...\"` 与 `H: Handlers=... eventN` 行，包括电源键、触摸屏、`Valve Software Steam Controller`，Steam 运行时还有 `Microsoft X-Box 360 pad` 虚拟手柄。"),
            Step.Read(
                "找到内置控制器对应的 hidraw 节点",
                "for h in /sys/class/hidraw/hidraw*; do grep -q '28DE' $h/device/uevent && echo \"/dev/$(basename $h): $(grep HID_NAME $h/device/uevent)\"; done",
                "控制器有多个 HID 接口，所以可能列出不止一个节点。").Then("输出一到三行，如 `/dev/hidraw0: HID_NAME=Valve Software Steam Controller`。"),
            Step.Read(
                "查看 uinput 与 hidraw 的访问权限",
                "getfacl -p /dev/uinput 2>/dev/null | grep user:",
                "这是 udev 的 `uaccess` 规则按当前登录用户动态授予的 ACL。").Then("输出几行 `user:...` 条目，其中 `user:deck:rw-` 说明当前用户被授予了读写权限。"),
            Step.Read(
                "查看陀螺仪校准数据",
                "cat ~/.local/share/Steam/config/*_gyro.vdf",
                "文件名前半部分是控制器序列号。从未校准过时可能没有这个文件。").Then("输出一个 `\"gyro_data\"` 块，含 `gyro_drift_per_sample_x/y/z` 三个小数（如 `-0.3152`）、`gyro_stationary_noise_tolerance` 与 `accelerometer_stationary_noise_tolerance`。"),
            Step.Read(
                "查看 STEAM 键组合的绑定",
                "grep -E '^\\s*\"(button_[a-z]+|dpad_[a-z]+|left_bumper|right_bumper)\"$|\"binding\"' ~/.local/share/Steam/controller_base/chord_neptune.vdf | tr -s '\\t' ' ' | head -n 30",
                "只挑出按键名和它下面的绑定动作；一个键可能有短按、长按两种绑定。").Then("按键名与动作交替出现，如 `\"button_b\"` 下有 `\"binding\" \"controller_action quit_application\"`（长按结束游戏），`\"button_x\"` 下有 `SHOW_KEYBOARD`，`\"button_a\"` 下有 `gr_toggle`（开始/停止录制）。"),
            Step.Read(
                "查看环境光传感器读数",
                "cat /sys/bus/iio/devices/iio:device*/name; cat /sys/bus/iio/devices/iio:device0/in_illuminance_input",
                "用手遮住屏幕上方再执行一次，数值会明显下降。").Then("先输出两行 `ltrf216a`（传感器名），再输出一个数字，如 `1.35`，单位勒克斯 (lux)。"),
            Step.Change(
                "调节状态指示灯亮度",
                "cat /sys/class/leds/status:white/max_brightness; echo 50 > /sys/class/leds/status:white/brightness",
                "第一条显示最大值（Deck 上为 `100`），`echo` 写入 0~100 之间的值，`0` 为熄灭。不需要 sudo。").Then("先输出 `100`，随后指示灯亮度立即改变；Steam 在充电状态变化等时机可能把它改回自己的设定值。"),
            Step.Read(
                "实时观察输入设备的热插拔事件",
                "udevadm monitor --udev --subsystem-match=input",
                "持续监听，直到手动退出。").Then("先显示 `monitor will print the received events`，然后停在那里等待；插拔手柄、连接蓝牙设备时滚出 `UDEV [时间] add ...` 或 `remove ...` 行。按 `Ctrl+C` 退出。"),
            Step.Read(
                "查看控制器固件版本",
                "jupiter-controller-update --check | tail -n 3",
                "只读检查，不会刷写。完整输出中有控制器的 hidraw 节点、厂商与产品 ID（十进制 `10462`、`4613`）等信息。").Then("最后三行如 `BIOS newer than 105 -- no initial firmware update necessary`、`NO UPDATE: Type 3 Device is running latest build 69C6B03C`、`needs_update=0`。"),
        ],

        Verify =
        [
            "1. Steam 运行时，第 1 步输出中有 `Microsoft X-Box 360 pad`，说明 Steam 已通过 uinput 创建虚拟手柄。",
            "2. 在桌面模式退出 Steam 后再执行第 1 步，虚拟手柄消失；此时右触控板仍能移动鼠标，即蜥蜴模式生效。",
            "3. 陀螺仪是否工作：Steam → 设置 → 控制器 → 校准，倾斜机身时陀螺仪指示随之移动。",
        ],
    };
}
