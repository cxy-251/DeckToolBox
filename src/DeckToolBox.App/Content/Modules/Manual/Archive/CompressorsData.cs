using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Archive;

public static class CompressorsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-compressors",
        Title = "gzip、xz、zstd：压缩算法对比",
        Keywords = "gzip · bzip2 · xz · zstd · lz4 · 压缩级别 · -T0",
        Level = ModuleLevel.Intermediate,
        Overview = "gzip、bzip2、xz、zstd、lz4 都只压缩单个文件（目录要先用 tar 打包）。它们在速度和压缩率之间各有取舍。下面的对比数据是在 Steam Deck 上压缩 141 MB 的 Python 标准库实测得到的。",

        Why =
        [
            "1. Steam Deck 实测（141 MB 原始数据，默认级别，xz 与 zstd -19 使用全部线程）：",
            "- `lz4`：51 MB，0.2 秒。最快，压缩率最低，适合对速度敏感的场合。",
            "- `zstd`（默认级别 3）：29 MB，1.0 秒。速度与压缩率最均衡，推荐作为日常首选。",
            "- `gzip`：39 MB，5.1 秒。最通用，任何系统都能解压。",
            "- `bzip2`：29 MB，13 秒。较老，现已少用。",
            "- `xz`：22 MB，17 秒。压缩率最高，适合发布一次、下载很多次的文件。",
            "- `zstd -19`：23 MB，29 秒。接近 xz 的压缩率，且解压速度远快于 xz。",
            "2. 通用选项（各工具基本一致）：",
            "- 默认压缩后**删除原文件**，只保留 `.gz`/`.xz`/`.zst`；`-k` 保留原文件。",
            "- `-d` 解压（或用 `gunzip`、`unxz`、`unzstd`）。",
            "- `-1` 到 `-9`（zstd 到 `-19`）：压缩级别，数字越大越慢、压得越小。",
            "- `-T0`（xz、zstd）：使用全部 CPU 线程。",
            "- `-c`：输出到标准输出，便于接管道。",
            "- `-l`（gzip、xz）/ `zstd -l`：查看压缩前后的大小。",
            "3. 已经压缩过的数据（视频、图片、游戏资源包、zip）再压缩几乎不会变小，只会浪费时间。",
            "4. 查看压缩文件内容而不解压：`zcat`、`xzcat`、`zstdcat` 输出解压后的内容，`zgrep` 直接在 `.gz` 中搜索。",
        ],

        Consequences =
        [
            "• **不加 `-k` 时原文件会被删除**，只剩压缩文件。",
            "• 高压缩级别会占满 CPU 并明显发热耗电，大文件建议插着电源进行。",
            "• 解压时若同名文件已存在，工具会询问或报错，不会静默覆盖（加 `-f` 强制覆盖）。",
        ],

        Steps =
        [
            Step.Change(
                "准备一个测试文件",
                "mkdir -p /tmp/cmp-demo && cd /tmp/cmp-demo && tar -cf data.tar -C /usr/lib python3.13 && ls -lh data.tar",
                "把系统的 Python 标准库打包作为测试数据（约 140 MB）。").Then("显示 `data.tar` 的大小，约 `141M`。"),
            Step.Change(
                "用不同算法压缩并计时",
                "for c in 'gzip -k' 'xz -k -T0' 'zstd -q -k' 'lz4 -q -k'; do TIMEFORMAT=\"%R 秒  $c\"; time $c data.tar; done; ls -lh data.tar.*",
                "`-k` 保留原文件；bash 内建的 `time` 按 `TIMEFORMAT` 的格式输出耗时（`%R` 为实际秒数）。xz 需要十几秒。").Then("每种算法一行耗时，最后列出四个压缩文件及其大小：`.lz4` 最大、`.xz` 最小、`.zst` 居中但耗时约 1 秒。"),
            Step.Read(
                "查看压缩比",
                "gzip -l data.tar.gz; xz -l data.tar.xz; zstd -l data.tar.zst",
                "`-l` 显示压缩前后大小。").Then("每个工具输出一张小表，包含压缩前、压缩后大小和压缩比（ratio）。"),
            Step.Read(
                "不解压直接查看内容",
                "zstdcat data.tar.zst | tar -t | sed -n 1,5p",
                "`zstdcat` 输出解压后的数据流，直接交给 tar 列出。").Then("列出包中的前 5 个路径，磁盘上不产生任何解压文件。"),
            Step.Change(
                "解压并比较",
                "zstd -d -o restored.tar data.tar.zst && cmp data.tar restored.tar && echo 完全一致",
                "`-d` 解压，`-o` 指定输出文件名。").Then("输出 `data.tar.zst: 147... bytes` 之类的一行解压信息，然后是「完全一致」。"),
        ],

        Verify =
        [
            "1. `ls -l data.tar restored.tar` 两个文件大小完全相同。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/cmp-demo` 清理。",
        ],
    };
}
