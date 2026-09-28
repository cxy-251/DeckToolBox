using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class DeckHardwareEcData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "deck_hardware_ec_tdp",
        Title = "硬件嵌入式控制器 (EC)、APU 功耗墙与温控 (hwmon / jupiter-fan / TDP)",
        Subtitle = "硬件 EC 控制器与 APU 调控",
        CategoryName = "硬件 EC 控制器与 APU 调控",
        Badge = "功耗温控",
        Overview = "深入解密 Steam Deck 定制嵌入式控制器 (EC 芯片)、Linux 内核 hwmon 传感器拓扑、Valve 官方 jupiter-fan 风扇静音算法与 AMD APU 毫瓦级 TDP 功耗调控。",

        WhyCard = new InfoCardItem(
            "📌",
            "揭秘掌机主板大脑：嵌入式控制器 (EC) 与 APU 动态能耗中枢",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 独立于 CPU 运行的微型主板管家 (Embedded Controller - EC)：",
                "• 掌机主板上集成了一颗专用的 EC 微控制器。在 AMD APU 处理器尚未上电前，EC 就负责 Type-C PD 快速充电握手、电池充放电曲线保护、以及风扇 PWM 硬件脉宽控制。",
                "• Linux 内核通过定制平台驱动与 EC 通信，将其传感器统一映射到内核标准的 `/sys/class/hwmon/` (Hardware Monitoring) 虚拟文件目录树下。",

                "2. Valve 官方风扇静音调度算法 (jupiter-fan-control)：",
                "• 早期传统 BIOS 硬件风扇曲线极其敏感，APU 稍有瞬时负载风扇便呼啸狂转，发热回落后又急停，噪音极度刺耳。",
                "• Valve 研发了 `jupiter-fan-control` 守护进程：通过软件级 PID 动态滤波算法平滑温度突刺，根据历史散热趋势柔和变速，兼顾芯片寿命与绝对静音体验。",

                "3. 掌机 TDP 功耗墙与 GPU 锁频的内核真相：",
                "• 掌机右侧【...】快捷菜单中的“TDP 限制 (3W~15W)”本质上没有任何神秘魔法——它直接在向内核的 `/sys/class/hwmon/hwmon*/power1_cap` 接口写入微瓦 (μW) 整数（例如 10W 即写入 10000000）！",
                "• 锁显卡频率则是直接写入 AMDGPU 驱动的 DPM 运行级接口 (`pp_dpm_sclk` / `pp_od_clk_voltage`)，将宝贵的 APU 功耗预算精准留给 CPU 运算或 GPU 图形渲染。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "功耗调节与风扇接管的安全红线 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 查询只读传感器绝对安全：读取温度、当前转速、实时放电瓦数纯属内核内存数据镜像，0 硬件开销。",
                "• TDP 调节的下限保护：TDP 最低不宜限制在 3W 以下，否则可能导致 Wayland 桌面渲染发生严重丢帧或掉驱动；若限制在 15W 以上由于掌机散热模组物理限制会引发剧烈降频。",
                "• 芯片过热硬件断电兜底：即便有错误脚本误停风扇，AMD APU 与 EC 芯片内部均硬编码了 105°C 物理熔断关机保护，不会发生芯片物理烧毁事故。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看系统当前全部注册的 hwmon 硬件传感器设备清单与驱动名",
                "for d in /sys/class/hwmon/hwmon*; do echo -e \"$d -> \\t$(cat $d/name 2>/dev/null)\"; done",
                "识别 amdgpu (显卡核显)、k10temp (AMD CPU 核心测温)、acpitz (主板热区) 与掌机专属平台驱动。"
            ),
            new(
                2,
                "查看当前各个硬件组件的毫摄氏度实时温度榜",
                "for d in /sys/class/hwmon/hwmon*; do name=$(cat $d/name 2>/dev/null); for t in $d/temp*_input; do [ -f \"$t\" ] && awk -v dev=\"$name\" -v file=\"$(basename $t)\" '{printf \"[%s / %s]: %.1f °C\\n\", dev, file, $1/1000}' $t; done; done",
                "全面扫描掌机各关键芯片与热区温度，折算为直观的摄氏度列表。"
            ),
            new(
                3,
                "读取掌机散热风扇当前实际转速 (RPM 转/分钟)",
                "cat /sys/class/hwmon/hwmon*/fan1_input 2>/dev/null | awk '{printf \"风扇当前实时转速: %d RPM\\n\", $1}'",
                "获取当前风扇的实际转速读数（待机约 0~2000 RPM，满载约 4500~6000 RPM）。"
            ),
            new(
                4,
                "查看 APU 当前设定的 TDP 功耗上限与硬件最大允许功耗",
                "cat /sys/class/hwmon/hwmon*/power1_cap 2>/dev/null | awk '{printf \"当前 TDP 功耗限制: %.2f W (微瓦: %d)\\n\", $1/1000000, $1}'",
                "验证掌机当前正在生效的 APU 功耗墙数值。"
            ),
            new(
                5,
                "查看 Valve 官方风扇智能调谐服务运行状态",
                "systemctl status jupiter-fan-control 2>/dev/null | grep -E \"(Active:|Loaded:)\" || echo \"未运行独立守护进程，当前由内核原生温控接管\"",
                "检查掌机温控软件层是否正常工作。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "散热与能耗健康状态评估 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 温度正常区间：掌机在桌面待机状态下 APU 温度通常在 40°C~55°C 之间；运行 3A 大作满载状态下在 75°C~88°C 属于 AMD Zen 微架构的健康工作设计范围。",
                "2. 动态调节验证：若通过快捷菜单将 TDP 调至 8W，再次执行步骤 4 的命令，读数将精确变为 `8.00 W`，证实用户态配置已穿透至内核驱动层。"
            }
        )
    };
}
