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
            "4. 无线网卡省电 (Power Management)：开启时网卡在空闲间隙短暂休眠，省电，但可能让联机游戏的延迟周期性跳高。NetworkManager 在 SteamOS 上的默认值是开启（`wifi.powersave=3`），Steam 的开发者选项和下面的 SteamOS 辅助程序可以把它关掉；重启或重连后可能恢复为开启。",
        ],

        Consequences =
        [
            "• 查询类命令只读；`ping` 按 `Ctrl+C` 停止。",
            "• `nmcli` 创建或修改的连接会永久保存（在 `/etc/NetworkManager/system-connections/`），和在系统设置里操作效果相同。",
            "• 断开并重连 Wi-Fi 会中断当前所有网络连接（下载、SSH 会话等）。",
            "• 如果 shell 中设置了 `HTTP_PROXY` / `HTTPS_PROXY` 环境变量，`curl` 等命令行工具会经过代理访问网络，测试结果反映的是代理的状况。用 `env | grep -i proxy` 查看，临时绕过可加 `--noproxy '*'`。",
        ],

        Steps =
        [
            Step.Read(
                "查看网卡状态和 IP 地址",
                "ip -br addr",
                "`-br` 简洁输出。`lo` 是本机回环。").Then("每个网卡一行，如 `wlan0  UP  192.168.0.21/24 fe80::.../64`。无线网卡显示 `DOWN` 或没有 IPv4 地址，说明没连上网络。"),
            Step.Read(
                "查看当前生效的连接",
                "nmcli -f NAME,TYPE,DEVICE connection show --active",
                "同时列出已保存的所有连接：`nmcli connection show`。").Then("表格：`NAME TYPE DEVICE`，如 `MyWiFi  802-11-wireless  wlan0`。"),
            Step.Read(
                "扫描附近的 Wi-Fi 及信号强度",
                "nmcli dev wifi list",
                "按 `q` 退出分页（如果出现）。").Then("表格列出附近的网络：BSSID、SSID（名称）、信道、速率、`SIGNAL` 信号强度（0~100）、`BARS` 信号格、加密方式。当前连接的网络前有 `*`。"),
            Step.Read(
                "查看当前无线连接的信号强度与速率",
                "iw dev wlan0 link",
                "`signal` 越接近 0 越好：-50 以上很好，-60 左右正常，-70 以下容易卡顿断线。`freq` 2412~2484 为 2.4 GHz，5000 以上为 5 GHz。").Then("输出 `SSID: MyWiFi`、`freq: 2462.0`、`signal: -59 dBm`、`rx bitrate: 81.0 MBit/s` 等行；未连接时显示 `Not connected.`。"),
            Step.Change(
                "连接一个 Wi-Fi",
                "nmcli --ask dev wifi connect '<网络名称>'",
                "`<网络名称>` 填入上一步 `nmcli dev wifi list` 中的 SSID 名。`--ask` 交互式询问密码，密码不留在命令历史。").Then("提示输入密码（输入时不显示），成功后输出 `成功用 'xxx' 激活了设备 'wlan0'`。连接配置会保存下来，以后自动连接。"),
            Step.Read(
                "测试到网关和外网的连通性",
                "ping -c 3 $(ip route | awk '/default/{print $3; exit}'); ping -c 3 223.5.5.5",
                "`-c 3` 只发 3 个包。第一条测网关，第二条测外网（阿里公共 DNS 的 IP）。").Then("两段 ping 输出，各 3 行 `64 字节，来自 ... 时间=3.2 毫秒`，最后是统计：`0% packet loss` 表示通畅。若网关通而外网不通，问题在路由器或运营商。"),
            Step.Read(
                "测试 DNS 解析",
                "resolvectl query store.steampowered.com",
                "IP 能 ping 通而这里失败，就是 DNS 的问题。").Then("输出若干 IP 地址及 `-- Information acquired via protocol DNS in xx.xms.`；失败时显示解析错误。"),
            Step.Read(
                "测试网页访问并显示耗时",
                "curl -sS -o /dev/null -w 'HTTP %{http_code}  总耗时 %{time_total}s\\n' https://store.steampowered.com",
                "`-o /dev/null` 丢弃网页内容，`-w` 只显示状态码与耗时。").Then("输出一行，如 `HTTP 200  总耗时 0.85s`。`000` 表示连接失败。"),
            Step.Change(
                "重新连接无线网络",
                "nmcli dev disconnect wlan0 && nmcli dev connect wlan0",
                "很多「连着 Wi-Fi 却上不了网」的问题重连后即可恢复。").Then("先提示设备已断开，再提示成功激活；网络中断几秒后恢复。"),
            Step.Read(
                "查看无线网卡节能管理状态",
                "iwconfig wlan0 | grep -E 'Bit Rate|Power Management|Link Quality'",
                "`Bit Rate` 是当前协商的速率，`Link Quality` 是信号质量（分母 70）。").Then("三行：`Bit Rate=27 Mb/s   Tx-Power=16 dBm`、`Power Management:off`（或 `on`）、`Link Quality=50/70  Signal level=-60 dBm`。"),
            Step.Change(
                "关闭无线网卡节能降低延迟",
                "/usr/bin/steamos-polkit-helpers/steamos-disable-wireless-power-management && iwconfig wlan0 | grep 'Power Management'",
                "这个辅助程序不在 PATH 中，需写完整路径。它通过 pkexec 以 root 身份执行 `iwconfig wlan0 power off`；polkit 规则允许当前登录用户免密码执行。").Then("没有密码提示，随后输出 `Power Management:off`。网卡不再在空闲时休眠，耗电略有增加；重新连接 Wi-Fi 或重启后可能恢复默认。"),
        ],

        Verify =
        [
            "1. `ip -br addr` 中 `wlan0` 为 `UP` 并有 IPv4 地址。",
            "2. 两个 `ping` 都显示 `0% packet loss`，`resolvectl query` 能返回地址，`curl` 显示 `HTTP 200`。",
        ],
    };
}
