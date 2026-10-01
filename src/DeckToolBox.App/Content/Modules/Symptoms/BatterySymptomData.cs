using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class BatterySymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-battery",
        Title = "掉电快或插着电却不充电",
        Keywords = "电池 · 掉电快 · 续航 · 不充电 · Not charging · 充电上限 · 功耗",
        Level = ModuleLevel.Basic,
        Overview = "电量掉得比以前快，或者插着电源电量却不涨、显示「未充电」。先看电池状态和是否设置了充电上限，再看当前整机功耗与电池健康度。",

        Why =
        [
            "1. **插电不充电，多数是充电上限在起作用**：设置了充电上限（如 80%）后，电量到达上限就停止充电，状态为 `Not charging`，整机由电源直接供电。这是正常行为。",
            "2. **电源功率不足**：使用低功率充电器或供电不足的扩展坞时，游戏中的耗电可能超过输入功率，电量反而缓慢下降。",
            "3. **耗电高**：高 TDP、高帧率、高屏幕亮度、后台程序都会增加功耗。",
            "4. **电池老化**：满电容量随循环次数下降，续航自然变短。",
        ],

        Consequences =
        [
            "• 下面的检查都是只读的。",
        ],

        Steps =
        [
            Step.Read(
                "电池状态、电量与充电上限",
                "cat /sys/class/power_supply/BAT1/status /sys/class/power_supply/BAT1/capacity /sys/class/power_supply/ACAD/online; steamosctl get-max-charge-level",
                "依次是状态、电量百分比、电源是否插着 (`1`/`0`)、充电上限。").Then("如 `Not charging`、`80`、`1`、`Max charge level: 80`。**状态为 `Not charging` 且电量等于充电上限** → 正常，不需要处理；需要充满时执行 `steamosctl set-max-charge-level 100`。**插着电（`1`）却是 `Discharging`** → 电源功率不足，换用原装或更大功率的充电器。"),
            Step.Read(
                "当前整机功耗",
                "cd /sys/class/power_supply/BAT1 && awk 'NR==FNR{c=$1; next} {printf \"%.2f W\\n\", c*$1/1e12}' current_now voltage_now; sensors | grep PPT",
                "第一行是电池当前的充放电功率；`PPT` 是 APU 的功耗与上限。在不插电时执行才能反映实际耗电。").Then("不插电时第一行如 `8.42 W`；APU 功耗接近上限 `15.00 W` 时，降低快捷菜单中的 TDP 或帧率上限能明显延长续航。插着电且不充电时为 `0.00 W`。"),
            Step.Read(
                "电池健康度",
                "cd /sys/class/power_supply/BAT1 && echo \"健康度 $((100 * $(cat charge_full) / $(cat charge_full_design)))%  循环次数 $(cat cycle_count)\"",
                "当前满电容量与设计容量之比。").Then("如 `健康度 103%  循环次数 0`。健康度明显低于 80% 时，续航变短主要是电池老化造成的。"),
        ],

        Verify =
        [
            "• 取消充电上限后（设为 100），插电时状态变为 `Charging`，电量开始上涨。",
        ],

        Related = ["deck-hardware-ec", "steamosctl", "sysfs", "power-sleep"],
    };
}
