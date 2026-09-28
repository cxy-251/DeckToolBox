using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Network;

public static class SshAdvancedData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-ssh-advanced",
        Title = "SSH 进阶：配置文件与端口转发",
        Keywords = "~/.ssh/config · -L · -R · -D · socat · 跳板",
        Level = ModuleLevel.Advanced,
        Overview = "经常连接的主机，可以在 ~/.ssh/config 中起个短名字，省去每次输入 IP、用户名和端口。SSH 还能把端口「转发」到另一端：访问只在远端本机开放的网页服务，或让远端访问你这边的服务。socat 则可以在本机临时转发端口或搭一个测试服务。基础用法见入门必修的「SSH 远程登录与传文件」。",

        Why =
        [
            "1. `~/.ssh/config`：每个 `Host 别名` 段落定义一台主机：",
            "- `HostName` 实际地址、`User` 用户名、`Port` 端口、`IdentityFile` 使用的私钥。",
            "- 定义后 `ssh 别名`、`scp 文件 别名:路径`、`rsync ... 别名:路径` 都能直接使用。",
            "- 文件权限应为 600（只有自己可读写），否则 ssh 会拒绝使用。",
            "2. 端口转发：",
            "- 本地转发 `-L 本地端口:目标地址:目标端口`：访问本机的某个端口，实际经 SSH 连到远端能访问的目标。常用于访问只监听在远端 `127.0.0.1` 上的管理页面。",
            "- 远程转发 `-R 远端端口:本地地址:本地端口`：让远端机器访问你这边的服务。",
            "- 动态转发 `-D 端口`：在本机开一个 SOCKS5 代理，所有流量经远端机器发出。",
            "- `-N` 不执行远程命令只做转发，`-f` 转入后台。",
            "3. socat：功能强大的数据转发工具，`socat TCP-LISTEN:本地端口,fork TCP:目标:端口` 在本机把一个端口转发到另一个地址；也可以临时充当一个简单的服务端，用来测试防火墙或连通性。",
        ],

        Consequences =
        [
            "• 转发和 socat 监听的端口只在命令运行期间存在；按 `Ctrl+C` 或结束进程即关闭。",
            "• 监听在 `0.0.0.0` 上的端口对整个局域网可见（在防火墙允许的范围内），只需本机访问时监听 `127.0.0.1`。",
            "• 通过 `-D` 代理的流量由远端机器发出，遵守远端所在网络的规定。",
        ],

        Steps =
        [
            Step.Change(
                "为常用主机写一个配置段",
                "mkdir -p ~/.ssh && chmod 700 ~/.ssh && cat >> ~/.ssh/config <<'EOF'\n\nHost pc\n    HostName <IP或主机名>\n    User <用户名>\n    Port 22\nEOF\nchmod 600 ~/.ssh/config",
                "`>>` 追加，不覆盖已有配置。").Then("没有输出；之后 `ssh pc` 等同于 `ssh <用户名>@<IP或主机名>`。"),
            Step.Read(
                "查看 ssh 对某个别名解析出的完整设置",
                "ssh -G pc | grep -E '^(hostname|user|port|identityfile) '",
                "`-G` 只打印最终生效的配置，不连接。").Then("输出四行，如 `hostname 192.168.0.10`、`user xxx`、`port 22`，确认配置写对了。"),
            Step.Change(
                "本地端口转发：访问远端才能打开的网页（示例）",
                "ssh -N -L 8080:127.0.0.1:<远端服务端口> pc",
                "保持运行期间，在 Deck 浏览器打开 `http://127.0.0.1:8080`；`Ctrl+C` 结束。").Then("命令不输出任何内容并保持运行；浏览器中看到的是远端机器上那个只在本机开放的服务页面。"),
            Step.Change(
                "动态转发：开一个经由远端的 SOCKS5 代理（示例）",
                "ssh -N -D 1080 pc",
                "浏览器或程序的代理设置为 SOCKS5 `127.0.0.1:1080`。").Then("命令保持运行；使用该代理的程序，其网络流量都从远端机器发出。"),
            Step.Change(
                "用 socat 临时开一个测试服务",
                "socat -T3 TCP-LISTEN:18080,reuseaddr SYSTEM:'echo HTTP/1.0 200 OK; echo; echo 来自 socat 的测试页面' &",
                "在后台监听 18080 端口，每次连接返回一小段 HTTP 响应。").Then("显示作业号与 PID；此时本机和局域网中的设备访问 `http://<Deck的IP>:18080` 都会看到「来自 socat 的测试页面」，可用来验证防火墙和连通性。"),
            Step.Read(
                "访问测试服务",
                "curl -s --noproxy '*' http://127.0.0.1:18080",
                "`--noproxy '*'` 避免请求被发往代理。").Then("输出 `来自 socat 的测试页面`。socat 响应一次后退出（没有 `fork` 选项）。"),
        ],

        Verify =
        [
            "1. 在另一台电脑上 `ssh -G` 同样的别名，确认两边的配置一致。",
        ],
    };
}
