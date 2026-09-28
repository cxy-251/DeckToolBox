using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Network;

public static class VpnQrData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-vpn-qr",
        Title = "VPN 导入与二维码",
        Keywords = "WireGuard · OpenVPN · nmcli import · wg · qrencode",
        Level = ModuleLevel.Intermediate,
        Overview = "公司或学校提供的 WireGuard、OpenVPN 配置文件，可以用 nmcli 一条命令导入 NetworkManager，之后在任务栏的网络菜单中一键连接。qrencode 能把任意文字（网址、Wi-Fi 信息、配置）生成二维码，方便用手机扫描。",

        Why =
        [
            "1. 导入 VPN：`nmcli connection import type wireguard file 文件.conf` 或 `type openvpn file 文件.ovpn`。导入后它就是一个普通连接：",
            "- `nmcli connection up 名称` 连接，`down` 断开。",
            "- 系统托盘的网络菜单和系统设置中都能看到并开关它。",
            "- 连接名默认取自配置文件名（WireGuard 的接口名）。",
            "2. WireGuard 工具：`wg show` 查看当前隧道的状态、对端、最近握手时间和传输量（需要 root）；`wg genkey`、`wg pubkey` 生成密钥对。",
            "3. qrencode：",
            "- `qrencode -t ANSIUTF8 '文字'`：直接在终端画出二维码。",
            "- `qrencode -o 文件.png '文字'`：生成 PNG 图片，`-s 10` 放大像素点。",
            "- Wi-Fi 二维码的标准格式：`WIFI:T:WPA;S:网络名;P:密码;;`，手机相机扫描即可加入。",
        ],

        Consequences =
        [
            "• 导入 VPN 配置不会立即连接；连接后，按配置不同，全部或部分流量经 VPN 发出。",
            "• VPN 配置文件包含私钥，不要分享或上传；导入后可以删除原文件（NetworkManager 已保存一份）。",
            "• 二维码中的内容任何人都能扫描读取，不要把密码类信息的二维码截图外传。",
        ],

        Steps =
        [
            Step.Change(
                "导入 WireGuard 配置",
                "nmcli connection import type wireguard file ~/Downloads/<名称>.conf",
                "OpenVPN 配置改为 `type openvpn file <名称>.ovpn`。").Then("提示 `连接 '<名称>' (UUID) 已成功添加`，网络菜单中出现该 VPN。"),
            Step.Change(
                "连接与断开 VPN",
                "nmcli connection up <名称> && ip -br addr show <名称>",
                "断开用 `nmcli connection down <名称>`。").Then("提示连接已成功激活；`ip` 显示一个以 VPN 名称命名的新网络接口及其地址。"),
            Step.Root(
                "查看 WireGuard 隧道状态",
                "sudo wg show",
                "需要 root。").Then("显示接口、公钥、监听端口，以及每个对端的地址、`latest handshake`（最近握手时间）和 `transfer`（收发量）。握手时间在几分钟内说明隧道正常。"),
            Step.Read(
                "在终端显示一个网址的二维码",
                "qrencode -t ANSIUTF8 'https://store.steampowered.com'",
                "`ANSIUTF8` 用字符方块绘制。").Then("终端中出现一个黑白方块组成的二维码，用手机相机扫描即可打开该网址。"),
            Step.Change(
                "生成一个 Wi-Fi 二维码图片",
                "qrencode -s 10 -o ~/Pictures/wifi-qr.png 'WIFI:T:WPA;S:<网络名>;P:<密码>;;' && xdg-open ~/Pictures/wifi-qr.png",
                "`-s 10` 让每个点 10 像素，打印或在屏幕上显示都清晰。").Then("生成 PNG 图片并用图片查看器打开；访客用手机扫描即可直接连上这个 Wi-Fi。"),
        ],

        Verify =
        [
            "1. 连接 VPN 后执行 `ip route`，能看到经由 VPN 接口的路由。",
        ],
    };
}
