using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Archive;

public static class ZipData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-zip",
        Title = "zip 与 unzip",
        Keywords = "zip -r · -e · -x · unzip -l · -d · -O 编码",
        Level = ModuleLevel.Basic,
        Overview = "zip 是与 Windows、macOS 交换文件时最通用的格式。本模块讲清楚打包目录、排除文件、加密码、查看和解压，以及解压 Windows 上压缩的 zip 时中文文件名变成乱码的处理方法。",

        Why =
        [
            "1. zip：",
            "- `zip -r 包.zip 目录`：打包目录（`-r` 递归，不加只会存入空目录）。",
            "- `-x '模式'`：排除文件，如 `-x '*.log' '*/.git/*'`。",
            "- `-e`：交互式设置密码；`-9` 最高压缩。",
            "- 对已存在的 zip 再次执行，会更新或追加其中的文件。",
            "2. unzip：",
            "- `unzip -l 包.zip`：只列出内容。",
            "- `unzip 包.zip -d 目录`：解压到指定目录（不存在会自动创建）。",
            "- `-o` 覆盖已有文件不询问，`-n` 从不覆盖。",
            "3. 中文文件名乱码：Windows 中文系统创建的 zip 常用 GBK 编码保存文件名，而不是 UTF-8。解压后文件名是乱码时，删掉结果，改用 `unzip -O GBK 包.zip` 重新解压；或用 `7z x`，它通常能自动识别。",
            "4. zip 的传统加密 (ZipCrypto) 强度很弱，只能防止无意间打开；需要可靠加密时使用 `7z -p`（AES-256）或 `gpg -c`。",
        ],

        Consequences =
        [
            "• 打包不影响原文件。",
            "• 解压遇到同名文件时，unzip 会逐个询问 `replace ...? [y]es, [n]o, [A]ll, [N]one, [r]ename`。",
            "• 命令行中写 `-P 密码` 会把密码留在命令历史里，优先用 `-e` 交互输入。",
        ],

        Steps =
        [
            Step.Change(
                "准备练习数据",
                "mkdir -p /tmp/zip-demo/docs && cd /tmp/zip-demo && echo 报告 > docs/report.txt && echo 调试 > docs/debug.log",
                "在 `/tmp/zip-demo` 中练习。").Then("没有输出，当前目录变为 `/tmp/zip-demo`。"),
            Step.Change(
                "打包目录并排除日志",
                "zip -r docs.zip docs -x '*.log'",
                "`-r` 递归扫描子目录，zip 默认基于 DEFLATE 压缩算法；`-x` 指定排除模式（带引号避免被 Shell 提前展开）。").Then("逐行输出 `adding: docs/ (stored 0%)`、`adding: docs/report.txt (stored 0%)`；没有 debug.log。"),
            Step.Read(
                "查看 zip 的内容",
                "unzip -l docs.zip",
                "`-l` 解析 Zip 文件末尾的中央目录（Central Directory）列出条目与未压缩大小，不解压写盘。").Then("表格列出每个文件的大小、日期和路径，最后一行是文件总数与总大小。"),
            Step.Change(
                "解压到指定目录",
                "unzip docs.zip -d out && find out",
                "`-d` 目标目录不存在时会自动创建。").Then("输出 `Archive: docs.zip`、`creating: out/docs/` 和 `extracting: out/docs/report.txt`（压缩过的文件显示为 `inflating:`），`find` 显示解出的文件。"),
            Step.Change(
                "创建带密码的 zip",
                "zip -r -e secret.zip docs",
                "`-e` 会两次提示输入密码，输入时不显示字符。").Then("提示 `Enter password:` 与 `Verify password:`，之后生成 `secret.zip`；用 `unzip secret.zip` 解压时会先要求输入密码。"),
            Step.Change(
                "解压 Windows 中文 zip 时修正文件名编码（示例）",
                "unzip -O GBK <文件>.zip -d <目标目录>",
                "`<文件>.zip` 为待解压包名；`<目标目录>` 为解压目标路径。`-O GBK` 解决 Windows ZIP 中文乱码。").Then("中文文件名正确显示，而不是 `╬─╝■` 之类的乱码。"),
        ],

        Verify =
        [
            "1. `unzip -t docs.zip` 输出 `No errors detected`，说明 zip 文件完整。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/zip-demo` 清理。",
        ],
    };
}
