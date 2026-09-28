using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;
using DeckToolBox.App.Content.Modules.SteamDeckNative;
using DeckToolBox.App.Content.Modules.UniversalDistros;
using DeckToolBox.App.Content.Modules.SystemHygiene;
using DeckToolBox.App.Content.Modules.DisplayAndDesktop;
using DeckToolBox.App.Content.Modules.NativeContainers;
using DeckToolBox.App.Content.Modules.LaunchAndIntegration;
using DeckToolBox.App.Content.Modules.KernelVfs;
using DeckToolBox.App.Content.Modules.ProcessesAndSystemd;
using DeckToolBox.App.Content.Modules.StorageAndFilesystems;
using DeckToolBox.App.Content.Modules.MemoryAndCgroups;
using DeckToolBox.App.Content.Modules.NetworkingAndSockets;
using DeckToolBox.App.Content.Modules.SecurityAndCapabilities;
using DeckToolBox.App.Content.Modules.AudioAndMultimedia;
using DeckToolBox.App.Content.Modules.KernelModulesAndDrivers;
using DeckToolBox.App.Content.Modules.DiagnosticsAndTracing;

namespace DeckToolBox.App.Content;

/// <summary>
/// 全部内容的目录：专区 → 分类 → 模块。新增模块只需在对应分类里加一行 Xxx.Create()。
/// 修改后运行 `./run.sh --check` 做内容静态检查。
/// </summary>
public static class ContentRegistry
{
    public static IReadOnlyList<KnowledgeScope> Scopes { get; } =
    [
        new("deck", "🎮", "Steam Deck 本机", "Steam Deck 本机 (SteamOS 3.x)",
            "只适用于 SteamOS / Steam Deck 的机制与操作：只读系统、装软件的正确方式、硬件与游戏兼容层。",
        [
            new("系统与更新", "🔒", "SteamOS 只读根分区、A/B 更新与恢复手段",
            [
                SteamOsImmutabilityData.Create(),
            ]),
            new("安装软件的正确方式", "📦", "在只读系统上安装软件：Flatpak、便携应用与 Distrobox 容器",
            [
                PackageManagementData.Create(),
                PortableAppsData.Create(),
                DistroboxContainerData.Create(),
            ]),
            new("存储与清理", "🧹", "日志、缓存、着色器与回收站的空间治理",
            [
                SystemStorageHygieneData.Create(),
            ]),
            new("硬件、显示与输入", "⚡", "传感器与功耗、Gamescope 合成器、手柄输入链",
            [
                DeckHardwareEcData.Create(),
                GamescopeArchitectureData.Create(),
                DeckInputSubsystemData.Create(),
            ]),
            new("游戏兼容与启动", "🍷", "Proton / Wine、启动选项、非 Steam 程序与 Steam Runtime 容器",
            [
                WineProtonRuntimeData.Create(),
                ProtonRuntimeDeepData.Create(),
                LaunchOptionsData.Create(),
                GameModeIntegrationData.Create(),
                PressureVesselData.Create(),
            ]),
        ]),

        new("distros", "🐧", "其他发行版", "主流 Linux 发行版与包管理",
            "各大发行版家族的包管理方式。每个模块第一步都能在 Deck 上用 Distrobox 建一个对应容器亲手练习。",
        [
            new("Debian / Ubuntu", "🍥", ".deb、dpkg 与 APT 软件源",
            [
                DebianAptEcosystemData.Create(),
            ]),
            new("Fedora / RHEL", "🎩", "RPM、DNF 与事务回滚",
            [
                RedHatRpmDnfData.Create(),
            ]),
            new("Arch Linux", "🏹", "pacman、AUR 与滚动更新（SteamOS 的上游）",
            [
                ArchPacmanAurData.Create(),
            ]),
            new("openSUSE", "🦎", "Zypper 与 Snapper 快照回滚",
            [
                SuseZypperSnapperData.Create(),
            ]),
            new("Alpine", "🏔️", "apk、musl libc 与容器镜像",
            [
                AlpineApkMuslData.Create(),
            ]),
            new("原子化与跨发行版格式", "❄️", "Nix、rpm-ostree 与 Flatpak / Snap / AppImage",
            [
                NixGuixDeclarativeData.Create(),
                UniversalPackageFormatsData.Create(),
            ]),
        ]),

        new("internals", "🧠", "系统原理", "Linux 系统原理与排障",
            "所有发行版通用的底层机制：进程、服务、存储、内存、网络、权限、音频与驱动。",
        [
            new("进程与服务", "⚙️", "进程状态与信号、systemd 与日志、D-Bus",
            [
                ProcessLifecycleSignalsData.Create(),
                SystemdAndJournalData.Create(),
                DbusInterProcessData.Create(),
            ]),
            new("内核虚拟文件系统", "📂", "/proc、/sys 与 /dev：内核把一切暴露成文件",
            [
                ProcVirtualFsData.Create(),
                SysDeviceModelData.Create(),
                DeviceNodesUdevData.Create(),
            ]),
            new("存储与文件系统", "💾", "挂载、OverlayFS、ext4 与 Btrfs",
            [
                MountsAndOverlayFsData.Create(),
                BtrfsAndFilesystemsData.Create(),
            ]),
            new("内存与资源控制", "🧮", "Page Cache、ZRAM、OOM 与 cgroups v2",
            [
                MemoryAndOomData.Create(),
                CgroupsResourceControlData.Create(),
            ]),
            new("网络", "🌐", "网络协议栈诊断与 UNIX 域套接字",
            [
                LinuxNetworkStackData.Create(),
                UnixDomainSocketsData.Create(),
            ]),
            new("权限与安全", "🛡️", "文件权限、ACL 与 Capabilities",
            [
                PosixPermissionsAclData.Create(),
                LinuxCapabilitiesData.Create(),
            ]),
            new("桌面、显示与音频", "🖥️", "Wayland / X11、XDG 规范、PipeWire",
            [
                DisplayServerStackData.Create(),
                XdgStandardsData.Create(),
                AudioStackPipewireData.Create(),
            ]),
            new("驱动与性能", "🔬", "内核模块、性能观测与追踪",
            [
                KernelModulesKmodData.Create(),
                PerformanceTracingData.Create(),
            ]),
        ]),
    ];
}
