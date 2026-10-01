using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NativeContainers;

public static class PressureVesselData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "pressure-vessel",
        Title = "Steam Runtime 游戏容器",
        Keywords = "pressure-vessel · bwrap · sniper · soldier",
        Level = ModuleLevel.Advanced,
        Overview = "Steam 启动游戏时，会先用 pressure-vessel 建一个轻量容器，让游戏看到一套固定版本的运行库。它用的 bubblewrap (bwrap) 也是 Flatpak 的沙箱基础，你可以亲手用它搭一个最小沙箱。",

        Why =
        [
            "1. 为什么需要游戏运行时：",
            "• 游戏发布后很少更新，而发行版的库在不断升级。游戏依赖的 glibc、SDL、图形库版本一旦和系统不一致，就可能无法启动。",
            "• Valve 提供 Steam Linux Runtime：`scout`（最早，基于 Ubuntu 12.04）、`soldier`（Proton 5.13~7 使用）、`sniper`（Proton 8~10 与较新的原生游戏使用）、`SteamLinuxRuntime_4`（steamrt4，Proton 11 起使用）等，每个都是一套固定版本的用户空间库。每个 Proton 目录里的 `toolmanifest.vdf` 用 `require_tool_appid` 指明自己需要哪一个。",
            "2. pressure-vessel 做了什么：",
            "• 把运行时作为容器里的 `/usr`，同时把宿主的显卡驱动（Mesa、Vulkan 层）和必要设备带进容器，因此游戏既有稳定的库，又能用最新驱动。",
            "• 底层用 **bubblewrap (`bwrap`)**：借助 Linux 的用户命名空间 (user namespace)，普通用户无需 root 就能为进程构造独立的挂载视图和进程视图。",
            "• Steam 客户端自己的网页界面 `steamwebhelper` 也运行在这样的容器里（进程树中可以看到 `srt-bwrap` 与 `pv-adverb`）。",
            "3. Steam 启动一个 Proton 游戏时，命令行由外到内是：",
            "- `steam-launch-wrapper`：调整 OOM 优先级等，然后执行后面的命令。",
            "- `reaper SteamLaunch AppId=<AppID> --`：跟踪游戏派生的全部子进程，Steam 据此判断游戏是否还在运行、点「停止」时一并结束它们。",
            "- 启动选项里写在 `%command%` 前面的包装器（如 `mangohud`、`gamemoderun`）插在这里；环境变量则加到整个进程环境中。",
            "- `SteamLinuxRuntime_*/_v2-entry-point --verb=waitforexitandrun --`：进入 pressure-vessel 容器。",
            "- `Proton */proton waitforexitandrun <游戏.exe>`：在容器内启动 Wine，`%command%` 后面的参数传给游戏本体。",
            "• Steam 每次启动游戏都会在 `~/.local/share/Steam/logs/console_log.txt` 中记一行 `Game process added : AppID ... \"<完整命令行>\"`，可以据此核对启动选项是否按预期生效。",
            "4. 这对调试的影响：游戏运行在另一个挂载命名空间中，它看到的 `/usr` 与你终端里看到的不同；从宿主附加调试器、注入工具时要考虑这一点。",
        ],

        Consequences =
        [
            "• 查看运行时目录和进程只读无害。",
            "• 下面的 bwrap 实验只创建一个临时沙箱：沙箱里只读挂载 `/usr`，看不到你的家目录，退出 shell 即销毁，不会修改任何文件。",
            "• 不要删除 `SteamLinuxRuntime_*` 目录，Steam 会在下次启动游戏时重新下载；要卸载请在 Steam 库的「工具」中操作。",
        ],

        Steps =
        [
            Step.Read(
                "查看已安装的 Steam 运行时",
                "ls -d ~/.local/share/Steam/steamapps/common/SteamLinuxRuntime*",
                "每个目录是一套运行时。").Then("列出几个目录路径，如 `SteamLinuxRuntime_sniper`、`SteamLinuxRuntime_soldier`。"),
            Step.Read(
                "查看每个 Proton 需要哪个运行时",
                "grep -H require_tool_appid ~/.local/share/Steam/steamapps/common/Proton*/toolmanifest.vdf",
                "`-H` 在每行前显示文件名。`1628350` 是 sniper，`4183110` 是 SteamLinuxRuntime_4，`1391110` 是 soldier。").Then("每个已安装的 Proton 一行，如 `.../Proton 10.0/toolmanifest.vdf:  \"require_tool_appid\" \"1628350\"`、`.../Proton 11.0/toolmanifest.vdf:  \"require_tool_appid\" \"4183110\"`。"),
            Step.Read(
                "查看 Steam 最近拼出的启动命令行",
                "grep -h 'Game process added' ~/.local/share/Steam/logs/console_log.txt | tail -n 2 | sed 's/ -- /\\n  -- /g'",
                "`sed` 在每个 ` -- ` 处换行，方便看清一层层的包装。先启动并退出一个 Proton 游戏再执行。").Then("最近两次启动，每次拆成多行：`steam-launch-wrapper --oom-score-adjust 900`、`-- .../reaper SteamLaunch AppId=<AppID>`，Proton 游戏还有 `-- .../SteamLinuxRuntime_4/_v2-entry-point --verb=waitforexitandrun` 与 `-- .../Proton 11.0/proton waitforexitandrun <程序>.exe`；原生程序和非 Steam 程序在 reaper 之后直接是程序路径。"),
            Step.Read(
                "游戏运行时查看容器进程",
                "pgrep -a -f 'pressure-vessel|bwrap' | cut -c1-200",
                "启动任意游戏后在桌面模式终端执行；`cut` 截断过长的命令行。").Then("列出 `pressure-vessel-wrap`、`bwrap` 等进程及其（被截断的）命令行；没有游戏运行时没有输出。"),
            Step.Change(
                "用 bwrap 亲手启动一个最小沙箱",
                "bwrap --ro-bind /usr /usr --symlink usr/lib /lib --symlink usr/lib /lib64 --symlink usr/bin /bin --proc /proc --dev /dev --unshare-all --die-with-parent /bin/bash",
                "SteamOS 的 `/lib`、`/bin` 都是指向 `/usr` 的符号链接，所以用 `--symlink` 在沙箱里重建它们；`--unshare-all` 隔离进程、网络等所有命名空间。").Then("提示符变为 `bash-5.x$` 这样的简单形式，说明你已经在沙箱里的新 bash 中。此时看不到家目录，也看不到系统的其他进程。输入 `exit` 返回原来的终端，沙箱随即消失。"),
            Step.Read(
                "在沙箱里观察隔离效果",
                "echo \"我的 PID: $$\"; ps -e; ls /; ls ~ 2>&1",
                "在上一步打开的沙箱 shell 中执行。").On("bwrap 沙箱内").Then("显示 `我的 PID: 2`；`ps -e` 只列出三四个进程；`ls /` 只有 `bin dev lib lib64 proc sbin usr`；`ls ~` 报「没有那个文件或目录」。"),
        ],

        Verify =
        [
            "1. 沙箱里 `echo $$` 输出 `2`（bwrap 自己是 1 号进程），`ps -e` 只能看到寥寥几个进程，而宿主上有几百个。",
            "2. 沙箱里 `ls /` 只有 `bin dev lib lib64 proc sbin usr` 等少数目录，`ls ~` 报错「没有那个文件或目录」：家目录根本不存在于这个挂载视图中。",
        ],
    };
}
