using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class FirewallData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "firewall",
        Title = "防火墙：firewalld",
        Keywords = "firewall-cmd · 区域 · 服务 · 端口",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 默认启用 firewalld，Fedora、openSUSE 等发行版也用它。弄清楚当前放行了哪些服务和端口，才知道为什么某个局域网联机、串流或 SSH 连得上或连不上。",

        Why =
        [
            "1. 区域 (zone)：一组规则的集合。每个网卡属于一个区域，SteamOS 上 `wlan0` 属于 `public`。其他常见区域：`home`、`trusted`（全部放行）、`drop`（全部丢弃）。",
            "2. 服务与端口：",
            "• 区域里用 `services`（预定义的端口组合，如 `ssh` = 22/tcp）和 `ports`（直接写端口范围）描述放行什么。",
            "• SteamOS 的 `public` 区域默认放行 `ssh`、`dhcpv6-client`，以及 `1024-65535` 的 TCP/UDP 端口，因此 Steam 远程同乐、局域网串流、多数游戏联机不受影响；1024 以下的端口（除 SSH 外）默认拦截。",
            "3. 临时与永久：",
            "- 不加 `--permanent`：立即生效，重启或 `--reload` 后消失，适合测试。",
            "- 加 `--permanent`：写入配置，但**要 `--reload` 后才生效**。常见做法是两条都执行，或先临时测试、确认后用 `--runtime-to-permanent` 保存。",
            "4. 查询状态普通用户即可；修改规则需要 sudo。",
        ],

        Consequences =
        [
            "• 查询命令只读。",
            "• 放行端口会让同一网络中的设备能访问你在该端口上运行的服务，只放行确实需要的。",
            "• 通过 SSH 远程修改防火墙时要格外小心：误删 `ssh` 服务会把自己挡在外面。",
            "• 永久规则保存在 `/etc/firewalld/`，属于 `/etc` 覆盖层，系统更新后保留。",
        ],

        Steps =
        [
            Step.Read(
                "查看防火墙是否运行、网卡属于哪个区域",
                "firewall-cmd --state; firewall-cmd --get-active-zones",
                "`running` 表示已启用。"),
            Step.Read(
                "查看当前区域放行的服务与端口",
                "firewall-cmd --list-all",
                "重点看 `services` 与 `ports` 两行。"),
            Step.Read(
                "查看某个预定义服务包含哪些端口",
                "firewall-cmd --info-service=ssh",
                "`firewall-cmd --get-services` 列出所有预定义服务名。"),
            Step.Root(
                "临时放行一个端口（重启后失效）",
                "sudo firewall-cmd --add-port=<端口>/tcp",
                "例如 `--add-port=80/tcp`。测试没问题后用下一步保存。"),
            Step.Root(
                "把当前的临时规则保存为永久规则",
                "sudo firewall-cmd --runtime-to-permanent",
                "保存后重启依然有效。"),
            Step.Root(
                "移除一个端口规则（临时与永久都删除）",
                "sudo firewall-cmd --remove-port=<端口>/tcp; sudo firewall-cmd --permanent --remove-port=<端口>/tcp",
                "分别删除运行时与永久配置中的规则。"),
        ],

        Verify =
        [
            "1. 放行端口后，`firewall-cmd --list-ports` 中出现该端口。",
            "2. 在另一台设备上用 `curl http://<Deck的IP>:<端口>` 或对应程序测试连通性。",
        ],
    };
}
