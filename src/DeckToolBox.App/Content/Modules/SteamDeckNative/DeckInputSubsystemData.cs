using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class DeckInputSubsystemData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "deck-input",
        Title = "手柄输入链与 Steam Input",
        Keywords = "hid-steam · evdev · uinput",
        Level = ModuleLevel.Advanced,
        Overview = "深入解密 Steam Deck 触控板、背键与六轴陀螺仪硬件输入链，剖析 Linux IIO 工业传感器接口、hid-steam 内核驱动与 /dev/uinput 虚拟游戏手柄映射机制。",

        Why =
        [
            "1. 超越传统手柄协议的硬件拓扑：",
            "• 传统手柄（如 Xbox 手柄）只有固定的按键与双摇杆协议。而 Steam Deck 拥有双线性触觉触控板、4 颗背键 (L4/L5/R4/R5)、摇杆电容触摸传感器，以及一颗高精度的六轴 IMU (陀螺仪 + 加速度计)。",
            "• 这些非标硬件如果直接暴露给普通 Windows 游戏，游戏根本无法识别和映射！",
            "2. IIO (Industrial I/O) 工业传感器子系统：",
            "• 掌机的博世/InvenSense 六轴传感器接入了 Linux 内核的 **IIO (Industrial I/O)** 子系统（暴露在 `/sys/bus/iio/devices/`）。",
            "• 内核以极低延时持续采集角速度 (`in_anglvel_x_raw`) 与重力加速度，驱动陀螺仪体感瞄准。",
            "3. 核心分发中枢：`/dev/uinput` 与虚拟手柄合成：",
            "• **第一层**：掌机物理手柄通过内核模块 `hid_steam.ko` 作为原始 HID 设备挂载到 `/dev/hidraw*`。",
            "• **第二层**：Steam 客户端常驻进程独占读取原始按键与陀螺仪数据流，并根据你在 Steam 输入设置中配置的复杂逻辑（如触控板径向轮盘菜单、按压切换操作集、陀螺仪转鼠标）。",
            "• **第三层**：Steam 通过调用内核的 `/dev/uinput` (User Input) 接口，**动态凭空虚拟出一个标准的 Xbox 360 / DualShock 4 控制器设备**，游戏读到的就是这个虚拟手柄！",
            "4. 固件级保底防御：蜥蜴模式 (Lizard Mode)：",
            "• 当系统刚开机、或 Steam 客户端闪退/未启动时，手柄芯片会自动回退至硬件固化烧录的 **“蜥蜴模式”**：右触控板强制模拟为标准 USB 鼠标移动，右扳机 R2 强制模拟为鼠标左键，确保你永远不会陷入没有鼠标无法操作桌面的死局！",
        ],

        Consequences =
        [
            "• 读取 IIO 传感器与 input 事件只读无害：使用 cat 读取陀螺仪原始数值或监控事件流不会产生任何副作用。",
            "• 严禁篡改 `/dev/uinput` 访问权限：`/dev/uinput` 是 Steam 动态创建虚拟手柄的核心通道，若权限被误改将导致进入游戏后手柄完全无法操控。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前系统所有已识别的物理与虚拟输入设备硬件名称与通道映射",
                "cat /proc/bus/input/devices | grep -E \"(Name=|Handlers=|Phys=)\"",
                "定位 Valve Neptune Controller 物理节点与 Steam 虚拟手柄生成的 event* 句柄。"),
            Step.Read(
                "探测内核 IIO 工业传感器总线下的掌机六轴陀螺仪设备节点",
                "ls -la /sys/bus/iio/devices/ && cat /sys/bus/iio/devices/iio:device*/name 2>/dev/null",
                "确认掌机主板板载 IMU (如 bmi160 或 icm42607) 传感器设备驱动已被内核正确接管。"),
            Step.Read(
                "实时读取掌机六轴陀螺仪当前空间角速度与重力加速度瞬时原始采样",
                "for f in /sys/bus/iio/devices/iio:device*/in_*_raw; do [ -f \"$f\" ] && awk -v n=\"$(basename $f)\" '{printf \"%-24s: %d\\n\", n, $1}' $f; done | head -n 8",
                "倾斜掌机或转动手腕，实时观察角速度和加速度数值随着物理移动而剧烈跳变。"),
            Step.Read(
                "检查内核用户空间输入注入通道 (/dev/uinput) 权限与状态",
                "ls -l /dev/uinput",
                "验证权限为 `crw-rw-rw-` 或通过 udev 规则赋予桌面用户读写权限，确保虚拟手柄注入通道畅通。"),
        ],

        Verify =
        [
            "1. 虚拟手柄存活验证：当在 Steam 中启动任意手柄游戏时，执行 `ls -l /dev/input/by-id/`，你会看到诸如 `usb-Valve_Software_Steam_Controller-event-joystick` 或虚拟 Xbox 游戏杆节点已自动动态挂载，证实 uinput 链路工作正常。",
            "2. 陀螺仪基线验证：掌机静止平放于桌面时，读取陀螺仪角速度 (`in_anglvel_*_raw`) 读数应稳定在 0 附近微幅波动，重力轴输出接近 1G 加速度当量。",
        ],
    };
}
