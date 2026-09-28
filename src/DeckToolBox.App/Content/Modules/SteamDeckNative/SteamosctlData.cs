using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamosctlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steamosctl",
        Title = "steamosctl：终端里的快捷菜单",
        Keywords = "TDP · 充电上限 · GPU 频率 · 调度器 · 切换模式",
        Level = ModuleLevel.Intermediate,
        Overview = "快捷菜单（… 键）和设置里的很多开关，背后都是 steamos-manager 服务。steamosctl 是它的命令行客户端：TDP、电池充电上限、GPU 性能级别、CPU 调度器、风扇控制、切换桌面/游戏模式，都能在终端里查看和设置，不需要 sudo。",

        Why =
        [
            "1. steamos-manager 的两层结构：",
            "• 系统级服务 `steamos-manager.service`（root 身份）真正去写 `/sys` 下的硬件接口。",
            "• 用户级服务（systemd --user）接收 Steam 客户端和 `steamosctl` 的请求，经 D-Bus 转交给系统级服务。所以普通用户就能安全地调整这些设置，而且只能调整它允许的范围。",
            "2. 与直接写 `/sys` 的区别：steamos-manager 会做范围检查，并与 Steam 客户端保持一致。直接写 `/sys` 的值可能被 Steam 随时覆盖，用 steamosctl 设置的值 Steam 界面里也能看到。",
            "3. 常用项的含义：",
            "- **TDP 限制**：APU 功耗上限（瓦），对应快捷菜单 → 性能 → TDP 限制。",
            "- **充电上限 (max charge level)**：电池充到这个百分比就停止，长期插电使用时设为 80% 左右可以减缓电池老化。插着电源但停在该值、显示「未充电」是正常现象。",
            "- **GPU 性能级别**：`auto` 自动；`low`/`high` 锁定最低/最高频率；`manual` 配合 `set-manual-gpu-clock` 手动指定频率。",
            "- **CPU 调度器**：`none` 为内核默认调度器；`lavd` 是基于 sched_ext 的「延迟关键度感知」调度器，面向游戏的交互延迟优化。",
            "- **风扇控制**：`os` 由 jupiter-fan-control 服务控制（更安静），`bios` 交给固件曲线。",
            "4. 部分命令可能提示 `Unknown interface`：说明当前系统版本或机型不支持该功能，不是出错。",
        ],

        Consequences =
        [
            "• 所有 `get-*` 命令只读。",
            "• `set-*` 立即生效，范围由 steamos-manager 检查，普通用户无法设出危险值。游戏模式中快捷菜单里的设置可能按「每个游戏的配置」再次覆盖它们。",
            "• 把 GPU 锁定在 `high` 或把 TDP 调高会增加发热和耗电；锁 `low` 会降低游戏帧率。不确定时恢复 `auto`。",
            "• `switch-to-desktop-mode` / `switch-to-game-mode` 会立即结束当前会话，未保存的内容会丢失。",
        ],

        Steps =
        [
            Step.Read(
                "列出全部可用的子命令",
                "steamosctl --help",
                "`get-all-properties` 会一次性列出 steamos-manager 的所有属性。"),
            Step.Read(
                "查看机型",
                "steamosctl get-device-model",
                "`Jupiter` 为 LCD 版，`Galileo` 为 OLED 版。"),
            Step.Read(
                "查看 TDP 限制及其允许范围",
                "steamosctl get-tdp-limit; steamosctl get-tdp-limit-min; steamosctl get-tdp-limit-max",
                "单位为瓦。"),
            Step.Change(
                "设置 TDP 限制",
                "steamosctl set-tdp-limit <瓦数>",
                "例如 `steamosctl set-tdp-limit 10`。取值必须在上一步的范围内。"),
            Step.Read(
                "查看电池充电上限",
                "steamosctl get-max-charge-level; steamosctl suggested-minimum-charge-limit",
                "第二条是系统建议的最低可设值。"),
            Step.Change(
                "设置电池充电上限",
                "steamosctl set-max-charge-level <百分比>",
                "长期插电时设为 80；需要出门用满电前设为 100。"),
            Step.Read(
                "查看 GPU 性能级别与 CPU 状态",
                "steamosctl get-gpu-performance-level; steamosctl get-cpu-scaling-governor; steamosctl get-cpu-boost-state",
                "默认分别为 `auto`、`powersave`（配合 amd-pstate-epp 由硬件自动调频）、`enabled`。"),
            Step.Change(
                "设置 GPU 性能级别",
                "steamosctl set-gpu-performance-level <级别>",
                "`<级别>` 取 `auto`、`low` 或 `high`。恢复默认用 `auto`。"),
            Step.Read(
                "查看可用的 CPU 调度器",
                "steamosctl get-available-cpu-schedulers; steamosctl get-cpu-scheduler",
                "`none` 表示使用内核默认调度器。"),
            Step.Change(
                "切换 CPU 调度器",
                "steamosctl set-cpu-scheduler <调度器>",
                "`<调度器>` 取 `lavd`（启用 sched_ext 的 LAVD 调度器）或 `none`（恢复默认）。效果因游戏而异，可配合性能浮层对比帧时间。"),
            Step.Read(
                "查看风扇控制方式",
                "steamosctl get-fan-control-state",
                "`OS` 表示由系统的 jupiter-fan-control 服务控制。"),
            Step.Change(
                "从终端切换到游戏模式",
                "steamosctl switch-to-game-mode",
                "会立即结束桌面会话，先保存好文件。反向操作为 `steamosctl switch-to-desktop-mode`。"),
        ],

        Verify =
        [
            "1. 用 `set-tdp-limit` 修改后，`get-tdp-limit` 返回新值，快捷菜单 → 性能中的 TDP 滑块也同步变化；`sensors` 中 amdgpu 的 PPT 上限 (`cap`) 随之改变。",
            "2. 设置充电上限后插上电源，电量达到该值时 `cat /sys/class/power_supply/BAT1/status` 显示 `Not charging`。",
        ],
    };
}
