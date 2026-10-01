using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.KernelVfs;

public static class SysDeviceModelData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sysfs",
        Title = "/sys 设备模型",
        Keywords = "/sys · power_supply · cpufreq",
        Level = ModuleLevel.Advanced,
        Overview = "/sys 以目录树的形式展示内核管理的所有设备和驱动，每个属性是一个小文本文件：读取它就是查询硬件状态，root 写入它就是调整设置。电池、CPU 频率、温度、背光等信息都能在这里直接读到。",

        Why =
        [
            "1. 设备模型：内核把总线（PCI、USB、I2C）、设备和驱动组织成一棵树，`/sys/devices/` 是真实的层次结构，`/sys/class/` 按设备类型分组提供了便捷的入口。",
            "2. 很多图形工具（包括快捷菜单里的性能设置、第三方调优插件）最终都是在读写 `/sys` 下的这些文件。",
            "3. 常用入口：",
            "- `/sys/class/power_supply/BAT1/`：电池电量、状态、满电容量、设计容量。",
            "- `/sys/devices/system/cpu/cpu*/cpufreq/`：各核心当前频率、调频驱动和策略。",
            "- `/sys/class/thermal/thermal_zone*/`：温区温度。",
            "- `/sys/class/hwmon/`：各类硬件传感器（见「硬件传感器、功耗与风扇」）。",
            "- `/sys/class/drm/card0/device/`：显卡负载（`gpu_busy_percent`）与核心频率（`pp_dpm_sclk`）。",
            "- `/sys/class/backlight/`：屏幕背光驱动级别（`actual_brightness`、`max_brightness`）。",
            "- `/sys/devices/virtual/dmi/id/`：主板与 BIOS 固件信息（`product_name`、`bios_version`、`sys_vendor`）。",
            "- `/sys/class/net/<接口>/`：网络物理链路状态（`operstate`）与 MAC 地址（`address`）。",
            "4. 数值单位通常是整数形式的最小单位：温度为毫摄氏度，频率为 kHz，电流为微安，电压为微伏。",
        ],

        Consequences =
        [
            "• 读取 `/sys` 不会改变硬件状态。",
            "• 写入需要 root，并且立即生效。在 SteamOS 上，很多值由 steamos-manager 和 Steam 管理，手动写入的值可能随时被覆盖；需要调整时优先使用 `steamosctl` 或快捷菜单。",
        ],

        Steps =
        [
            Step.Read(
                "读取机型、厂商与 BIOS 版本",
                "head -n 1 /sys/class/dmi/id/{sys_vendor,product_name,bios_version,chassis_type}",
                "DMI 是固件提供给系统的硬件描述表。`head` 给多个文件时会在每个前面加 `==> 文件名 <==` 标题。`chassis_type` 是 SMBIOS 规定的机箱类型代码（8 = 便携式，10 = 笔记本，3 = 台式机）。").Then("四段输出：`Valve`、`Galileo`（OLED 版；LCD 版为 `Jupiter`）、BIOS 版本如 `F7G0114`、机箱类型 `8`。"),
            Step.Read(
                "查看电池健康度（满电容量 / 设计容量）",
                "cd /sys/class/power_supply/BAT1 && echo \"健康度 $((100 * $(cat charge_full) / $(cat charge_full_design)))%  循环次数 $(cat cycle_count)\"",
                "Deck 的电池名为 `BAT1`，大多数笔记本是 `BAT0`，可先 `ls /sys/class/power_supply/` 确认。").Then("输出一行，如 `健康度 103%  循环次数 0`（部分电池不报告循环次数，显示 0）。"),
            Step.Read(
                "计算电池当前充放电功率",
                "cd /sys/class/power_supply/BAT1 && awk 'NR==FNR{c=$1; next} {printf \"%.2f W\\n\", c*$1/1e12}' current_now voltage_now",
                "Deck 的电池没有 `power_now`，用电流 (µA) × 电压 (µV) 计算。").Then("输出一个功率值，如 `8.42 W`；插着电源且不在充电时为 `0.00 W`。"),
            Step.Read(
                "查看每个 CPU 核心的当前频率",
                "paste <(ls -d /sys/devices/system/cpu/cpu[0-9]* | grep -oE 'cpu[0-9]+') <(awk '{printf \"%.0f MHz\\n\", $1/1000}' /sys/devices/system/cpu/cpu[0-9]*/cpufreq/scaling_cur_freq)",
                "`scaling_cur_freq` 的单位是 kHz。").Then("每行「核心名  频率」，如 `cpu0  2425 MHz`，共 8 行；多执行几次可以看到频率随负载跳动。"),
            Step.Read(
                "查看 CPU 调频策略",
                "cat /sys/devices/system/cpu/cpu0/cpufreq/scaling_driver /sys/devices/system/cpu/cpu0/cpufreq/scaling_governor",
                "这里的 powersave 由硬件按能耗偏好调频，并不意味着锁在低频。").Then("输出两行：`amd-pstate-epp`（调频驱动）和 `powersave`（调速策略）。"),
            Step.Read(
                "查看各温区温度",
                "paste <(cat /sys/class/thermal/thermal_zone*/type) <(awk '{printf \"%.1f °C\\n\", $1/1000}' /sys/class/thermal/thermal_zone*/temp)",
                "更完整的传感器读数见 `sensors` 命令（「硬件传感器、功耗与风扇」模块）。").Then("每行「温区类型  温度」，Deck 上通常只有 `acpitz  62.0 °C`。"),
            Step.Read(
                "读取 GPU 核心实时负载百分比",
                "cat /sys/class/drm/card0/device/gpu_busy_percent",
                "amdgpu 驱动提供的瞬时值。连续观察可用 `watch -n 1 cat /sys/class/drm/card0/device/gpu_busy_percent`。").Then("输出一个 0~100 的整数；桌面空闲时通常是 `0` 或个位数，玩游戏时接近满载。"),
            Step.Read(
                "查看屏幕背光物理亮度与最大级别",
                "cd /sys/class/backlight/* && echo \"当前: $(cat actual_brightness) / 最大: $(cat max_brightness)\"",
                "Deck 只有一个背光设备 `amdgpu_bl0`，所以通配符 `*` 只会匹配到它。数值是驱动内部的级别，不是百分比。").Then("输出一行，如 `当前: 41931 / 最大: 599000`。拖动亮度滑块后再执行，`当前` 随之变化。"),
            Step.Read(
                "查看 DRM 屏幕物理连接与原生分辨率",
                "cat /sys/class/drm/card0-eDP-1/status && cat /sys/class/drm/card0-eDP-1/modes",
                "`eDP-1` 是内置屏的接口名，外接显示器为 `DP-1`。").Then("第一行 `connected`，第二行 `800x1280`：面板物理上是竖屏，由桌面或 Gamescope 旋转成横屏使用。"),
        ],

        Verify =
        [
            "1. 运行一个高负载程序时再读取 CPU 频率，数值会明显上升。",
            "2. 拔掉电源后读取电池状态，`status` 变为 `Discharging`。",
        ],
    };
}
