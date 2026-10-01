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
            "4. 风扇控制：`jupiter-fan-control` 服务（Python 脚本 `/usr/share/jupiter-fan-control/fancontrol.py`）按机型读取 `jupiter-config.yaml`、`galileo-config.yaml` 等配置：",
            "- CPU、GPU 温度各用一条二次曲线算出转速，并按功耗加一个前馈量；SSD 温度用另一条二次曲线；取几者的最大值，写入 `steamdeck_hwmon` 的 `fan1_target`。",
            "- 服务停止时把控制权交还 EC（嵌入式控制器）固件：新 BIOS 上的做法是向 `fan1_target` 写 `0`，由 EC 按固件内置曲线自行调速。",
            "5. 充电上限与插电不充：设置了充电上限（见「steamosctl」模块的 `set-max-charge-level`）后，值保存在 `steamdeck_hwmon` 的 `max_battery_charge_level`。电量达到上限时，电池状态变为 `Not charging`、电流约为 0，整机由电源适配器直接供电，电池不再被反复充满。",
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
                "来自 lm_sensors，汇总所有 hwmon 传感器。").Then("按设备分组输出：`amdgpu` 组的 `edge` 是 APU 温度，`slowPPT`/`fastPPT` 是当前功耗及上限 (`cap`)；`steamdeck_hwmon` 组有 `System Fan` 转速、电池温度和 USB-C 充电协商的电压电流；另有 nvme、Wi-Fi 网卡温度。"),
            Step.Read(
                "列出所有 hwmon 设备及其驱动名",
                "for d in /sys/class/hwmon/hwmon*; do echo \"$d  $(cat $d/name)\"; done",
                "编号每次开机可能变化，编写脚本时用驱动名定位目录。").Then("每行「目录  驱动名」，例如 `/sys/class/hwmon/hwmon6  amdgpu`、`/sys/class/hwmon/hwmon3  steamdeck_hwmon`。"),
            Step.Read(
                "查看当前 TDP 上限与允许范围（瓦）",
                "d=$(grep -l '^amdgpu$' /sys/class/hwmon/hwmon*/name | xargs dirname); for f in power1_cap power1_cap_min power1_cap_max; do awk -v n=$f '{printf \"%-16s %.1f W\\n\", n, $1/1000000}' $d/$f; done",
                "`power1_cap` 就是快捷菜单 TDP 滑块当前生效的值；`_max` 是硬件上限，比 SteamOS 允许设置的 15 W 高。").Then("输出三行，例如 `power1_cap 15.0 W`、`power1_cap_min 0.0 W`、`power1_cap_max 29.0 W`。"),
            Step.Read(
                "查看风扇转速",
                "cat $(grep -l '^steamdeck_hwmon$' /sys/class/hwmon/hwmon*/name | xargs dirname)/fan1_input",
                "按驱动名找到 steamdeck_hwmon 目录再读取。").Then("输出一个数字，如 `3366`，单位是每分钟转数 (RPM)。待机时可能为 0，高负载时约 4000~6000。"),
            Step.Read(
                "查看电池状态与健康度",
                "cd /sys/class/power_supply/BAT1 && echo \"状态 $(cat status)  电量 $(cat capacity)%  健康度 $((100 * $(cat charge_full) / $(cat charge_full_design)))%\"",
                "健康度 = 当前满电容量 / 设计容量，新电池可能略高于 100%。").Then("输出一行，例如 `状态 Not charging  电量 79%  健康度 103%`。状态可能是 Charging（充电中）、Discharging（放电中）、Full、Not charging（插电但未充电，常因达到充电上限）。"),
            Step.Read(
                "查看电源适配器在线与电池供电状态",
                "cat /sys/class/power_supply/ACAD/uevent; echo ---; grep -E 'STATUS|CAPACITY|CURRENT_NOW|VOLTAGE_NOW' /sys/class/power_supply/BAT1/uevent",
                "`ACAD` 是电源适配器，`BAT1` 是电池；`uevent` 文件把各项属性汇总成「键=值」。电流单位微安，电压单位微伏。").Then("第一段中 `POWER_SUPPLY_ONLINE=1` 表示插着电源；第二段如 `POWER_SUPPLY_STATUS=Not charging`、`POWER_SUPPLY_VOLTAGE_NOW=8536000`（8.5 V）、`POWER_SUPPLY_CURRENT_NOW=0`、`POWER_SUPPLY_CAPACITY=80`：插着电源、达到充电上限、电池不充也不放。"),
            Step.Read(
                "查看风扇控制服务状态",
                "systemctl status jupiter-fan-control --no-pager",
                "`--no-pager` 直接输出，不进入分页界面。").Then("显示服务的几行状态，其中 `Active: active (running)` 表示正常运行，下方还有最近几行日志。"),
            Step.Read(
                "查看 GPU 核心与显存当前运行频率及 DPM 档位",
                "cat /sys/class/drm/card0/device/pp_dpm_sclk; echo ---; cat /sys/class/drm/card0/device/pp_dpm_mclk",
                "`sclk` 是 GPU 核心频率，`mclk` 是内存频率（APU 与 CPU 共用 LPDDR5 内存）。带 `*` 的是当前档位；GPU 核心实际频率可以落在档位之间。").Then("第一段三行：`0: 200Mhz *`、`1: 1100Mhz`、`2: 1600Mhz`（空闲时停在 200）；第二段四行，如 `0: 400Mhz` 到 `3: 800Mhz *`。玩游戏时再执行，带 `*` 的档位会升高。"),
            Step.Read(
                "查看风扇实际转速与软件目标转速",
                "d=$(grep -l '^steamdeck_hwmon$' /sys/class/hwmon/hwmon*/name | xargs dirname); echo \"实际: $(cat $d/fan1_input) RPM / 目标: $(cat $d/fan1_target) RPM\"",
                "先按 `name` 找到 `steamdeck_hwmon` 所在目录，再读实际转速 `fan1_input` 与 jupiter-fan-control 写入的目标转速 `fan1_target`。").Then("输出一行，如 `实际: 2113 RPM / 目标: 2044 RPM`，两者应接近。目标为 `0` 说明风扇由 EC 固件控制（风扇服务未运行或已交还控制权）。"),
        ],

        Verify =
        [
            "1. 在快捷菜单中把 TDP 限制调到 8 W，重新执行第 3 步，`power1_cap` 应显示 8.0 W；关闭限制后恢复为默认值。",
            "2. 运行游戏时执行 `watch -n1 sensors`，可以看到温度与 PPT 功耗随负载实时变化（`Ctrl+C` 退出）。",
            "3. 参考范围：桌面待机时 APU 约 45~60 °C，满载游戏约 70~90 °C 都属于正常工作区间。",
        ],
    };
}
