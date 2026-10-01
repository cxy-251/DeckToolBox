using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class PolkitHelpersData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "polkit-helpers",
        Title = "不用 sudo 的系统操作",
        Keywords = "steamos-polkit-helpers · pkexec · polkit · 时区 · 主机名 · sshd · 回退系统 · priv-write",
        Level = ModuleLevel.Intermediate,
        Overview = "Steam 的设置界面能改时区、开 SSH、切换更新通道、格式化存储卡，而这些都是需要 root 的系统操作，Steam 本身却不以 root 运行。它靠的是 `/usr/bin/steamos-polkit-helpers/` 下的一组辅助程序：polkit 规则允许当前登录用户免密码执行它们。你在终端里也可以直接调用，不需要为 deck 用户设置密码、也不需要 sudo。",

        Why =
        [
            "1. 工作方式：每个辅助程序都是一个很短的脚本。普通用户运行时，它先用 `pkexec` 以 root 身份重新运行自己；polkit 查看 `/usr/share/polkit-1/actions/org.valve.steamos.policy`，其中对这些脚本写的是 `<allow_active>yes</allow_active>`，即「当前处于活动状态的本地会话可以直接执行」，于是不弹密码框。",
            "2. 为什么安全：每个脚本只做一件固定的事（设置时区、启用 sshd、重启进另一个槽位……），能传入的参数也受脚本限制。例如 `steamos-priv-write` 只允许写入屏幕亮度、GPU 性能级别、功耗上限等少数几个 `/sys` 文件，其他路径一律拒绝。",
            "3. 限制：只对「活动的本地会话」有效。通过 SSH 登录时会话不是活动的，调用会要求认证或直接失败；这时只能用 sudo。",
            "4. 常用的几个（都在 `/usr/bin/steamos-polkit-helpers/` 下，不在 PATH 中，需写完整路径）：",
            "- `steamos-set-timezone <时区>`、`steamos-set-hostname <主机名>`：设置时区、主机名。",
            "- `steamos-enable-sshd`：启用并启动 SSH 服务（相当于 `sudo systemctl enable --now sshd`）。",
            "- `steamos-update`、`steamos-select-branch`：检查与安装系统更新、切换更新通道；`steamos-update check` 与 `steamos-select-branch -c` 不需要辅助程序就能直接运行。",
            "- `steamos-reboot-other`：下次从另一个 A/B 槽位（通常是更新前的系统）启动并立即重启，用于更新后出问题时回退。",
            "- `jupiter-fan-control --enable/--disable`：启动或停止风扇控制服务；`steamos-disable-wireless-power-management`：关闭 Wi-Fi 省电。",
            "- `steamos-priv-write <路径> <值>`：向白名单中的 `/sys` 文件写值。",
            "- `steamos-format-device`、`steamos-trim-devices`、`jupiter-dock-updater`、`jupiter-biosupdate`、`steamos-factory-reset-config` 等也在这里，对应 Steam 设置中的同名功能。",
        ],

        Consequences =
        [
            "• 不需要密码不等于没有影响：这些操作与在 Steam 设置中点按钮效果相同，修改的是系统级配置。",
            "• `steamos-enable-sshd` 让局域网内的设备可以尝试登录 deck 账户；deck 用户需要先用 `passwd` 设置密码才能登录，不用时可以 `sudo systemctl disable --now sshd` 关闭。",
            "• `steamos-reboot-other` 会立即重启；另一个槽位的系统可能是旧版本，回退后可以再执行一次回到原来的槽位。",
            "• 格式化、恢复出厂等辅助程序会清除数据，只在完全明白时使用（见「SD 卡的自动挂载与格式化」）。",
        ],

        Steps =
        [
            Step.Read(
                "列出所有辅助程序及其授权",
                "awk -F'[<>]' '/<allow_active>/{a=$3} /exec.path/{n=$3; sub(\".*/\",\"\",n); print a\"\\t\"n}' /usr/share/polkit-1/actions/org.valve.steamos.policy | sort -u | column -t",
                "从 polkit 策略文件中取出每个动作的「活动会话是否允许」与对应的程序名。").Then("二十多行，每行如 `yes  steamos-set-timezone`、`yes  steamos-enable-sshd`、`yes  steamos-priv-write`：第一列 `yes` 表示当前登录用户可以免密码执行。"),
            Step.Read(
                "看一个辅助程序具体做了什么",
                "cat /usr/bin/steamos-polkit-helpers/steamos-enable-sshd",
                "其他辅助程序同样可以直接查看，大多只有几行。").Then("十来行脚本：先判断 `$EUID`，不是 root 就 `exec pkexec --disable-internal-agent \"$0\" \"$@\"`，最后一行 `exec systemctl enable --now sshd`。"),
            Step.Read(
                "试一个只读的辅助程序",
                "/usr/bin/steamos-polkit-helpers/jupiter-get-als-gain",
                "读取环境光传感器的增益校准值，用来确认辅助程序在当前会话中可以免密码运行。").Then("没有密码提示，直接输出一个小数，如 `13.196093956188967`。若在 SSH 会话中执行，会提示需要认证。"),
            Step.Read(
                "检查系统更新与当前通道",
                "steamos-update check; steamos-select-branch -c",
                "这两条本身就允许普通用户运行。").Then("有更新时输出 `Update available` 和新版本号（如 `20260922.1`），否则提示已是最新；第二条输出当前通道，如 `rel`。"),
            Step.Change(
                "设置时区",
                "/usr/bin/steamos-polkit-helpers/steamos-set-timezone <时区>",
                "`<时区>` 如 `Asia/Shanghai`，可用 `timedatectl list-timezones | grep <城市英文名>` 查找。").Then("没有输出，也没有密码提示；`timedatectl show -p Timezone` 显示新的时区，系统时间随之改变。"),
            Step.Change(
                "设置主机名",
                "/usr/bin/steamos-polkit-helpers/steamos-set-hostname <主机名>",
                "主机名用英文字母、数字和连字符。它会出现在终端提示符、局域网设备列表中。").Then("没有输出；`hostnamectl hostname` 显示新名字，新打开的终端提示符随之改变。"),
            Step.Change(
                "不用 sudo 启用 SSH 服务",
                "/usr/bin/steamos-polkit-helpers/steamos-enable-sshd && systemctl is-active sshd",
                "从电脑登录 Deck 前还需要为 deck 用户设置密码（`passwd`），或者配置密钥（见「SSH 远程登录与传文件」）。").Then("输出 `Created symlink ...`（首次启用时）与 `active`。之后 sshd 开机自动启动，局域网内可以用 `ssh deck@<Deck的IP>` 登录。"),
            Step.Change(
                "通过白名单写入屏幕亮度（示例）",
                "cat /sys/class/backlight/amdgpu_bl0/max_brightness; /usr/bin/steamos-polkit-helpers/steamos-priv-write /sys/class/backlight/amdgpu_bl0/brightness <数值>",
                "`<数值>` 介于 0 与第一条输出的最大值之间，例如最大值的一半。不在白名单中的路径会被拒绝。").Then("先输出最大值（如 `599000`），随后屏幕亮度立即改变，并输出 `checking: ...` 与 `commit: <数值> -> /sys/class/backlight/amdgpu_bl0/brightness`；拖动亮度滑块即可恢复。"),
            Step.Change(
                "更新后出问题：重启回到另一个槽位",
                "/usr/bin/steamos-polkit-helpers/steamos-reboot-other",
                "先保存所有文件。回到原槽位时在那个系统里再执行一次即可。").Then("整机立即重启，从另一个 A/B 槽位启动（通常是这次更新之前的系统）。启动后 `cat /etc/os-release | grep BUILD_ID` 显示的版本号会不同。"),
        ],

        Verify =
        [
            "1. 执行任一只读或设置类辅助程序时没有出现密码提示。",
            "2. `journalctl -b --no-pager | grep -i pkexec | tail -n 3` 能看到 pkexec 以 root 身份执行这些程序的记录。",
        ],

        Related = ["steamos-commands", "steamosctl", "steamos-immutable", "sudo-users", "ssh-remote", "steamos-storage-subsystem"],
    };
}
