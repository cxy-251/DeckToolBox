using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class NoInternetSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-no-internet",
        Title = "连着 Wi-Fi 却上不了网",
        Keywords = "上不了网 · 网页打不开 · Steam 离线 · DNS · 网关 · ping",
        Level = ModuleLevel.Basic,
        Overview = "Wi-Fi 图标显示已连接，但网页打不开、Steam 显示离线或下载不动。从近到远逐段检查：Deck 有没有拿到地址、能不能到路由器、能不能到外网、域名能不能解析，哪一段不通，问题就在哪里。",

        Why =
        [
            "一次上网要经过四段，任何一段断了都会表现为「上不了网」：",
            "1. **本机地址**：路由器通过 DHCP 给 Deck 分配 IP 地址。拿不到地址时，地址会是 `169.254.x.x` 或者干脆没有。",
            "2. **到路由器（网关）**：信号太弱或路由器卡死时这一段不通。",
            "3. **到外网**：路由器本身没联网（宽带断了、欠费、光猫故障）。",
            "4. **域名解析 (DNS)**：IP 能通但域名不行，就是 DNS 服务器的问题，常见于路由器 DNS 故障。",
        ],

        Consequences =
        [
            "• 第 1~4 步都是只读的。",
            "• 第 5 步会断开 Wi-Fi 几秒再重连，正在进行的下载会暂停。",
        ],

        Steps =
        [
            Step.Read(
                "有没有拿到 IP 地址",
                "ip -br addr show wlan0; ip route | grep default",
                "第一行是网卡状态与地址，第二行是默认网关（路由器地址）。").Then("正常时第一行为 `wlan0  UP  192.168.x.x/24`，第二行为 `default via 192.168.x.1 dev wlan0 ...`。**没有 IPv4 地址或是 `169.254.` 开头，或者没有 `default` 行** → 没从路由器拿到地址，直接做第 5 步重连，无效就重启路由器。"),
            Step.Read(
                "能不能到路由器",
                "ping -c 3 $(ip route | awk '/default/{print $3; exit}')",
                "自动取出网关地址并发 3 个包。").Then("正常时 3 行 `64 字节，来自 192.168.x.1 ... 时间=3 毫秒`，最后 `0% packet loss`。**全部丢包** → Deck 与路由器之间不通：靠近路由器、改连 5 GHz 或重启路由器。"),
            Step.Read(
                "能不能到外网",
                "ping -c 3 223.5.5.5",
                "直接用 IP 地址测试，排除 DNS 的影响。").Then("**通** → 外网正常，继续第 4 步。**不通但第 2 步通** → 问题在路由器之外（宽带、光猫），用手机连同一个 Wi-Fi 也上不了网即可确认。"),
            Step.Read(
                "域名能不能解析",
                "resolvectl query store.steampowered.com",
                "IP 能通而这一步失败，就是 DNS 的问题。").Then("**成功**时列出几个 IP 地址，网络应该正常，问题可能出在具体网站或代理设置。**失败**（如 `resolve call failed`）→ 在「系统设置 → 连接」中为这个 Wi-Fi 手动填写 DNS（如 `223.5.5.5`），或参考「nmcli：网络连接进阶」。"),
            Step.Change(
                "断开并重新连接 Wi-Fi",
                "nmcli dev disconnect wlan0 && nmcli dev connect wlan0",
                "很多临时问题重连后即可恢复。").Then("先提示设备已断开，几秒后提示成功激活；之后从第 1 步重新检查。"),
        ],

        Verify =
        [
            "• 第 2~4 步全部成功，`curl -sS -o /dev/null -w '%{http_code}\\n' https://store.steampowered.com` 输出 `200`。",
        ],

        Related = ["network-basics", "man-nmcli", "network-stack", "sym-lag-spikes"],
    };
}
