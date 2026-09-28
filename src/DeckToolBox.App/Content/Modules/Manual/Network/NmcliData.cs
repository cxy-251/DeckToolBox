using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Network;

public static class NmcliData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-nmcli",
        Title = "nmcli：网络连接进阶",
        Keywords = "connection · 静态 IP · DNS · 自动连接 · 显示密码 · 热点",
        Level = ModuleLevel.Intermediate,
        Overview = "nmcli 能完成系统设置中网络页面的所有操作，而且可以写进脚本：查看保存的连接、改为静态 IP、指定 DNS、调整自动连接优先级、显示当前 Wi-Fi 密码（附带二维码），以及开启热点。连接的基本排查见入门必修的「Wi-Fi 与网络排查」。",

        Why =
        [
            "1. 设备与连接：",
            "- 设备 (device)：网卡本身，如 `wlan0`。",
            "- 连接 (connection)：一份保存下来的配置（Wi-Fi 名称、密码、IP 设置等），一个设备可以有多份连接，例如家里和公司各一份。",
            "- 连接配置保存在 `/etc/NetworkManager/system-connections/`，系统更新后保留。",
            "2. 查看与修改连接的属性：`nmcli connection show 名称` 列出全部属性；`nmcli connection modify 名称 属性 值` 修改；改完后 `nmcli connection up 名称` 重新激活才生效。常用属性：",
            "- `ipv4.method`：`auto`（DHCP 自动获取）或 `manual`（手动/静态）。",
            "- `ipv4.addresses`、`ipv4.gateway`、`ipv4.dns`：静态地址、网关、DNS。",
            "- `ipv4.ignore-auto-dns yes`：DHCP 模式下忽略路由器下发的 DNS，只用你指定的。",
            "- `connection.autoconnect-priority`：多个网络都可用时，数值大的优先连接。",
            "3. SteamOS 默认的 Wi-Fi 后端是 iwd（`steamosctl get-wifi-backend` 可查看），NetworkManager 在其上工作，nmcli 用法不变。",
            "4. `nmcli device wifi show-password`：显示当前 Wi-Fi 的名称、加密方式、密码，并在终端画出一个二维码，手机扫描即可连接同一网络。",
        ],

        Consequences =
        [
            "• 修改连接属性并重新激活时，网络会中断几秒。",
            "• 静态 IP 填错（网段、网关不对）会导致无法上网；改回 `ipv4.method auto` 并重新连接即可恢复。",
            "• `show-password` 会在屏幕上以明文显示密码，注意周围环境和截图。",
            "• 开启热点会让无线网卡从当前 Wi-Fi 断开，改为自己发射信号。",
        ],

        Steps =
        [
            Step.Read(
                "列出保存的连接",
                "nmcli -f NAME,TYPE,DEVICE,AUTOCONNECT,AUTOCONNECT-PRIORITY connection show",
                "`-f` 选择显示的列。").Then("每个连接一行，正在使用的连接 `DEVICE` 列为 `wlan0`，其他为 `--`；`AUTOCONNECT` 为「是」表示在范围内时会自动连接。"),
            Step.Read(
                "查看当前连接的 IP 设置",
                "nmcli -g ipv4.method,ipv4.addresses,ipv4.gateway,ipv4.dns connection show \"$(nmcli -g NAME connection show --active | head -n 1)\"",
                "`-g` 只输出值，适合脚本。").Then("四行值：方式（`auto` 或 `manual`）、地址、网关、DNS；自动获取时后三项可能为空。"),
            Step.Read(
                "显示当前 Wi-Fi 的密码和二维码",
                "nmcli device wifi show-password",
                "密码会以明文显示。").Then("输出 `SSID:`、`安全性:`、`密码:` 三行，下面是一个由字符方块组成的二维码，手机相机扫描后可以直接加入这个 Wi-Fi。"),
            Step.Change(
                "改为静态 IP（示例）",
                "nmcli connection modify \"<连接名>\" ipv4.method manual ipv4.addresses <IP>/24 ipv4.gateway <网关IP> ipv4.dns \"223.5.5.5 1.1.1.1\" && nmcli connection up \"<连接名>\"",
                "IP 需与路由器在同一网段，且不与其他设备冲突。").Then("网络断开几秒后重新连上，提示 `连接已成功激活`；`ip -br addr` 显示你设定的地址，以后每次连接都使用它。"),
            Step.Change(
                "改回自动获取",
                "nmcli connection modify \"<连接名>\" ipv4.method auto ipv4.addresses '' ipv4.gateway '' ipv4.dns '' && nmcli connection up \"<连接名>\"",
                "清空手动设置的各项。").Then("重新连接后由路由器分配地址与 DNS。"),
            Step.Change(
                "设置多个网络的连接优先级",
                "nmcli connection modify \"<连接名>\" connection.autoconnect-priority 10",
                "默认优先级为 0，数值越大越优先。").Then("没有输出；多个已保存的网络同时可用时，优先连接这一个。"),
            Step.Change(
                "开启 Wi-Fi 热点（示例）",
                "nmcli device wifi hotspot ifname wlan0 ssid DeckAP password '<至少8位密码>'",
                "会断开当前 Wi-Fi。关闭：`nmcli connection down Hotspot`。使用 iwd 后端时若提示不支持，可在「设置 → 网络」中尝试。").Then("提示 `成功用 'Hotspot' 激活了设备 'wlan0'`；其他设备可以搜索到名为 DeckAP 的网络。"),
        ],

        Verify =
        [
            "1. `nmcli general status` 显示 `STATE 已连接`、`CONNECTIVITY 完全`。",
        ],
    };
}
