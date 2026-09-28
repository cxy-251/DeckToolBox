using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class ChecksumCryptoData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "checksum-crypto",
        Title = "校验与加密：sha256sum、gpg、openssl",
        Keywords = "sha256sum · gpg -c · openssl rand",
        Level = ModuleLevel.Basic,
        Overview = "下载的文件有没有损坏或被篡改？想把一个文件加密后放到网盘？需要一个随机密码？这三件事用系统自带的 sha256sum、gpg 和 openssl 都能完成。",

        Why =
        [
            "1. 校验和 (checksum)：",
            "• 对文件内容计算出一个固定长度的「指纹」，内容改动哪怕一个字节，指纹也完全不同。",
            "• 软件发布页常附带 SHA-256 值。下载后自己计算一遍并对比，一致才说明文件完整、未被替换。",
            "• 只防损坏和替换，不证明发布者身份；要验证发布者，需要检查 GPG 签名（`.sig`/`.asc` 文件）。",
            "2. 对称加密 `gpg -c`：用一个口令加密文件，生成 `.gpg` 文件；解密时输入同一个口令。适合加密备份、上传网盘前的私密文件。口令丢失就无法解密。",
            "3. `openssl rand`：从系统的密码学安全随机数源生成随机数据，可用来生成密码或密钥。",
            "4. `b2sum` 也是系统自带的校验工具（BLAKE2 算法），速度比 sha256sum 更快，适合校验大量文件的备份。",
        ],

        Consequences =
        [
            "• 计算校验和是只读操作。",
            "• `gpg -c` 生成新的加密文件，**不会删除原文件**；确认加密文件可以正常解密后，再自行删除原文件。",
            "• 第一次运行 gpg 会创建 `~/.gnupg` 目录，属于正常现象。",
            "• 口令不要写进命令行参数（会留在命令历史中），让 gpg 弹窗或提示输入。",
        ],

        Steps =
        [
            Step.Read(
                "计算文件的 SHA-256",
                "sha256sum <文件>",
                "把输出与发布页上的值逐字对比。"),
            Step.Read(
                "按发布页提供的校验文件批量校验",
                "sha256sum -c <校验文件>",
                "校验文件每行格式为「哈希值  文件名」。每个文件显示「成功」或 `OK` 即为一致。"),
            Step.Change(
                "为一个目录生成校验清单（用于日后检查备份是否完好）",
                "cd <目录> && find . -type f ! -name SHA256SUMS -exec sha256sum {} + > SHA256SUMS",
                "之后在同一目录执行 `sha256sum -c --quiet SHA256SUMS`，只列出不一致的文件。"),
            Step.Change(
                "用口令加密一个文件",
                "gpg -c <文件>",
                "会提示输入两次口令，生成 `<文件>.gpg`。"),
            Step.Change(
                "解密",
                "gpg -o <输出文件> -d <文件>.gpg",
                "输入加密时使用的口令。"),
            Step.Read(
                "生成一个随机密码",
                "openssl rand -base64 18",
                "18 字节随机数据编码后为 24 个字符。"),
        ],

        Verify =
        [
            "1. 对同一文件计算两次 `sha256sum`，结果相同；修改文件中任意一个字符后再计算，结果完全不同。",
            "2. 解密出的文件与原文件的 `sha256sum` 一致。",
        ],
    };
}
