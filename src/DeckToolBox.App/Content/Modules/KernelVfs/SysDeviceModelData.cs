using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.KernelVfs;

public static class SysDeviceModelData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sys_device_model",
        Title = "/sys 硬件设备模型与驱动调控 (Sysfs)",
        Subtitle = "内核虚拟文件系统",
        CategoryName = "内核虚拟文件系统",
        Badge = "硬件总线",
        Overview = "掌握 Linux 内核统一设备模型 (sysfs)，通过标准文本接口，深度观测与调控电池、温控、GPU 显卡与 CPU 调速策略。",

        WhyCard = new InfoCardItem(
            "📌",
            "揭秘 Linux 内核如何用 /sys 将物理硬件“纯文件化”",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 统一设备模型 (kobject 架构)：Linux 将 PCI/PCIe 总线、USB 控制器、I2C 传感器与物理芯片映射为清晰的树状拓扑。所有硬件属性都是一个普通文件，读文件就是查传感器，写文件就是调控参数！",
                "2. 掌机玩机工具的底层真相：所有第三方功耗调优插件（如 PowerTools、TDP 调节器、风扇曲线控制器），本质上没有高深魔法，全都是在读写 /sys 下的标准属性文件！",
                "3. 核心硬件类接口全景：",
                "• /sys/class/power_supply/BAT*：电池健康度、当前充电功率、设计容量与充放电循环次数。",
                "• /sys/devices/system/cpu/cpu*/cpufreq/：每个 CPU 核心的当前工作频率、可用调速器 (scaling_governor: schedutil / performance / powersave)。",
                "• /sys/class/thermal/thermal_zone*：主板各关键区域（APU 核心、供电、闪存）温度传感器毫度读数。",
                "• /sys/class/drm/card*：GPU 图形接口、显存时钟状态与连接器点亮状态。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "读写硬件属性的影响边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 只读查询完全无害：查询电池容量、温度、主频纯属内核数据镜像，不会引起硬件状态任何改变。",
                "• 调速器修改即时生效：如果向 scaling_governor 写入 performance，内核会立即锁定最高主频运行，提升响应速度但会加速耗电与发热。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(1, "查看电池健康度、设计容量与实际可用容量 (Health Calculation)", "awk 'BEGIN{getline f < \"/sys/class/power_supply/BAT1/charge_full\"; getline d < \"/sys/class/power_supply/BAT1/charge_full_design\"; printf \"电池当前健康度: %.2f%% (实际满电: %d mAh / 设计出厂: %d mAh)\\n\", (f/d)*100, f/1000, d/1000}' 2>/dev/null || cat /sys/class/power_supply/BAT1/capacity", "直接通过内核底层数值精确计算出电池无损耗健康百分比。"),
            new(2, "查看电池当前实时充放电功率与电压", "paste <(echo -n \"当前功率(W): \") <(echo \"scale=2; $(cat /sys/class/power_supply/BAT1/power_now 2>/dev/null || cat /sys/class/power_supply/BAT1/current_now * $(cat /sys/class/power_supply/BAT1/voltage_now) / 1000000000000) / 1000000\" | bc -l) 2>/dev/null || cat /sys/class/power_supply/BAT1/status", "实时获知掌机当前待机或高负荷运行下的真实放电瓦数。"),
            new(3, "查看 CPU 所有核心当前调速器与实时运行频率", "paste <(cat /sys/devices/system/cpu/cpu*/cpufreq/scaling_cur_freq | awk '{printf \"Core %d: %.1f MHz | \", NR-1, $1/1000}') <(cat /sys/devices/system/cpu/cpu*/cpufreq/scaling_governor | awk '{print \"Governor: \" $1}')", "实时监控每个 CPU 核心在 Linux 调度器下的频率跳变。"),
            new(4, "查看内核 APU 核心各热区当前实时温度", "paste <(cat /sys/class/thermal/thermal_zone*/type) <(cat /sys/class/thermal/thermal_zone*/temp | awk '{printf \"%.1f °C\\n\", $1/1000}')", "读取毫摄氏度读数并折算为清晰的摄氏度温度榜。")
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "验证与观察结论",
            "#238636",
            "#3FB950",
            new[]
            {
                "观察输出结果：你会看到清晰的 CPU 当前核心主频（通常在 1600MHz~3500MHz 间弹性浮动），电池健康度通常在 95%~100% 之间，证实物理设备已被 Linux 内核精准接管。"
            }
        )
    };
}
