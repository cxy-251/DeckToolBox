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
            "• Valve 提供 Steam Linux Runtime：`scout`（最早，基于 Ubuntu 12.04）、`soldier`（Proton 5.13~7 使用）、`sniper`（新版 Proton 与原生游戏使用）等，每个都是一套固定版本的用户空间库。",
            "2. pressure-vessel 做了什么：",
            "• 把运行时作为容器里的 `/usr`，同时把宿主的显卡驱动（Mesa、Vulkan 层）和必要设备带进容器，因此游戏既有稳定的库，又能用最新驱动。",
            "• 底层用 **bubblewrap (`bwrap`)**：借助 Linux 的用户命名空间 (user namespace)，普通用户无需 root 就能为进程构造独立的挂载视图和进程视图。",
            "3. 这对调试的影响：游戏运行在另一个挂载命名空间中，它看到的 `/usr` 与你终端里看到的不同；从宿主附加调试器、注入工具时要考虑这一点。",
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
                "`SteamLinuxRuntime_sniper` / `_soldier` 等目录各是一套运行时。"),
            Step.Read(
                "游戏运行时查看容器进程",
                "pgrep -a -f 'pressure-vessel|bwrap' | cut -c1-200",
                "启动任意游戏后在桌面模式终端执行；`cut` 截断过长的命令行。"),
            Step.Change(
                "用 bwrap 亲手启动一个最小沙箱",
                "bwrap --ro-bind /usr /usr --symlink usr/lib /lib --symlink usr/lib /lib64 --symlink usr/bin /bin --proc /proc --dev /dev --unshare-all --die-with-parent /bin/bash",
                "SteamOS 的 `/lib`、`/bin` 都是指向 `/usr` 的符号链接，所以用 `--symlink` 在沙箱里重建它们。输入 `exit` 退出沙箱。"),
            Step.Read(
                "在沙箱里观察隔离效果",
                "echo \"我的 PID: $$\"; ps -e; ls /; ls ~ 2>&1",
                "在上一步打开的沙箱 shell 中执行。").On("bwrap 沙箱内"),
        ],

        Verify =
        [
            "1. 沙箱里 `echo $$` 输出 `2`（bwrap 自己是 1 号进程），`ps -e` 只能看到寥寥几个进程，而宿主上有几百个。",
            "2. 沙箱里 `ls /` 只有 `bin dev lib lib64 proc sbin usr` 等少数目录，`ls ~` 报错「没有那个文件或目录」：家目录根本不存在于这个挂载视图中。",
        ],
    };
}
