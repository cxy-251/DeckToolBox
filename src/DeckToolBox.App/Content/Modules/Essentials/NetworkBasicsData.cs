using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class NetworkBasicsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "network-basics",
        Title = "Wi-Fi 与网络排查",
        Keywords = "nmcli · ping · ip · resolvectl · curl",
        Level = ModuleLevel.Basic,
        Overview = "SteamOS 和大多数桌面发行版都用 NetworkManager 管理网络，右下角的网络图标和 nmcli 命令操作的是同一套配置。断网时按「本机 → 网关 → 外网 IP → 域名」四步排查，就能定位问题出在哪一层。",

        Why =
        [
            "1. NetworkManager：保存每个 Wi-Fi / 有线连接的配置（称为「连接」），负责自动连接、获取 IP、设置 DNS。`nmcli` 是它的命令行界面，`nmtui` 是终端里的菜单界面。",
            "2. 断网时的四步排查：",
            "- 本机有没有 IP：`ip -br addr`，无线网卡 `wlan0` 应为 `UP` 并有 `192.168.x.x` 之类的地址。",
            "- 能否到达路由器（网关）：`ip route` 找到 `default via` 后的地址，`ping` 它。",
            "- 能否到达外网：`ping 223.5.5.5` 或 `ping 1.1.1.1`，用 IP 地址测试，排除 DNS 影响。",
            "- 域名能否解析：`resolvectl query <域名>`。IP 能通而域名不行，就是 DNS 的问题。",
            "3. 信号与频段：`nmcli dev wifi` 的 `SIGNAL` 为信号强度（0~100），`CHAN` 为信道。1~13 为 2.4 GHz（穿墙好、速度低），36 以上为 5 GHz（速度快、穿墙差）。",
        ],

        Consequences =
        [
            "• 查询类命令只读；`ping` 按 `Ctrl+C` 停止。",
            "• `nmcli` 创建或修改的连接会永久保存（在 `/etc/NetworkManager/system-connections/`），和在系统设置里操作效果相同。",
            "• 断开并重连 Wi-Fi 会中断当前所有网络连接（下载、SSH 会话等）。",
        ],

        Steps =
        [
            Step.Read(
                "查看网卡状态和 IP 地址",
                "ip -br addr",
                "`-br` 简洁输出。`lo` 是本机回环，`wlan0` 是无线网卡。"),
            Step.Read(
                "查看当前生效的连接",
                "nmcli -f NAME,TYPE,DEVICE connection show --active",
                "同时列出已保存的所有连接：`nmcli connection show`。"),
            Step.Read(
                "扫描附近的 Wi-Fi 及信号强度",
                "nmcli dev wifi list",
                "当前连接的网络前面有 `*`。"),
            Step.Change(
                "连接一个 Wi-Fi",
                "nmcli --ask dev wifi connect '<网络名称>'",
                "`--ask` 让 nmcli 交互式询问密码，密码不会留在命令历史里。"),
            Step.Read(
                "测试到网关和外网的连通性",
                "ping -c 3 $(ip route | awk '/default/{print $3; exit}'); ping -c 3 223.5.5.5",
                "`-c 3` 只发 3 个包。第一条测网关，第二条测外网（阿里公共 DNS 的 IP）。"),
            Step.Read(
                "测试 DNS 解析",
                "resolvectl query store.steampowered.com",
                "能返回 IP 地址说明 DNS 正常。"),
            Step.Read(
                "测试网页访问并显示耗时",
                "curl -sS -o /dev/null -w 'HTTP %{http_code}  总耗时 %{time_total}s\\n' https://store.steampowered.com",
                "`HTTP 200` 表示访问成功。"),
            Step.Change(
                "重新连接无线网络",
                "nmcli dev disconnect wlan0 && nmcli dev connect wlan0",
                "很多「连着 Wi-Fi 却上不了网」的问题重连后即可恢复。"),
        ],

        Verify =
        [
            "1. `ip -br addr` 中 `wlan0` 为 `UP` 并有 IPv4 地址。",
            "2. 两个 `ping` 都显示 `0% packet loss`，`resolvectl query` 能返回地址，`curl` 显示 `HTTP 200`。",
        ],
    };
}
