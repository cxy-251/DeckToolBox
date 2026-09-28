using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NativeContainers;

public static class PressureVesselData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "pressure-vessel",
        Title = "Steam Runtime 游戏容器",
        Keywords = "pressure-vessel · bwrap · sniper",
        Level = ModuleLevel.Advanced,
        Overview = "揭秘 SteamOS 底层类似于 Docker 的轻量非特权容器技术，深入 Linux 命名空间与游戏环境沙箱隔离。",

        Why =
        [
            "1. 为什么 Steam 游戏需要独立的轻量容器？",
            "• 游戏通常依赖特定版本的 glibc、动态库与编译环境。传统 Linux 发行版升级往往导致老游戏崩溃。Valve 为此开发了 Steam Runtime（包含 Sniper 与 Soldier 两个容器镜像）。",
            "• 每次你点击【开始游戏】，Steam 在后台调用 Pressure-Vessel，利用底层 Bubblewrap (bwrap) 建立命名空间沙箱，将一个纯净的只读 Linux 环境挂载给游戏，完全与宿主机系统包解耦！",
            "2. 底层支柱：Bubblewrap (bwrap) 核心机制：",
            "• 非特权沙箱 (Unprivileged)：bwrap 借助 Linux 内核 user_namespaces，无需 root 提权即可动态挂载文件系统与隔离进程树。",
            "• Mount Namespace 隔离：游戏看到的是容器内的虚拟 /usr 和 /lib，但宿主机的 Mesa 驱动、Vulkan 层和 PipeWire 音频通过 bind mount 穿透进去，实现零额外性能损耗。",
            "• PID 隔离：游戏在容器中拥有专属 PID 树，游戏退出时，Pressure-Vessel 能瞬间收割并彻底杀死所有残留子进程，防止后台卡死。",
        ],

        Consequences =
        [
            "• 很多 Mod 注入器在 Linux 下失效的原因：很多开发者在宿主机终端里运行外部注入器或调试器，但游戏本身运行在 bwrap 隔离容器内，外部进程根本无法直接访问容器内的虚拟内存与私有命名空间！",
            "• 正确姿势：Mod 注入和挂钩必须直接放入游戏目录让 Proton / 容器内部自身加载（如 BepInEx / Harmony），或者使用容器调试参数进入同一个命名空间。",
        ],

        Steps =
        [
            Step.Read(
                "检查 Steam 原生预装的 Pressure-Vessel 运行时镜像",
                "ls -la ~/.local/share/Steam/steamapps/common/SteamLinuxRuntime_*",
                "查看当前设备上持有的官方容器环境版本。"),
            Step.Read(
                "查看正在运行的 bwrap 原生沙箱容器进程树",
                "ps aux | grep -E \"(pressure-vessel|bwrap)\" | grep -v grep",
                "启动任何 Steam 游戏后执行此命令，即可抓到后台正在运行的容器沙箱。"),
            Step.Read(
                "动手实验：无需 Docker，用系统自带的 bwrap 手动拉起一个轻量隔离 Shell",
                "bwrap --ro-bind /usr /usr --ro-bind /lib /lib --ro-bind /lib64 /lib64 --proc /proc --dev /dev --unshare-all /bin/bash",
                "利用 Linux 原生内核特性，零开销创建独立沙箱并赋予虚拟命名空间！"),
        ],

        Verify =
        [
            "进入由 bwrap 启动的隔离 Shell 后，在里面执行 ps aux，你会惊奇地发现：宿主机的几百个进程全都不见了，容器内只有 PID 1 和你的 bash！这就是完全原生的 Linux 容器威力。",
        ],
    };
}
