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
        Overview = "掌握 Linux 内核统一设备模型 (sysfs)，通过标准文本接口，深度观测与调控电池、温控、GPU 显卡与 CPU 调速策略。",

        Why =
        [
            "1. 统一设备模型 (kobject 架构)：Linux 将 PCI/PCIe 总线、USB 控制器、I2C 传感器与物理芯片映射为清晰的树状拓扑。所有硬件属性都是一个普通文件，读文件就是查传感器，写文件就是调控参数！",
            "2. 掌机玩机工具的底层真相：所有第三方功耗调优插件（如 PowerTools、TDP 调节器、风扇曲线控制器），本质上没有高深魔法，全都是在读写 /sys 下的标准属性文件！",
            "3. 核心硬件类接口全景：",
            "• /sys/class/power_supply/BAT*：电池健康度、当前充电功率、设计容量与充放电循环次数。",
            "• /sys/devices/system/cpu/cpu*/cpufreq/：每个 CPU 核心的当前工作频率、可用调速器 (scaling_governor: schedutil / performance / powersave)。",
            "• /sys/class/thermal/thermal_zone*：主板各关键区域（APU 核心、供电、闪存）温度传感器毫度读数。",
            "• /sys/class/drm/card*：GPU 图形接口、显存时钟状态与连接器点亮状态。",
        ],

        Consequences =
        [
            "• 只读查询完全无害：查询电池容量、温度、主频纯属内核数据镜像，不会引起硬件状态任何改变。",
            "• 调速器修改即时生效：如果向 scaling_governor 写入 performance，内核会立即锁定最高主频运行，提升响应速度但会加速耗电与发热。",
        ],

        Steps =
        [
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
        ],

        Verify =
        [
            "观察输出结果：你会看到清晰的 CPU 当前核心主频（通常在 1600MHz~3500MHz 间弹性浮动），电池健康度通常在 95%~100% 之间，证实物理设备已被 Linux 内核精准接管。",
        ],
    };
}
