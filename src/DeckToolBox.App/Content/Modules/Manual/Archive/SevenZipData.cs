using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Archive;

public static class SevenZipData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-7z",
        Title = "7z 与 unrar：7z、rar 与加密分卷",
        Keywords = "7z a · x · l · t · -p · -mhe · -v · unrar",
        Level = ModuleLevel.Basic,
        Overview = "7z 能解压几乎所有常见格式（7z、zip、rar、tar、iso、cab 等），自己的 .7z 格式压缩率高，并支持 AES-256 加密与分卷。rar 格式由系统自带的 unrar 解压。下载的游戏、Mod、资源包经常是这两种格式。",

        Why =
        [
            "1. 7z 的子命令（写在最前面）：",
            "- `a`：添加文件到压缩包（创建）。",
            "- `x`：解压并保留目录结构（常用）；`e` 解压时把所有文件放到同一层（很少需要）。",
            "- `l`：列出内容；`t`：测试完整性。",
            "2. 常用开关：",
            "- `-o目录`：解压目标，**`-o` 与目录之间没有空格**。",
            "- `-p密码`：设置或提供密码；只写 `-p` 则交互输入。",
            "- `-mhe=on`：同时加密文件名（7z 格式），不输入密码连文件列表都看不到。",
            "- `-v100m`：按 100 MB 分卷，生成 `.7z.001`、`.7z.002`…；解压时对 `.001` 执行 `x` 即可。",
            "- `-mx=9`：最高压缩级别。",
            "3. unrar：`unrar l 文件.rar` 列出，`unrar x 文件.rar 目标目录/` 解压并保留目录结构。分卷 rar（`.part1.rar`、`.part2.rar`）对第一卷执行即可。",
            "4. 7z 也能解压 rar：`7z x 文件.rar`，两者选一即可。",
        ],

        Consequences =
        [
            "• 创建压缩包不影响原文件；解压遇到同名文件会询问是否覆盖。",
            "• 忘记 7z 加密密码无法恢复数据。",
            "• 分卷缺少任何一卷都无法完整解压；`7z t` 可以在解压前检查。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习数据",
                "mkdir -p /tmp/7z-demo/src && cd /tmp/7z-demo && seq 1 300000 > src/nums.txt && echo hi > src/a.txt",
                "`seq` 生成一个约 2 MB 的数字文件。").Then("没有输出，当前目录变为 `/tmp/7z-demo`。"),
            Step.Change(
                "创建 7z 压缩包",
                "7z a data.7z src",
                "`a` 表示添加。").Then("输出扫描与压缩信息，最后一行 `Everything is Ok`，生成体积远小于原数据的 `data.7z`。"),
            Step.Read(
                "列出与测试",
                "7z l data.7z | tail -n 4; 7z t data.7z | tail -n 3",
                "`l` 列出，`t` 测试完整性。").Then("先显示文件列表的末尾和合计行；测试部分以 `Everything is Ok` 结尾。"),
            Step.Change(
                "创建加密且隐藏文件名的压缩包",
                "7z a -p -mhe=on secret.7z src",
                "`-p` 后不写密码则交互输入两次。").Then("提示 `Enter password:` 与 `Verify password:`；之后执行 `7z l secret.7z` 也会要求输入密码，否则连文件名都无法看到。"),
            Step.Change(
                "按大小分卷压缩",
                "head -c 3M /dev/urandom > src/random.bin && 7z a -v1m parts.7z src/random.bin && ls parts.7z.*",
                "随机数据无法压缩，3 MB 的文件按 1 MB 分卷。").Then("生成 `parts.7z.001`、`parts.7z.002`、`parts.7z.003`、`parts.7z.004` 四个分卷。"),
            Step.Change(
                "解压到指定目录",
                "7z x data.7z -oout && ls out/src",
                "`-o` 与目录名之间不加空格。").Then("输出 `Everything is Ok`，`ls` 显示 `a.txt  nums.txt`。"),
            Step.Change(
                "解压 rar 文件（示例）",
                "unrar x <文件>.rar <目标目录>/",
                "目标目录末尾要带 `/`；也可以用 `7z x <文件>.rar`。").Then("逐个显示 `Extracting ... OK`，最后 `All OK`。"),
        ],

        Verify =
        [
            "1. `7z t parts.7z.001` 测试全部分卷，以 `Everything is Ok` 结尾。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/7z-demo` 清理。",
        ],
    };
}
