using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamosCommandsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steamos-commands",
        Title = "SteamOS 系统命令集",
        Keywords = "steamos-* · jupiter-* · steam:// · 系统报告",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 在 /usr/bin 里自带了几十个 steamos-* 和 jupiter-* 命令，设置界面里的很多按钮就是调用它们。认识其中常用的几个：切换会话、更新系统、生成诊断报告、检查固件、把程序加进 Steam，以及用 steam:// 链接从终端控制 Steam。",

        Why =
        [
            "1. 这些命令大多是 shell 脚本，可以直接阅读：`less /usr/bin/steamos-session-select`。想知道设置里某个按钮做了什么，读脚本是最直接的办法。",
            "2. 常用命令一览：",
            "- `steamos-session-select`：切换会话。`plasma` 进入桌面模式，`gamescope` 回到游戏模式（桌面上「返回游戏模式」图标调用的就是它）。",
            "- `steamos-update` / `steamos-select-branch`：检查与安装系统更新、切换更新通道（见「只读系统与 A/B 更新」）。",
            "- `steamos-systemreport`：收集系统版本、硬件、驱动、音频、日志等诊断信息，输出很长，适合保存为文件附在求助帖中。",
            "- `steamos-add-to-steam <路径>`：把一个程序添加为非 Steam 游戏，相当于 Dolphin 右键菜单中的「添加到 Steam」。",
            "- `jupiter-controller-update --check`：检查内置手柄固件是否需要更新。",
            "- `steamos-readonly`：只读保护开关。",
            "- `steamos-reboot`：带引导选项的重启。`--reboot-other` 下次从另一个 A/B 槽位启动，`--reboot-to-firmware-setup` 下次进入 BIOS 设置，`--list-firmware-entries` 列出 UEFI 启动项。设置下次启动目标需要写 EFI 变量，要用 sudo，设置完会立即重启。只想进 BIOS 时，systemd 通用的 `systemctl reboot --firmware-setup` 不需要 sudo。",
            "- `steamos-factory-reset`：恢复出厂设置，**会清除所有用户数据**，请只通过「设置 → 系统 → 恢复出厂设置」操作。",
            "3. `steam://` 链接：Steam 运行时，执行 `steam steam://<命令>` 会把命令交给正在运行的客户端。常用的有：",
            "- `steam://rungameid/<AppID>`：启动游戏。",
            "- `steam://validate/<AppID>`：验证游戏文件完整性。",
            "- `steam://open/downloads`、`steam://open/screenshots`：打开下载页、截图管理器。",
            "- `steam://open/console`：打开 Steam 控制台（高级调试用）。",
        ],

        Consequences =
        [
            "• `--check`、查看报告、阅读脚本都是只读的。",
            "• `steamos-session-select` 会立即结束当前会话，先保存所有文件。",
            "• `systemctl reboot --firmware-setup` 与 `steamos-reboot` 的各种重启选项都会立即重启，先保存所有文件。",
            "• 不要直接运行 `jupiter-biosupdate`、`steamos-factory-reset`、`steamos-partition` 等底层命令：它们用于刷写固件、重置和分区，误用可能导致无法开机。系统会在需要时自动调用它们。",
            "• `steamos-systemreport` 的输出包含设备序列号、用户名、网络信息等，公开分享前检查并删除敏感内容。",
        ],

        Steps =
        [
            Step.Read(
                "列出所有 SteamOS 专有命令",
                "ls /usr/bin | grep -E '^(steamos-|jupiter-|galileo-)'",
                "`galileo-*` 只在 OLED 版上起作用。").Then("列出六十多个命令名，例如 `steamos-readonly`、`steamos-session-select`、`jupiter-biosupdate`。"),
            Step.Read(
                "查看某个命令的实现",
                "less /usr/bin/steamos-session-select",
                "`file /usr/bin/<命令>` 可以先判断它是脚本还是二进制程序。").Then("在终端里分页显示这个脚本的源代码。空格翻页、`/` 搜索、`q` 退出；不会执行它。"),
            Step.Read(
                "列出 UEFI 启动项",
                "steamos-reboot --list-firmware-entries",
                "内部调用 `efibootmgr` 读取固件中登记的启动项，只读。").Then("先列出 `Boot0000`、`Boot2001` 等编号，再逐行列出名称，如 `EFI Hard Drive (...Micron_2400...)`、`EFI USB Device`、`EFI Network`。"),
            Step.Change(
                "从终端进入桌面模式 / 回到游戏模式",
                "steamos-session-select gamescope",
                "进入桌面模式用 `steamos-session-select plasma`。").Then("当前会话立即结束，屏幕黑一两秒后进入游戏模式；所有桌面程序都会被关闭。"),
            Step.Change(
                "重启并进入 BIOS 设置",
                "systemctl reboot --firmware-setup",
                "由 logind 设置「下次启动进入固件设置」再重启；当前登录用户不需要 sudo。效果等同于关机后按住「音量 +」再按电源键。").Then("所有程序被关闭，整机立即重启，开机后直接显示 Deck 的 BIOS 设置界面；在其中选择退出即可正常启动系统。"),
            Step.Change(
                "生成系统诊断报告",
                "steamos-systemreport > ~/steamos-report-$(date +%F).txt 2>&1; ls -lh ~/steamos-report-*.txt",
                "报告开头会显示只读保护状态，以及系统是否曾被以读写方式挂载过。").Then("终端先静默运行十几秒，然后 `ls` 显示生成的文件，例如 `~/steamos-report-2026-09-28.txt`，大小约 100 KB。可用 `less` 打开查看。"),
            Step.Read(
                "检查手柄固件是否需要更新",
                "jupiter-controller-update --check",
                "检查内置手柄的固件版本。").Then("输出几行检查过程，最后一行 `needs_update=0` 表示已是最新，`needs_update=1` 表示有新固件（系统会在更新时自动刷写）。只检查，不会刷写。"),
            Step.Root(
                "检查 BIOS 固件版本与更新状态",
                "sudo jupiter-biosupdate check",
                "`check` 只比较版本，不刷写。需要 root 是因为它要读取 DMI 表来识别机型；不加 `check` 会真的刷写 BIOS，不要那样执行。").Then("输出几行 `INFO:`，包括 `Checking for BIOS updates` 与当前 BIOS 版本；已是最新时以「无需更新」的说明结束，有新版本时输出 `BIOS <版本号>` 和 `Updates available, not applying in check mode`。"),
            Step.Read(
                "查看当前 Wi-Fi 后端",
                "steamos-wifi-set-backend --check",
                "NetworkManager 管理 Wi-Fi 时依赖的后端程序。个别路由器与 `iwd` 配合不好、频繁断线时，可以执行 `steamos-wifi-set-backend wpa_supplicant` 切换（会重启网络服务），换回用 `iwd`。").Then("输出一个词，本机为 `iwd`。"),
            Step.Change(
                "把一个程序添加到 Steam 库",
                "steamos-add-to-steam ~/Applications/<程序>",
                "需要 Steam 正在运行。").Then("Steam 窗口弹出或闪一下，该程序出现在「库 → 非 Steam」分类中，终端里没有输出。"),
            Step.Change(
                "从终端启动一个游戏",
                "steam steam://rungameid/<AppID>",
                "`<AppID>` 是游戏的数字编号，查法见「Wine 与 Proton 原理」或「Steam 目录结构与关键文件」。需要 Steam 正在运行。").Then("终端立即返回，Steam 开始启动该游戏，与在库中点「开始游戏」效果相同。"),
            Step.Change(
                "验证游戏文件完整性",
                "steam steam://validate/<AppID>",
                "`<AppID>` 同上，也可以从商店页网址 `store.steampowered.com/app/<AppID>/` 中看到。与游戏属性 → 已安装文件 → 验证完整性相同。").Then("Steam 窗口显示「正在验证文件」进度条，结束后提示有几个文件验证失败并自动重新下载它们。"),
        ],

        Verify =
        [
            "1. `head -n 8 ~/steamos-report-*.txt` 能看到只读保护状态与报告生成时间。",
            "2. `steam steam://open/downloads` 执行后，Steam 窗口切换到下载页面。",
        ],
    };
}
