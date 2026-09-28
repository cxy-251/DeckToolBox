using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Systemd;

public static class SystemctlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-systemctl",
        Title = "systemctl：管理服务",
        Keywords = "status · start/stop/restart · enable/disable · cat · edit · list-units",
        Level = ModuleLevel.Basic,
        Overview = "systemctl 是 systemd 的主要命令：查看服务状态、启动停止、设为开机自启、查看和覆盖服务的配置。系统服务的写操作需要 sudo；加 --user 管理你自己的用户服务，则不需要。",

        Why =
        [
            "1. 查看：",
            "- `systemctl status 服务`：是否运行、主进程 PID、内存占用、最近几行日志。",
            "- `is-active` / `is-enabled` / `is-failed`：只输出一个词，适合脚本判断。",
            "- `list-units --type=service`：已加载的服务；`--state=running`、`--state=failed` 筛选。",
            "- `list-unit-files --state=enabled`：所有设为自启的单元。",
            "- `cat 服务`：显示单元文件的完整内容（包括覆盖配置）。",
            "- `show 服务 -p 属性`：读取单个属性，如 `MainPID`、`MemoryCurrent`、`ActiveEnterTimestamp`。",
            "2. 控制：",
            "- `start` / `stop` / `restart`：立即启动、停止、重启，只影响本次运行。",
            "- `reload`：让服务重新读取配置而不中断（服务需支持）。",
            "- `enable` / `disable`：设置或取消开机（用户服务为登录时）自动启动；加 `--now` 同时立即启动或停止。",
            "- `mask`：彻底禁止启动（连依赖它的服务也无法拉起），`unmask` 解除。",
            "3. 覆盖配置：`systemctl edit 服务` 打开编辑器创建一个「drop-in」覆盖文件（位于 `/etc/systemd/system/服务.d/override.conf`），只写需要修改的项，原文件不动，系统更新后你的修改依然有效。",
            "4. 单元名可以省略 `.service` 后缀：`systemctl status sshd` 等同于 `sshd.service`。",
        ],

        Consequences =
        [
            "• 查看类命令只读；停止系统服务可能让对应功能立刻失效（如停掉 NetworkManager 会断网）。",
            "• 不要随意 disable 或 mask 系统自带的服务，SteamOS 的功能（风扇控制、更新、手柄）都依赖它们。",
            "• 修改单元文件后需要 `daemon-reload`；`systemctl edit` 保存后会自动执行。",
        ],

        Steps =
        [
            Step.Read(
                "查看一个服务的状态",
                "systemctl status sshd --no-pager",
                "`--no-pager` 直接输出，不进入分页界面。").Then("第一行 `● sshd.service - OpenSSH Daemon`（圆点为绿色表示运行中），下面是 `Loaded:`（文件路径与是否自启）、`Active: active (running) since ...`、`Main PID`、`Memory`，最后是最近几行日志。"),
            Step.Read(
                "脚本式判断",
                "systemctl is-active sshd; systemctl is-enabled sshd bluetooth",
                "每个单元输出一个词。").Then("输出 `active`（或 `inactive`），然后两行 `enabled`/`disabled`。"),
            Step.Read(
                "列出正在运行的服务",
                "systemctl list-units --type=service --state=running --no-pager | head -n 15",
                "加 `--user` 则列出用户服务。").Then("表格列出 `UNIT LOAD ACTIVE SUB DESCRIPTION`，如 `jupiter-fan-control.service loaded active running Jupiter fan control`。"),
            Step.Read(
                "列出所有开机自启的单元",
                "systemctl list-unit-files --state=enabled --no-pager",
                "`PRESET` 列是发行版的默认设置。").Then("列出 `avahi-daemon.service enabled`、`bluetooth.service enabled`、`firewalld.service enabled` 等。"),
            Step.Read(
                "查看服务的单元文件",
                "systemctl cat sshd --no-pager",
                "第一行注释是文件路径。").Then("显示 `# /usr/lib/systemd/system/sshd.service` 以及 `[Unit]`、`[Service]`、`[Install]` 三节配置。"),
            Step.Read(
                "读取单个属性",
                "systemctl show sshd -p MainPID -p ActiveEnterTimestamp -p MemoryCurrent",
                "`MemoryCurrent` 单位是字节。").Then("三行 `名称=值`，如 `MainPID=1312`、`ActiveEnterTimestamp=Wed 2026-09-23 16:27:43 CST`。"),
            Step.Root(
                "重启一个系统服务",
                "sudo systemctl restart bluetooth",
                "蓝牙异常时可用；所有蓝牙设备会短暂断开。").Then("输入 sudo 密码后没有输出，服务重启；`systemctl status bluetooth` 中 `Active` 的时间变为刚才。"),
            Step.Root(
                "用 drop-in 覆盖服务配置（示例）",
                "sudo systemctl edit <服务名>",
                "在打开的编辑器中两行注释之间写入要修改的项，如 `[Service]` 与 `Restart=always`，保存退出。").Then("在 `/etc/systemd/system/<服务名>.service.d/override.conf` 生成覆盖文件并自动重新加载；`systemctl cat <服务名>` 末尾会多出这个文件的内容。"),
        ],

        Verify =
        [
            "1. `systemctl --failed` 显示 `0 loaded units listed.`，说明没有失败的服务。",
        ],
    };
}
