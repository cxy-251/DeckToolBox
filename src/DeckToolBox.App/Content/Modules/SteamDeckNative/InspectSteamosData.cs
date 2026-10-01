using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class InspectSteamosData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "inspect-steamos",
        Title = "自己查 SteamOS 功能的实现",
        Keywords = "pacman -Qo · pacman -Ql · systemctl cat · grep -rl · journalctl -t · busctl · strings · 读源码",
        Level = ModuleLevel.Advanced,
        Overview = "SteamOS 的很多功能（SD 卡自动挂载、风扇控制、电源键、格式化、快捷菜单里的 TDP）其实是系统里看得见的脚本、服务和 D-Bus 接口。网上的说法常常过时或不准确，系统更新后实现也可能改变；学会从一个命令、一个文件出发，顺藤摸瓜找到它属于哪个软件包、被谁调用、日志写在哪里，就能自己核实，而不必依赖二手资料。本模块的方法在其他发行版上同样适用（把 `pacman` 换成对应的包管理器）。",

        Why =
        [
            "一条线索可以从这几个方向追：",
            "1. **它是什么**：`type` 看命令对应哪个文件，`file` 看是脚本还是编译好的程序。脚本可以直接阅读；程序可以用 `strings` 看其中的文字，往往能看到它调用的命令、URL 或配置路径。",
            "2. **它属于哪个包**：`pacman -Qo <文件>` 找出所属软件包，`pacman -Ql <包名>` 列出同一个包里的其他文件。SteamOS 的硬件相关脚本大多在 `jupiter-hw-support` 包中，系统定制在 `steamos-customizations-jupiter` 中，`steamosctl` 与其后台服务在 `steamos-manager` 中。",
            "3. **谁调用了它**：用 `grep -rl <名字>` 在 `/usr/lib/systemd`（服务）、`/usr/lib/udev/rules.d`（设备事件）、`/usr/share/polkit-1/actions`（权限）、`/usr/lib/hwsupport`、`/usr/lib/steamos` 中搜索，找出引用它的服务、规则或脚本。",
            "4. **它怎样运行**：`systemctl cat <服务>`（用户服务加 `--user`）显示服务文件及所有覆盖片段，从 `ExecStart` 等行能看到实际执行的命令。",
            "5. **它留下了什么日志**：`journalctl -b -F SYSLOG_IDENTIFIER` 列出本次开机所有写过日志的程序名，再用 `journalctl -t <名字>` 单独查看。",
            "6. **它提供了什么接口**：`steamosctl` 与 Steam 的快捷菜单都通过用户总线上的 D-Bus 服务 `com.steampowered.SteamOSManager1`（由 steamos-manager 提供）完成设置；`busctl` 可以列出它的全部接口并读取属性值。",
        ],

        Consequences =
        [
            "• 本模块的命令都只读取文件、软件包数据库与日志，不改变系统。",
            "• 读懂实现后，不要直接修改 `/usr` 下的这些文件：只读系统会阻止修改，解除只读后改了也会在下次更新时被覆盖。需要改变行为时，优先用服务的覆盖片段、`/etc` 下的配置，或提供的命令和接口。",
        ],

        Steps =
        [
            Step.Read(
                "它是脚本还是程序",
                "type steamos-session-select; file $(command -v steamos-session-select) | cut -c1-100",
                "以「切换到桌面模式 / 游戏模式」的命令为例，换成任何想了解的命令。").Then("`steamos-session-select 是 /usr/bin/steamos-session-select` 与 `Bourne-Again shell script, ASCII text executable`：这是 bash 脚本，可以用 `less` 直接阅读。"),
            Step.Read(
                "它属于哪个软件包",
                "pacman -Qo /usr/bin/steamos-session-select /usr/lib/hwsupport/steamos-automount.sh /usr/bin/steamosctl",
                "`-Qo` 查询文件的所属包，只读本机的包数据库，不需要联网。").Then("三行，如 `/usr/lib/hwsupport/steamos-automount.sh 由 jupiter-hw-support 3.8.x-1 所拥有`、`/usr/bin/steamosctl 由 steamos-manager 26.x-1 所拥有`。"),
            Step.Read(
                "同一个包里还有哪些命令和脚本",
                "pacman -Ql jupiter-hw-support | grep -E '/usr/(bin|lib/hwsupport)/.+[^/]$'",
                "`-Ql` 列出包中的全部文件，这里只保留命令与硬件支持脚本。").Then("几十行，如 `jupiter-hw-support /usr/bin/jupiter-biosupdate`、`/usr/bin/jupiter-controller-update`、`/usr/lib/hwsupport/format-device.sh`、`/usr/lib/hwsupport/steamos-powerbuttond`：这就是 Deck 硬件相关功能的清单。"),
            Step.Read(
                "谁在调用这个脚本",
                "grep -rl 'steamos-automount' /usr/lib/systemd /usr/lib/udev/rules.d /usr/share/polkit-1/actions /usr/lib/hwsupport 2>/dev/null",
                "`-r` 递归搜索，`-l` 只输出文件名。").Then("输出 `/usr/lib/hwsupport/block-device-event.sh` 与 `/usr/lib/hwsupport/format-device.sh`：自动挂载脚本由插拔事件脚本和格式化脚本调用；再对 `block-device-event` 搜索一次，就能找到触发它的 udev 规则。"),
            Step.Read(
                "服务实际执行什么",
                "systemctl cat jupiter-fan-control --no-pager | grep -E '^# /|^Exec'",
                "`# /` 开头的行是服务文件及覆盖片段的路径。").Then("`# /usr/lib/systemd/system/jupiter-fan-control.service`，以及 `ExecStart=/usr/share/jupiter-fan-control/fancontrol.py --run`、`ExecStopPost=... --stop`：风扇控制是一个 Python 脚本，可以直接阅读它和同目录下的曲线配置。"),
            Step.Read(
                "编译好的程序里有哪些线索",
                "strings /usr/lib/hwsupport/steamos-powerbuttond | grep -E 'steam://|press'",
                "`strings` 提取二进制文件中的可读文字。").Then("输出 `steam://%s`、`longpowerpress`、`shortpowerpress`：说明电源键守护进程把短按、长按转换成 `steam://shortpowerpress` 这类链接发给 Steam。"),
            Step.Read(
                "哪些 SteamOS 组件写了日志",
                "journalctl -b -F SYSLOG_IDENTIFIER | grep -iE 'steam|jupiter|gamescope|block-device'",
                "`-F` 列出某个字段出现过的所有值，这里是日志的程序标识。").Then("几行标识，如 `block-device-event.sh`、`steam`、`steamos-manager`。之后用 `journalctl -b -t steamos-manager --no-pager | tail` 单独查看某一个。"),
            Step.Read(
                "steamosctl 背后的 D-Bus 接口",
                "busctl --user introspect com.steampowered.SteamOSManager1 /com/steampowered/SteamOSManager1 | grep -E '^com\\.steampowered' | awk '{print $1}'",
                "列出 steamos-manager 提供的全部接口名，每个对应一类设置。").Then("二十多行，如 `com.steampowered.SteamOSManager1.TdpLimit1`、`...BatteryChargeLimit1`、`...FanControl1`、`...WifiPowerManagement1`。用 `busctl --user introspect <服务> <路径> <接口名>` 可以进一步看到其中的属性与方法。"),
            Step.Read(
                "直接读一个属性，与 steamosctl 对照",
                "busctl --user get-property com.steampowered.SteamOSManager1 /com/steampowered/SteamOSManager1 com.steampowered.SteamOSManager1.TdpLimit1 TdpLimit; steamosctl get-tdp-limit",
                "两条命令读的是同一个值。").Then("先输出 `u 15`（`u` 表示无符号整数），再输出 `TDP limit: 15`，说明 `steamosctl` 只是这个 D-Bus 接口的命令行前端。"),
        ],

        Verify =
        [
            "• 任选一个 SteamOS 命令（如 `steamos-add-to-steam`），依次查出它的类型、所属包、调用者与日志标识。",
        ],

        Related = ["polkit-helpers", "steamos-commands", "steamosctl", "steamos-storage-subsystem", "dbus", "man-systemctl", "man-journalctl", "man-hex-strings"],
    };
}
