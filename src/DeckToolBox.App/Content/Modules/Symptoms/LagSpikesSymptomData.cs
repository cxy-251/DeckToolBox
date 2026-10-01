using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class LagSpikesSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-lag-spikes",
        Title = "联机延迟忽高忽低、Wi-Fi 时断时续",
        Keywords = "延迟 · 跳 ping · 丢包 · 断流 · 省电 · iwd · 信号",
        Level = ModuleLevel.Intermediate,
        Overview = "网能上，但联机游戏时延迟一阵阵跳高，或者 Wi-Fi 隔一会儿就断开重连。通常与信号强度、频段、网卡省电以及 Wi-Fi 后端有关，可以逐项检查。",

        Why =
        [
            "1. **信号弱或干扰大**：2.4 GHz 频段穿墙好但拥挤，容易受邻居路由器、蓝牙、USB 3 设备干扰；5 GHz 速度快、干扰少，但穿墙差。",
            "2. **网卡省电**：省电开启时网卡在空闲间隙短暂休眠，可能造成周期性的延迟跳高。",
            "3. **Wi-Fi 后端兼容性**：SteamOS 默认用 `iwd` 管理 Wi-Fi，个别路由器与它配合不好，表现为频繁断线；可以切换到 `wpa_supplicant`。",
        ],

        Consequences =
        [
            "• 第 1~3 步只读。",
            "• 第 4 步关闭省电会略微增加耗电；切换 Wi-Fi 后端会重启网络服务、断网几秒。",
        ],

        Steps =
        [
            Step.Read(
                "当前信号、频段与速率",
                "nmcli -f IN-USE,SSID,CHAN,SIGNAL,RATE dev wifi | head -n 6",
                "`IN-USE` 列带 `*` 的是当前连接。`CHAN` 1~13 为 2.4 GHz，36 以上为 5 GHz。").Then("几行附近的网络。**当前连接的 `SIGNAL` 低于 50，或者在 2.4 GHz 的拥挤信道上** → 靠近路由器，或在路由器上启用并改连 5 GHz。"),
            Step.Read(
                "网卡省电是否开着",
                "iwconfig wlan0 | grep -E 'Power Management|Link Quality'",
                "也会显示链路质量（分母 70）。").Then("**`Power Management:on`** → 可以做第 4 步把它关掉再观察。`off` 则排除这一项。"),
            Step.Read(
                "用持续 ping 观察延迟是否周期性跳高",
                "ping -c 30 -i 0.5 $(ip route | awk '/default/{print $3; exit}') | tail -n 2",
                "向路由器发 30 个包（共 15 秒），只看统计结果。到路由器这一段本应非常稳定。").Then("最后两行是丢包率与 `rtt min/avg/max/mdev`。**到路由器的 `max` 达到几十上百毫秒、`mdev` 很大或有丢包** → 问题在 Wi-Fi 这一段（信号、省电、后端），而不是外网或游戏服务器。"),
            Step.Change(
                "关闭网卡省电",
                "/usr/bin/steamos-polkit-helpers/steamos-disable-wireless-power-management && iwconfig wlan0 | grep 'Power Management'",
                "SteamOS 自带的辅助程序，不需要密码。重连或重启后可能恢复为开启。").Then("输出 `Power Management:off`。再做一次第 3 步对比。"),
            Step.Read(
                "查看当前 Wi-Fi 后端",
                "steamos-wifi-set-backend --check",
                "频繁断线时可以试着换后端：`steamos-wifi-set-backend wpa_supplicant`，换回用 `iwd`。").Then("输出 `iwd` 或 `wpa_supplicant`。"),
        ],

        Verify =
        [
            "• 第 3 步的 `max` 稳定在十几毫秒以内且没有丢包。",
        ],

        Related = ["network-basics", "steamos-commands", "man-nmcli", "sym-no-internet"],
    };
}
