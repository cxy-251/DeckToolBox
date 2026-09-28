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
        Overview = "全面剖析 Linux 内核网络子系统 (sk_buff / Netfilter / TCP状态机)，淘汰过时的 ifconfig/netstat，掌握 iproute2 现代运维体系、路由决策、网络命名空间与 systemd-resolved DNS 链。",

        Why =
        [
            "1. 淘汰 net-tools (ifconfig / netstat)，拥抱 iproute2 (ip / ss)：",
            "• 过去运维常用的 ifconfig 和 netstat 依赖读取 `/proc/net/dev` 与过时的 ioctl 系统调用，在网络连接数巨大或存在虚拟网卡时存在严重的解析性能瓶颈，且完全不支持 Policy Routing、VLAN、网络命名空间等现代 Linux 特性。",
            "• 现代工具通过 Netlink 套接字 (AF_NETLINK) 直接与内核交换结构化数据，支持策略路由、网络命名空间、VLAN 等 net-tools 无法表达的特性；`ss` 还能显示 TCP 的往返时间 (rtt)、拥塞窗口 (cwnd) 等细节。",
            "2. Linux 内核网络数据流与核心数据结构 (sk_buff)：",
            "• 网卡 (NIC) 收到数据帧后通过 DMA 写入环形缓冲区 (Ring Buffer)，触发硬中断转软中断 (NAPI / NET_RX)，数据被封装为内核核心结构体 `sk_buff` (Socket Buffer)。",
            "• `sk_buff` 沿内核协议栈向上逐层解包（链路层 MAC -> 网络层 IP 路由查找 -> 传输层 TCP/UDP 校验），最终唤醒等待在 socket 读取队列上的用户进程（通过 epoll / read 系统调用）。",
            "3. 网络命名空间 (Network Namespaces - CLONE_NEWNET)：",
            "• Linux 容器技术（Docker、Podman、Bubblewrap、Pressure-Vessel）之所以能实现网络隔离，依靠的就是内核网络命名空间。每个 namespace 拥有独立的网卡设备表、IP 地址、ARP 缓存表、iptables/nftables 规则链与套接字分配表。",
            "4. 现代 DNS 解析管道 (systemd-resolved 与 NSSwitch)：",
            "• 现代 Linux 桌面普遍使用 `systemd-resolved` 本地守护进程管理 DNS 解析。应用程序发起 `getaddrinfo` 时，通过 `/etc/nsswitch.conf` 路由至 `nss-resolve` 模块，本地监听于 127.0.0.53:53 内存缓存中，支持 DNSSEC、DoT (DNS over TLS) 与 Split-DNS 路由。",
        ],

        Consequences =
        [
            "• 只读诊断绝对安全：使用 `ip addr`、`ip route`、`ss -tulnp`、`resolvectl status` 进行查询为纯只读操作，不会断开任何连接，无任何风险。",
            "• 路由表或 IP 临时修改即时生效：通过 `ip addr add` 或 `ip route add` 变更网络配置，该修改立即作用于内核，若配置错误可能导致网络即时中断；但所有未持久化到 NetworkManager 连接文件的修改在重启或重新联网后均会自动复原。",
            "• 端口占用排查注意事项：使用 `ss -tulpn` 查看进程绑定需要对应权限，非 root 用户只能看到属于自己 UID 的进程 PID，查看系统级守护进程需要加上 `sudo`。",
        ],

        Steps =
        [
            Step.Read(
                "查看全量网络接口状态、MAC 地址与 IP 分配详情",
                "ip -brief addr show",
                "`-brief` 精简输出。").Then("每个网卡一行：名称、状态（`UP`/`DOWN`/`UNKNOWN`）、IPv4 与 IPv6 地址。容器运行时还会出现 `podman0`、`veth...` 等虚拟网卡。"),
            Step.Read(
                "查看内核当前路由决策表与默认网关出口",
                "ip route show",
                "多网卡同时在线时，`metric` 小的默认路由优先。").Then("第一行一般是 `default via 192.168.0.1 dev wlan0 proto dhcp ... metric 600`：所有不在本地网段的流量都交给 192.168.0.1（路由器）；其余行是本地网段路由。"),
            Step.Read(
                "排查系统当前所有正在监听的 TCP 与 UDP 服务端口及所属进程",
                "ss -tulnp",
                "`-t` TCP、`-u` UDP、`-l` 只看监听、`-n` 不解析名称、`-p` 显示进程。").Then("表格：协议、状态、本地地址:端口、对端地址、进程。例如 `0.0.0.0:22` 是 sshd。普通用户只能看到自己进程的名称，其他进程的列为空。"),
            Step.Read(
                "观测活跃 TCP 连接的微观拥塞控制指标与延时 (RTT)",
                "ss -tie state established | head -n 25",
                "`rtt` 是往返时延（毫秒），`cwnd` 是拥塞窗口。").Then("每个已建立的连接两行：第一行地址和端口，第二行 `cubic rto:... rtt:23.5/4.2 ... cwnd:10 ...` 等内部指标。"),
            Step.Read(
                "排查本地 DNS 解析中继链与各网络接口绑定的 DNS 上游",
                "resolvectl status",
                "systemd-resolved 为每个网卡分别记录 DNS 服务器。").Then("先是 `Global` 全局设置，然后按网卡分段，如 `Link 3 (wlan0)` 下的 `Current DNS Server` 与 `DNS Servers`。"),
            Step.Read(
                "审计网络协议栈全局统计数据与丢包/错误计数",
                "ip -s link show wlan0",
                "`-s` 显示统计数据。").Then("除网卡基本信息外，多出 `RX:` 与 `TX:` 两组统计：字节数、包数、错误、丢弃、溢出等。`errors`、`dropped` 持续增长说明信号或驱动有问题。"),
        ],

        Verify =
        [
            "1. 网关连通性验证：执行 `ip route get 1.1.1.1`，内核会实时模拟路由计算，输出数据包将经由哪块接口 (dev) 与网关 (via) 发出，以及匹配的源 IP (src)。",
            "2. TIME_WAIT 状态评估：执行 `ss -s` 查看全量 Socket 汇总，若 TIME_WAIT 套接字在数千以内属于高并发网络通信的正常 TCP 四次挥手状态；若异常膨胀可结合内核 tcp_tw_reuse 调优参数排查。",
            "3. DNS 本地解析测试：执行 `resolvectl query store.steampowered.com`，应毫秒级返回对应 A/AAAA 记录，并标明是由哪个 DNS 接口与缓存完成解析。",
        ],
    };
}
