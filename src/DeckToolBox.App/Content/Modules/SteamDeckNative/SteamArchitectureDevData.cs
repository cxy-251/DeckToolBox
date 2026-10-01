using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class SteamArchitectureDevData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "steam-architecture-dev",
        Title = "Steam 客户端：进程、日志与自动修复",
        Keywords = "steamwebhelper · reaper · steam-launcher.service · steam-short-session-tracker · steam -shutdown · console · devkit",
        Level = ModuleLevel.Advanced,
        Overview = "Steam 打不开、反复闪退、界面空白，或者想知道游戏退出后为什么 Steam 还显示「运行中」，需要先了解 Steam 客户端由哪些进程组成、在游戏模式中怎样被启动、日志写在哪里。SteamOS 还会在 Steam 连续启动失败时自动修复它，本模块也说明这个机制和手动触发的方法。",

        Why =
        [
            "1. 进程组成：",
            "- `steam`：客户端主进程，是一个 32 位程序（`~/.local/share/Steam/ubuntu12_32/steam`），负责登录、下载、云同步、启动游戏等。",
            "- `steamwebhelper`：基于 Chromium 的界面进程，商店、库、游戏模式的整个界面都由它绘制，通常有十个左右。它本身运行在 Steam Runtime 容器中，所以进程树里 `steam` 下面先是 `srt-bwrap`、`pv-adverb`，再是 `steamwebhelper`。",
            "- `reaper`：每个游戏的启动链中都有它（见「Steam Runtime 游戏容器」），负责跟踪游戏派生的所有进程。游戏窗口关了但 Steam 仍显示「运行中」，通常是还有子进程没退出。",
            "2. 游戏模式中的启动方式：Steam 由用户服务 `steam-launcher.service` 运行，它从 `$XDG_RUNTIME_DIR/gamescope-environment` 读取环境变量。服务启动前后各调用一次 `/usr/lib/steamos/steam-short-session-tracker`，记录这次运行持续了多久。",
            "3. 自动修复：如果 Steam 连续 3 次在启动后 60 秒内就退出，下次启动前 tracker 会自动修复：把 `~/.steam` 改名为 `~/dot-steam.bak.<时间戳>`（保留其中的 `registry.vdf`），再从系统自带的 `/usr/lib/steam/bootstraplinux_ubuntu12_32.tar.xz` 解压一份干净的引导程序到 `~/.local/share/Steam`。游戏、存档和 `steamapps` 不受影响。创建 `~/.config/inhibit-short-session-tracker` 文件可以关闭这个功能。",
            "4. 日志：客户端的日志都在 `~/.local/share/Steam/logs/`。`console_log.txt` 是主日志（含每次启动游戏的完整命令行），`bootstrap_log.txt` 记录启动与自更新，`webhelper*.txt` 与 `cef_log.txt` 记录界面进程，`compat_log.txt` 记录 Proton 的选择，其余按功能命名（下载、云、着色器、控制器等）。每次启动 Steam 时上一次的日志被改名为 `*.previous.txt`。",
            "5. 控制台：`steam steam://open/console` 打开客户端内置的「控制台」页面，可以执行 `app_info_print <AppID>` 等诊断命令。",
            "6. Devkit 服务：`steamos-devkit-service` 是一个系统服务，以 deck 用户身份在 TCP 32000 端口监听，供 PC 上的 SteamOS Devkit Client 发现这台 Deck、配对 SSH 密钥并远程部署程序。Steam 的开发者设置通过 polkit 辅助程序 `/usr/bin/steamos-polkit-helpers/steamos-devkit-mode --enable/--disable` 开启或关闭它。不做开发时不需要开启。",
            "7. `steam.sh --reset`：Steam 安装目录中的 `steam.sh` 还有一个更彻底的重置，只保留 `steamapps`、`userdata` 与下载缓存，删除其余全部内容（包括 `config/`、`compatibilitytools.d/` 与日志），需要重新登录。它要求先退出 Steam，且必须直接运行 `~/.local/share/Steam/steam.sh --reset`（`/usr/bin/steam` 会在参数前加上自己的选项，`--reset` 不会生效）。一般先用上面的自动修复，无效再考虑它。",
        ],

        Consequences =
        [
            "• 查看进程、端口、日志都是只读的。",
            "• `steam -shutdown` 会关闭 Steam 和由它启动的游戏；在游戏模式中 Steam 随后会被自动重新启动。",
            "• 手动触发修复后需要重新登录 Steam；备份目录 `~/dot-steam.bak.*` 确认无用后可以删除。",
            "• 控制台命令直接作用于客户端，只执行你了解用途的命令。",
        ],

        Steps =
        [
            Step.Read(
                "查看 Steam 的进程树",
                "pstree -T $(pgrep -n -x steam)",
                "`-T` 不显示线程，只看进程；`pgrep -n -x steam` 找到主进程。").Then("以 `steam` 为根的树：`steam-+-srt-bwrap---pv-adverb-+-steamwebhelper-+-...`，下面挂着多层 `steamwebhelper`，另有一个 `steam-runtime-l...`；游戏运行时还会出现 `reaper` 分支。"),
            Step.Read(
                "确认主进程是 32 位程序",
                "file -L /proc/$(pgrep -n -x steam)/exe | cut -c1-90",
                "`/proc/<PID>/exe` 指向进程正在运行的程序文件。").Then("输出 `/proc/<PID>/exe: ELF 32-bit LSB pie executable, Intel i386, ...`。"),
            Step.Read(
                "查看最近的客户端主日志",
                "tail -n 20 ~/.local/share/Steam/logs/console_log.txt",
                "Steam 打不开、界面异常时，先看这里和 `bootstrap_log.txt` 的末尾。").Then("最近 20 行带时间戳的记录，例如游戏进程的增减（`Game process added : AppID ...`）、Proton 工具的调用、各项任务的结果。"),
            Step.Read(
                "查看自动修复的设置与历史",
                "grep -E '^short_session' /usr/lib/steamos/steam-short-session-tracker; ls -d ~/dot-steam.bak.* ~/.config/inhibit-short-session-tracker 2>/dev/null || echo '（没有修复记录，自动修复未被关闭）'",
                "第一条显示判定规则；第二条列出以往自动修复留下的备份，以及是否关闭了这个功能。").Then("先输出 `short_session_tracker_file=\"/tmp/steamos-short-session-tracker\"`、`short_session_duration=60`、`short_session_count_before_reset=3` 等几行；从未修复过且未关闭时，第二条显示「没有修复记录」的提示。"),
            Step.Change(
                "退出 Steam",
                "steam -shutdown",
                "让 Steam 正常退出（保存设置、结束游戏），比直接结束进程安全。").Then("Steam 窗口关闭，几秒后 `pgrep -x steam` 不再有输出。桌面模式中从应用菜单重新打开；游戏模式中会自动重启。"),
            Step.Change(
                "手动触发 SteamOS 的 Steam 修复",
                "steam -shutdown; sleep 10; /usr/lib/steamos/steam-short-session-tracker --repair-now",
                "在桌面模式中执行。效果与连续闪退后的自动修复相同：备份 `~/.steam` 并重新解压引导程序。").Then("先输出 `steam-short-session-tracker: Re-bootstrapping steam from OS copy.`，没有其他输出。之后打开 Steam 会先自我更新，然后要求重新登录；家目录中多出 `dot-steam.bak.<时间戳>`。"),
            Step.Change(
                "打开 Steam 控制台",
                "steam steam://open/console",
                "需要 Steam 正在运行。在控制台输入框中可以执行如 `app_info_print 1493710`。").Then("Steam 窗口切换到「控制台」页面，上方是输出区，下方是命令输入框。"),
            Step.Read(
                "查看开发者模式服务是否在运行",
                "systemctl is-active steamos-devkit-service; ss -tlpn | grep 32000",
                "服务未启用时为 `inactive`，端口不监听。").Then("开启时输出 `active`，以及 `LISTEN ... 0.0.0.0:32000 ... users:((\"python\",...))`；关闭时只输出 `inactive`。"),
        ],

        Verify =
        [
            "1. `pgrep -c steamwebhelper` 输出一个大于 1 的数，说明界面进程正常运行。",
            "2. `systemctl --user cat steam-launcher.service | grep short-session` 显示 `ExecStartPre` 与 `ExecStopPost` 两行。",
        ],
    };
}
