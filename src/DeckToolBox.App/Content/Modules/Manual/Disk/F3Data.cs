using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Disk;

public static class F3Data
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-f3",
        Title = "f3：检测扩容假卡",
        Keywords = "f3write · f3read · f3probe · SD 卡 · U 盘 · 实际容量",
        Level = ModuleLevel.Basic,
        Overview = "有些廉价的 SD 卡、U 盘被篡改了固件，系统显示 1 TB，实际只能存几十 GB，写满后前面的数据会被悄悄覆盖。f3（Fight Flash Fraud）通过写满再读回校验的方式检测真实容量。SteamOS 自带这套工具，新买的存储卡建议先测一遍再存游戏。",

        Why =
        [
            "1. 扩容假卡的原理：固件向系统报告一个虚假的大容量，超过真实容量的写入会被映射回已用区域，覆盖旧数据。表面上一切正常，直到读取文件时才发现损坏。",
            "2. f3write 与 f3read（普通用户即可运行，卡上已有文件不受影响）：",
            "- `f3write 挂载点`：在卡的剩余空间中写满 `1.h2w`、`2.h2w`… 每个 1 GB 的测试文件，每个文件的内容都可校验。",
            "- `f3read 挂载点`：读回这些文件并校验，报告 `Data OK`（完好）与 `Data LOST`（丢失）各多少。`Data LOST` 不为 0 即说明有问题。",
            "- 两者还分别报告平均写入与读取速度，可以顺便判断速度标称是否属实。",
            "- `--end-at=N` 只写到第 N 个文件，可以先快速抽查。",
            "3. f3probe（需要 root，会破坏卡上数据）：直接探测块设备，几分钟内给出真实容量与伪造类型，不需要写满。",
            "4. 发现是假卡后，可以用 `f3fix` 把分区缩小到真实容量继续使用，但更稳妥的做法是退货。",
        ],

        Consequences =
        [
            "• f3write 会把卡写满，需要的时间等于「剩余容量 ÷ 写入速度」，1 TB 的卡可能需要数小时；测完后删除 `*.h2w` 文件即可恢复空间。",
            "• **f3probe 会破坏卡上的全部数据**，只用于新卡或已备份的卡。",
            "• 测试期间保持供电，不要让 Deck 休眠（可以配合 `systemd-inhibit`，见「电源、休眠与防止休眠」）。",
        ],

        Steps =
        [
            Step.Read(
                "确认存储卡的挂载点与剩余空间",
                "df -h /run/media/deck/*",
                "SD 卡通常挂载在 `/run/media/deck/<卷标>`。").Then("显示每张卡的容量、已用、可用空间与挂载点；f3write 会用掉「可用」这一列的空间。"),
            Step.Change(
                "先快速抽查：只写 2 GB",
                "mkdir -p /run/media/deck/<卷标>/f3test && f3write --end-at=2 /run/media/deck/<卷标>/f3test",
                "`<卷标>` 由第 1 步 `df -h` 查得。测试文件写在单独子目录中。").Then("显示 `Free space: ...`、`Creating file 1.h2w ... OK!`、`Creating file 2.h2w ... OK!`，最后是平均写入速度。"),
            Step.Read(
                "读回校验",
                "f3read /run/media/deck/<卷标>/f3test",
                "`<卷标>` 由第 1 步查得。逐个读取并校验测试文件。").Then("每个文件一行校验结果，最后汇总 `Data OK: 2.00 GB`、`Data LOST: 0.00 Byte`，以及平均读取速度。`Data LOST` 不为 0 说明卡有问题。"),
            Step.Change(
                "完整测试：写满整张卡（耗时较长）",
                "systemd-inhibit --what=sleep:idle --why='f3 测试' f3write /run/media/deck/<卷标>/f3test && f3read /run/media/deck/<卷标>/f3test",
                "`<卷标>` 由第 1 步查得。`systemd-inhibit` 防止测试期间系统休眠。").Then("先把剩余空间写满（终端持续显示进度与剩余时间），再全部读回校验，最终的 `Data OK` 应接近卡的可用容量，`Data LOST` 为 0。"),
            Step.Change(
                "删除测试文件，恢复空间",
                "rm -r /run/media/deck/<卷标>/f3test && df -h /run/media/deck/<卷标>",
                "`<卷标>` 由第 1 步查得。删除测试文件恢复存储空间。").Then("可用空间恢复到测试前的数值。"),
            Step.Danger(
                "快速探测真实容量（会清除卡上数据）",
                "sudo f3probe --destructive --time-ops /dev/<磁盘名>",
                "**会破坏卡上的全部数据。**`<磁盘名>` 如 `mmcblk0` 或 `sda`，是整个设备而不是分区。").Then("几分钟后给出结论：正常卡显示 `Good news: The device ... is the real thing`；假卡显示 `Bad news` 以及真实容量 (`usable size`)。"),
        ],

        Verify =
        [
            "1. 完整测试后 `Data LOST` 为 0，且 `Data OK` 接近 `df -h` 显示的可用空间，说明这张卡容量真实。",
        ],
    };
}
