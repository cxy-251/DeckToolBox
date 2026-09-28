using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NativeContainers;

public static class WineProtonRuntimeData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "wine-proton",
        Title = "Wine 与 Proton 原理",
        Keywords = "Wine · DXVK · VKD3D · PROTON_LOG",
        Level = ModuleLevel.Intermediate,
        Overview = "彻底搞懂 Linux 运行 Windows 软件的底层原理：系统调用翻译、虚拟 WINEPREFIX 架构与 DXVK 图形转译。",

        Why =
        [
            "1. Wine 不是硬件模拟器（零 CPU 模拟损耗）：",
            "• 模拟器（如 3DS 模拟器）需要通过纯软件在内存里重建一套虚拟 CPU 芯片和寄存器，CPU 性能损失极大。而 Wine (Wine Is Not an Emulator) 是一套标准的“系统调用转译库”。当 Windows 软件调用 CreateFileW / VirtualAlloc 时，Wine 直接将其等价翻译为 Linux 内核原生的 open / mmap，CPU 指令是在物理 CPU 上以 100% 原生速度执行的！",
            "2. WINEPREFIX 架构：虚拟 C 盘与注册表容器：",
            "• 在 Linux 下，每个 Windows 应用都拥有独立的 WINEPREFIX（位于 Steam 的 compatdata/<AppID>/pfx 目录下）。它包含：",
            "  - drive_c/：伪装的 Windows 根目录（内有 Windows、Program Files、users/steamuser/AppData 等）。",
            "  - system.reg & user.reg：用可读纯文本存储的 Windows 注册表数据库。",
            "  - dosdevices/：Linux 软链接，c: 指向 drive_c，z: 映射到整个 Linux 宿主机文件系统。",
            "3. Valve Proton 的四大革命性技术支柱：",
            "• DXVK：利用 Vulkan 原生计算着色器，把 DirectX 9/10/11 图形指令实时编译为 Vulkan，帧率经常反超 Windows 原生。",
            "• VKD3D-Proton：负责将现代 DirectX 12 API 高效转译到 Vulkan。",
            "• Fsync (Fast Synchronization)：Valve 推动 Linux 内核引入的多线程同步机制，将 Windows 互斥锁映射为 Linux 原生极速同步，彻底根除掉帧卡顿。",
        ],

        Consequences =
        [
            "• 掌握存档真身：无论游戏怎么隐藏，其存档必然落在 compatdata/<AppID>/pfx/drive_c/users/steamuser/AppData 或 Documents 下，你可以直接手动备份与修改。",
            "• 摆脱黑盒恐慌：遇到游戏黑屏、视频播放失败时，不再盲目重装，而是通过 PROTON_LOG 输出精准定位是缺了哪个系统 DLL 或注册表键值。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统安装的 Valve 官方 Proton 运行时",
                "ls -la ~/.local/share/Steam/steamapps/common/Proton*",
                "列出当前机器可用的各个版本 Proton（Experimental, 10, 9 等）。"),
            Step.Read(
                "探测特定游戏或应用的虚拟 C 盘路径",
                "find ~/.local/share/Steam/steamapps/compatdata/ -maxdepth 2 -name \"pfx\"",
                "找到各个游戏的专属虚拟 Windows 环境目录。"),
            Step.Change(
                "启用 DXVK 原生 HUD 监控（帧率/显存/着色器管线实时显示）",
                "DXVK_HUD=fps,frametimes,devinfo %command%",
                "在 Steam 游戏属性的“启动选项”中填入此参数，即可在屏幕左上角唤出底层渲染监视器。"),
            Step.Change(
                "捕获游戏运行期底层崩溃日志 (排错神技)",
                "PROTON_LOG=1 %command%",
                "启动后日志会自动写入 ~/steam-<appid>.log，所有缺失的 DLL、崩溃断点一览无余。"),
        ],

        Verify =
        [
            "在 Steam 游戏启动后，检查 ~/ 目录下是否生成了 steam-<appid>.log，查看第一行即可看到完整的 Wine 版本、Vulkan 物理显卡设备信息及加载的动态转译链。",
        ],
    };
}
