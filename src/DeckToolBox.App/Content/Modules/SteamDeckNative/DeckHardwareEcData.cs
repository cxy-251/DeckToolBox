using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class DeckHardwareEcData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "deck-hardware-ec",
        Title = "硬件传感器、功耗与风扇",
        Keywords = "sensors · hwmon · TDP · jupiter-fan-control",
        Level = ModuleLevel.Intermediate,
        Overview = "Steam Deck 的温度、风扇转速、APU 功耗上限和电池状态，内核都以普通文本文件的形式放在 /sys 下。快捷菜单里的 TDP 滑块、性能浮层读的也是这些文件。",

        Why =
        [
            "1. hwmon：内核统一的硬件监控接口：",
            "• 每个传感器驱动在 `/sys/class/hwmon/hwmonN/` 下注册一个目录，`name` 文件是驱动名。Deck 上常见的有：",
            "- `amdgpu`：APU 的边缘温度 (`temp1_input`)、功耗 (`power1_input`) 与功耗上限 (`power1_cap`)。",
            "- `steamdeck_hwmon`：Valve 的平台驱动，提供风扇转速 (`fan1_input`)、电池温度、USB-C PD 协商电压电流。",
            "- `nvme`、`ath11k_hwmon`（Wi-Fi 网卡）、`BAT1`、`acpitz` 等。",
            "• `N` 的编号每次开机可能变化，脚本里要按 `name` 查找，不要写死 `hwmon6`。",
            "2. 数值单位：温度是毫摄氏度（`65000` = 65 °C），功耗是微瓦（`15000000` = 15 W），电压是毫伏。",
            "3. TDP 滑块的本质：快捷菜单（`…` 键）→ 性能 → TDP 限制，由 SteamOS 写入 `amdgpu` 的 `power1_cap`。APU 的实际功耗不会长时间超过这个上限，因此降低 TDP 能换来更长续航和更低温度。",
            "4. 风扇控制：`jupiter-fan-control` 是一个系统服务，根据 APU 与电池温度计算目标转速，比固件默认曲线更平缓、更安静。",
        ],

        Consequences =
        [
            "• 读取 `/sys` 下的传感器文件完全无副作用。",
            "• 不建议用 `sudo` 直接写 `power1_cap`、风扇或 GPU 频率文件：SteamOS 会在切换游戏或调整快捷菜单时覆盖你的值，而错误的值可能导致降频或不稳定。日常调节请使用快捷菜单。",
            "• 停止 `jupiter-fan-control` 服务后风扇交回固件控制，噪音会增大；APU 自身有过热降频与过热关机保护，但不应长期依赖它。",
        ],

        Steps =
        [
            Step.Read(
                "一次性查看所有传感器",
                "sensors",
                "`amdgpu` 下的 `edge` 是 APU 温度，`slowPPT`/`fastPPT` 是功耗与上限；`steamdeck_hwmon` 下是风扇转速和电池温度。"),
            Step.Read(
                "列出所有 hwmon 设备及其驱动名",
                "for d in /sys/class/hwmon/hwmon*; do echo \"$d  $(cat $d/name)\"; done",
                "编写脚本时用驱动名定位目录。"),
            Step.Read(
                "查看当前 TDP 上限与允许范围（瓦）",
                "d=$(grep -l '^amdgpu$' /sys/class/hwmon/hwmon*/name | xargs dirname); for f in power1_cap power1_cap_min power1_cap_max; do awk -v n=$f '{printf \"%-16s %.1f W\\n\", n, $1/1000000}' $d/$f; done",
                "`power1_cap` 就是快捷菜单 TDP 滑块当前生效的值。"),
            Step.Read(
                "查看风扇转速",
                "cat $(grep -l '^steamdeck_hwmon$' /sys/class/hwmon/hwmon*/name | xargs dirname)/fan1_input",
                "单位 RPM。待机时可能为 0，高负载时约 4000~6000。"),
            Step.Read(
                "查看电池状态与健康度",
                "cd /sys/class/power_supply/BAT1 && echo \"状态 $(cat status)  电量 $(cat capacity)%  健康度 $((100 * $(cat charge_full) / $(cat charge_full_design)))%\"",
                "健康度 = 当前满电容量 / 设计容量。新电池可能略高于 100%。"),
            Step.Read(
                "查看风扇控制服务状态",
                "systemctl status jupiter-fan-control --no-pager",
                "`Active: active (running)` 表示正常工作。"),
        ],

        Verify =
        [
            "1. 在快捷菜单中把 TDP 限制调到 8 W，重新执行第 3 步，`power1_cap` 应显示 8.0 W；关闭限制后恢复为默认值。",
            "2. 运行游戏时执行 `watch -n1 sensors`，可以看到温度与 PPT 功耗随负载实时变化（`Ctrl+C` 退出）。",
            "3. 参考范围：桌面待机时 APU 约 45~60 °C，满载游戏约 70~90 °C 都属于正常工作区间。",
        ],
    };
}
