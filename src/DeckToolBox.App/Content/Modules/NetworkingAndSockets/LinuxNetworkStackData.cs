using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NetworkingAndSockets;

public static class LinuxNetworkStackData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "network-stack",
        Title = "网络协议栈与诊断",
        Keywords = "ip · ss · resolvectl",
        Level = ModuleLevel.Advanced,
        Overview = "Linux 网络的诊断工具已经从 ifconfig、netstat 换成了 iproute2（ip、ss）。本模块讲清楚内核怎样处理网络数据、路由如何决定数据走哪块网卡，以及怎样用 ip、ss、resolvectl 查看地址、路由、连接和 DNS。",

        Why =
        [
            "1. 新旧工具：`ifconfig`、`netstat`、`route` 属于已停止维护的 net-tools，很多发行版已不再默认安装。`ip` 和 `ss` 通过 Netlink 直接与内核交换结构化数据，能显示多地址、策略路由、网络命名空间等 net-tools 无法表达的信息。",
            "2. 数据的收发路径：网卡收到数据后写入内存并通知内核，内核逐层处理（以太网 → IP → TCP/UDP），最后放进对应套接字的接收队列，由程序读取。发送时方向相反，经路由表选择出口网卡。",
            "3. 路由：内核按「最长前缀匹配」选路由；`default` 路由匹配所有其他目的地，指向路由器（网关）。多块网卡同时在线时，`metric` 较小的默认路由优先。",
            "4. 网络命名空间：每个命名空间有独立的网卡、地址、路由和防火墙规则。容器（Podman、Distrobox 的部分模式）借此实现网络隔离。",
            "5. DNS：SteamOS 使用 systemd-resolved。程序查询域名时经由它转发给各网卡配置的 DNS 服务器，并缓存结果；`/etc/resolv.conf` 指向本机的 `127.0.0.53`。",
        ],

        Consequences =
        [
            "• `ip addr`、`ip route`、`ss`、`resolvectl status` 都是只读查询。",
            "• 用 `ip addr add`、`ip route add` 做的修改立即生效，但 NetworkManager 重连后会被覆盖；需要持久的设置应通过 `nmcli` 修改连接配置。",
            "• `ss -p` 只能显示你自己进程的进程名，要看所有进程需要 sudo。",
        ],

        Steps =
        [
            Step.Read(
                "查看网卡状态与 IP 地址",
                "ip -brief addr show",
                "`-brief` 精简输出。").Then("每个网卡一行：名称、状态（`UP`/`DOWN`/`UNKNOWN`）、IPv4 与 IPv6 地址。容器运行时还会出现 `podman0`、`veth...` 等虚拟网卡。"),
            Step.Read(
                "查看路由表与默认网关",
                "ip route show",
                "多网卡同时在线时，`metric` 小的默认路由优先。").Then("第一行一般是 `default via 192.168.0.1 dev wlan0 proto dhcp ... metric 600`：所有不在本地网段的流量都交给 192.168.0.1（路由器）；其余行是本地网段路由。"),
            Step.Read(
                "查看正在监听的端口及所属进程",
                "ss -tulnp",
                "`-t` TCP、`-u` UDP、`-l` 只看监听、`-n` 不解析名称、`-p` 显示进程。").Then("表格：协议、状态、本地地址:端口、对端地址、进程。例如 `0.0.0.0:22` 是 sshd。普通用户只能看到自己进程的名称，其他进程的列为空。"),
            Step.Read(
                "查看已建立 TCP 连接的延迟与拥塞指标",
                "ss -tie state established | head -n 25",
                "`rtt` 是往返时延（毫秒），`cwnd` 是拥塞窗口。").Then("每个已建立的连接两行：第一行地址和端口，第二行 `cubic rto:... rtt:23.5/4.2 ... cwnd:10 ...` 等内部指标。"),
            Step.Read(
                "查看每块网卡使用的 DNS 服务器",
                "resolvectl status",
                "systemd-resolved 为每个网卡分别记录 DNS 服务器。").Then("先是 `Global` 全局设置，然后按网卡分段，如 `Link 3 (wlan0)` 下的 `Current DNS Server` 与 `DNS Servers`。"),
            Step.Read(
                "查看网卡的收发统计与错误计数",
                "ip -s link show wlan0",
                "`-s` 显示统计数据。").Then("除网卡基本信息外，多出 `RX:` 与 `TX:` 两组统计：字节数、包数、错误、丢弃、溢出等。`errors`、`dropped` 持续增长说明信号或驱动有问题。"),
        ],

        Verify =
        [
            "1. `ip route get 223.5.5.5` 显示去往该地址会经过哪块网卡、哪个网关，以及使用的源地址。",
            "2. `ss -s` 显示各类套接字的数量汇总。",
            "3. `resolvectl query store.steampowered.com` 能返回 IP 地址。",
        ],
    };
}
