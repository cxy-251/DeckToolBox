using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Media;

public static class PdfToolsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-pdf-tools",
        Title = "PDF 工具：合并、拆分、提取、压缩",
        Keywords = "pdfinfo · pdftotext · pdfunite · pdfseparate · pdftoppm · qpdf · gs",
        Level = ModuleLevel.Basic,
        Overview = "SteamOS 自带 poppler、qpdf 和 Ghostscript 三套 PDF 工具，不需要安装任何软件就能查看 PDF 信息、提取文字和图片、合并与拆分、转成图片、加解密，以及压缩体积。",

        Why =
        [
            "1. poppler 工具集（命令以 `pdf` 开头）：",
            "- `pdfinfo`：页数、页面尺寸、作者、创建程序等。",
            "- `pdftotext 文件.pdf 输出.txt`：提取文字（`-layout` 尽量保持排版；输出写 `-` 表示打印到终端）。扫描件是图片，提取不出文字。",
            "- `pdfimages -list` 列出内嵌图片，`pdfimages -png 文件.pdf 前缀` 导出它们。",
            "- `pdfunite a.pdf b.pdf 输出.pdf`：合并；`pdfseparate 文件.pdf page-%d.pdf`：每页拆成一个文件（`-f`、`-l` 指定起止页）。",
            "- `pdftoppm -png -r 150 文件.pdf 前缀`：每页转为 PNG 图片，`-r` 为分辨率 (DPI)。",
            "2. qpdf：结构层面的处理。",
            "- `--pages 文件 1-3,5 --` 按页码提取、重组。",
            "- `--encrypt 用户密码 所有者密码 256 --` 加密；`--decrypt --password=密码` 解密。",
            "- `--check` 检查文件结构。",
            "3. Ghostscript (`gs`)：重新渲染 PDF，最常用于压缩体积：`-dPDFSETTINGS=` 取 `/screen`（最小，72 dpi）、`/ebook`（150 dpi）、`/printer`（300 dpi）。图片多的 PDF 压缩效果明显；本身已经很小的 PDF 反而可能变大。",
        ],

        Consequences =
        [
            "• 这些工具都生成新文件，不修改原 PDF；输出文件名已存在时会被覆盖。",
            "• gs 压缩会降低图片清晰度，压缩后先检查效果再删除原文件。",
            "• 忘记加密 PDF 的密码无法解密。",
        ],

        Steps =
        [
            Step.Change(
                "准备两个练习用的 PDF",
                "mkdir -p /tmp/pdf-demo && cd /tmp/pdf-demo && cp /usr/share/cups/data/classified.pdf a.pdf && cp /usr/share/cups/data/confidential.pdf b.pdf",
                "系统自带的打印测试页。").Then("没有输出，当前目录变为 `/tmp/pdf-demo`。").Practice(),
            Step.Read(
                "查看 PDF 信息",
                "pdfinfo a.pdf",
                "对任何 PDF 都适用。").Then("列出 `Producer: GPL Ghostscript 9.14`、`CreationDate`、`Pages: 1`、`Page size` 等。"),
            Step.Read(
                "提取文字",
                "pdftotext -layout a.pdf - | head",
                "`-` 表示输出到终端。").Then("输出页面上的文字，这里是 `Classified`。"),
            Step.Change(
                "合并两个 PDF",
                "pdfunite a.pdf b.pdf ab.pdf && pdfinfo ab.pdf | grep Pages",
                "按参数顺序合并，最后一个参数是输出文件。").Then("输出 `Pages: 2`。").Practice(),
            Step.Change(
                "拆分为单页文件",
                "pdfseparate ab.pdf page-%d.pdf && ls page-*",
                "`%d` 替换为页码。").Then("生成 `page-1.pdf` 与 `page-2.pdf`。").Practice(),
            Step.Change(
                "按页码重组（取第 2 页在前、第 1 页在后）",
                "qpdf --empty --pages ab.pdf 2,1 -- reordered.pdf && pdftotext reordered.pdf - | head -n 1",
                "`--empty` 从空文档开始，`--pages` 指定来源与页码。").Then("生成 `reordered.pdf`，第一页的文字为原来第 2 页的内容 `Confidential`。").Practice(),
            Step.Change(
                "把第一页转成图片",
                "pdftoppm -png -r 100 -f 1 -l 1 ab.pdf thumb && ls thumb*",
                "`-f 1 -l 1` 只转第 1 页，`-r 100` 为 100 DPI。").Then("生成 `thumb-1.png`，可以用图片查看器打开。").Practice(),
            Step.Change(
                "加密与解密",
                "qpdf --encrypt 1234 owner 256 -- ab.pdf locked.pdf && qpdf --decrypt --password=1234 locked.pdf unlocked.pdf && ls",
                "AES-256 加密；打开 `locked.pdf` 需要密码 1234。").Then("生成 `locked.pdf`（打开需要密码）与 `unlocked.pdf`（已解除密码）。本例的测试文件结构不规范，qpdf 可能附带 `operation succeeded with warnings` 警告，不影响结果。").Practice(),
            Step.Change(
                "压缩一个图片较多的 PDF（示例）",
                "gs -q -sDEVICE=pdfwrite -dPDFSETTINGS=/ebook -o <输出>.pdf <输入>.pdf && ls -lh <输入>.pdf <输出>.pdf",
                "`<输入>.pdf` 为源文件；`<输出>.pdf` 为目标文件。`-dPDFSETTINGS=/ebook` 重采样图片至 150 DPI。").Then("生成压缩后的 PDF，`ls` 对比前后大小；扫描件、图片多的文档通常能缩小到原来的几分之一。"),
        ],

        Verify =
        [
            "1. `pdfinfo reordered.pdf | grep Pages` 输出 `Pages: 2`。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/pdf-demo` 清理。",
        ],
    };
}
