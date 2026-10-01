using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Network;

public static class CurlWgetData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-curl-wget",
        Title = "curl 与 wget：下载与请求",
        Keywords = "curl -L -o -O -I -C - · wget -c · 代理 · API",
        Level = ModuleLevel.Basic,
        Overview = "curl 和 wget 都能从命令行下载文件。wget 擅长「把东西下载下来」，支持断点续传和递归下载；curl 擅长「发送一个请求看结果」，能查看响应头、调用网页 API、测量耗时。两者 SteamOS 都自带。",

        Why =
        [
            "1. curl 常用选项：",
            "- `-o 文件名` 保存为指定文件；`-O` 使用网址中的文件名保存。不加时内容直接输出到终端。",
            "- `-L`：跟随跳转（很多下载链接会先 302 跳转到真正的地址，不加 `-L` 只会下载到一个很小的跳转页面）。",
            "- `-C -`：断点续传，从已下载的部分继续。",
            "- `-I`：只取响应头，查看状态码、文件大小、类型。",
            "- `-s` 静默，`-S` 静默时仍显示错误，`-f` HTTP 出错时返回非零退出码（脚本中常用 `-fsSL`）。",
            "- `-w '%{http_code}'`：输出状态码、耗时等统计信息。",
            "- `-H '名称: 值'` 添加请求头；`-d '数据'` 发送 POST 数据。",
            "2. wget 常用选项：",
            "- `wget 网址`：下载到当前目录，显示进度条。",
            "- `-c`：断点续传；`-O 文件名`：指定保存名；`-P 目录`：保存到目录。",
            "- `-i 列表.txt`：批量下载文件中列出的每个网址。",
            "- `--limit-rate=2m`：限速，避免占满带宽。",
            "3. 代理：两者都读取 `http_proxy`、`https_proxy`（及大写形式）环境变量。设置了代理时，`curl -I` 的第一行会是代理返回的 `HTTP/1.1 200 Connection established`，真正的响应头在它之后。临时绕过代理：curl 加 `--noproxy '*'`，wget 加 `--no-proxy`。",
        ],

        Consequences =
        [
            "• 下载会写入文件，同名文件会被覆盖（curl `-o`、wget `-O`）；wget 默认遇到同名文件会自动改名为 `文件.1`。",
            "• 执行 `curl 网址 | bash` 这类「下载并直接运行」的命令前，先下载下来阅读内容。",
            "• 从网上下载的程序与脚本需要自行判断可信度，并核对发布者提供的校验值（见「校验与加密」）。",
        ],

        Steps =
        [
            Step.Read(
                "只查看响应头",
                "curl -sI https://www.kernel.org | head -n 8",
                "`-I` 只请求头信息。").Then("第一行是状态行，如 `HTTP/2 200`（经过代理时先出现一行 `HTTP/1.1 200 Connection established`），随后是 `content-type`、`date` 等响应头。"),
            Step.Read(
                "测量访问耗时与速度",
                "curl -s -o /dev/null -w '状态 %{http_code}  DNS %{time_namelookup}s  连接 %{time_connect}s  总计 %{time_total}s  速度 %{speed_download} B/s\\n' https://www.kernel.org",
                "`-w` 在结束时输出统计变量。").Then("输出一行各阶段耗时，如 `状态 200  DNS 0.01s  连接 0.05s  总计 0.6s  速度 ...`，可以用来判断慢在解析、连接还是传输。"),
            Step.Change(
                "下载文件：跟随跳转并使用原文件名",
                "cd ~/Downloads && curl -LO https://cdn.kernel.org/pub/linux/kernel/v6.x/sha256sums.asc",
                "`-L` 跟随跳转，`-O` 使用网址末尾的文件名。").Then("显示一张进度表（总大小、已下载、速度、剩余时间），完成后 `~/Downloads/sha256sums.asc` 出现。"),
            Step.Change(
                "用 wget 下载并支持断点续传",
                "wget -c -P ~/Downloads https://cdn.kernel.org/pub/linux/kernel/v6.x/sha256sums.asc",
                "`-c` 已有同名的部分文件时从断点继续；`-P` 指定目录。").Then("显示连接过程和进度条；文件已完整存在时提示「文件已下载完成；不会进行任何操作。」。"),
            Step.Change(
                "批量下载一个列表中的所有文件（示例）",
                "wget -c -i <网址列表>.txt -P ~/Downloads --limit-rate=5m",
                "`<网址列表>.txt` 替换为你保存的文本文件路径，内容每行一个 URL；`--limit-rate` 限速。").Then("依次下载每个网址，每个文件一条进度条，最后汇总下载了多少文件、总大小和耗时。"),
            Step.Read(
                "调用一个返回 JSON 的接口",
                "curl -fsSL 'https://api.github.com/repos/ValveSoftware/Proton/releases/latest' | jq -r '.tag_name, .published_at'",
                "`-fsSL` 是脚本中最常用的组合；jq 提取需要的字段。").Then("输出两行：Proton 最新发布的版本号（如 `proton-11.0-2`）与发布时间。仓库没有正式发布时接口返回 404，`-f` 会让 curl 报错退出。"),
        ],

        Verify =
        [
            "1. `curl -o /dev/null -s -w '%{http_code}\\n' https://www.kernel.org/不存在的页面` 输出 `404`。",
        ],
    };
}
