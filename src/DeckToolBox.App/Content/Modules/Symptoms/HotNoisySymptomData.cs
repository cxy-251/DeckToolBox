using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class HotNoisySymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-hot-noisy",
        Title = "风扇很吵或机身很烫",
        Keywords = "风扇 · 噪音 · 发热 · 温度 · jupiter-fan-control · TDP",
        Level = ModuleLevel.Basic,
        Overview = "风扇一直高速转、声音很大，或者机身摸起来很烫。先看温度与风扇转速是否匹配，再确认风扇控制服务在运行，最后找出是什么在持续占用 CPU 或 GPU。",

        Why =
        [
            "1. **负载高**：游戏、着色器编译、后台下载解压都会让 APU 满负荷，风扇随之加速，这是正常的。",
            "2. **风扇控制服务没有运行**：SteamOS 的 `jupiter-fan-control` 比固件默认曲线更安静；服务停止时风扇交给固件控制，噪音可能变大。",
            "3. **出风口被挡住或积灰**：温度高、风扇满速却降不下来。",
        ],

        Consequences =
        [
            "• 下面的检查都是只读的。",
        ],

        Steps =
        [
            Step.Read(
                "温度、功耗与风扇转速",
                "sensors | grep -E 'edge|PPT|Fan|temp1'",
                "`edge` 是 APU 温度，`PPT` 是功耗，`System Fan` 是风扇转速。").Then("空闲时 `edge` 一般在 40~60 °C，风扇两千转上下；满载时温度 80~90 °C、风扇四五千转都属正常。**空闲时风扇仍然很快** → 继续第 2、3 步。"),
            Step.Read(
                "风扇控制服务是否在运行",
                "systemctl is-active jupiter-fan-control; steamosctl get-fan-control-state",
                "第二条显示当前由系统服务 (`OS`) 还是固件 (`BIOS`) 控制风扇。").Then("正常时为 `active` 与 `Fan control state: OS`。**不是 `active`** → 执行 `sudo systemctl restart jupiter-fan-control`，或重启一次。"),
            Step.Read(
                "是什么在占用 CPU",
                "ps -eo pid,%cpu,comm --sort=-%cpu | head -n 6",
                "按 CPU 占用从高到低列出前 5 个进程。").Then("**某个后台程序（如 `fossilize_replay` 着色器编译、浏览器）长期占用很高** → 等它完成或关掉它，风扇随后会降下来。"),
        ],

        Verify =
        [
            "• 空闲几分钟后，`sensors` 中温度回落、风扇转速降低。",
        ],

        Related = ["deck-hardware-ec", "steamosctl", "process-basics", "sym-game-stutter"],
    };
}
