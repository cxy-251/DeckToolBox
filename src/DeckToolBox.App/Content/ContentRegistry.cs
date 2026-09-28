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

public static class ContentRegistry
{
    public static List<CategoryDefinition> LoadAllCategories()
    {
        return new List<CategoryDefinition>
        {
            // =========================================================================
            // 专区一：🎮 Steam Deck 本机专精 (SteamDeckNative)
            // 聚焦掌机硬件特化、Gamescope、不可变系统、Proton 转译与手柄传感器
            // =========================================================================

            new(
                "系统纯净化与便携规范",
                "🧹",
                "彻底清理 Flatpak 沙箱孤儿与依赖，建立绿色便携标准与持久缓存治理",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    PackageManagementData.Create(),
                    PortableAppsData.Create(),
                    SystemStorageHygieneData.Create()
                }
            ),

            new(
                "不可变系统与更新机制",
                "🔒",
                "掌握 SteamOS 3.x 工业级 A/B 根分区对刷机制、RAUC 控制器与只读保护",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    SteamOsImmutabilityData.Create()
                }
            ),

            new(
                "掌机微合成器与显示流水线",
                "🎯",
                "解析 Valve 自研 Gamescope 微型合成器、DRM Lease 显卡独占与 FSR 低延时锁帧",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    GamescopeArchitectureData.Create()
                }
            ),

            new(
                "硬件 EC 控制器与 APU 调控",
                "⚡",
                "深入嵌入式控制器 (EC)、hwmon 传感器拓扑、jupiter-fan 风扇静音与 TDP 功耗墙",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    DeckHardwareEcData.Create()
                }
            ),

            new(
                "输入拓扑与虚拟手柄总线",
                "🕹️",
                "剖析 Linux IIO 工业陀螺仪、hid-steam 内核驱动与 /dev/uinput 虚拟游戏手柄映射",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    DeckInputSubsystemData.Create()
                }
            ),

            new(
                "Proton 兼容层与运行库补丁",
                "🍷",
                "掌握虚拟 C 盘 (compatdata/pfx)、Protontricks 运行库静默注入与 DLL 劫持注塑",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    ProtonRuntimeDeepData.Create(),
                    WineProtonRuntimeData.Create()
                }
            ),

            new(
                "原生沙箱与运行时",
                "📦",
                "掌握 SteamOS 官方预装的 Distrobox 用户开发沙箱与 Valve 游戏专用 Pressure-Vessel",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    DistroboxContainerData.Create(),
                    PressureVesselData.Create()
                }
            ),

            new(
                "启动编排与游戏模式接入",
                "🚀",
                "非 Steam 应用接入全屏游戏流、shortcuts.vdf 与 Steam 启动项参数注入",
                KnowledgeScope.SteamDeckNative,
                new List<ModuleDefinition>
                {
                    GameModeIntegrationData.Create(),
                    LaunchOptionsData.Create()
                }
            ),

            // =========================================================================
            // 专区二：🐧 广域 Linux 宇宙与全景包管理 (UniversalDistros)
            // 聚焦四大包管理家族、下一代函数式系统、容器发行版等全生态分发哲学
            // =========================================================================

            new(
                "Debian / Ubuntu 体系",
                "🍥",
                "解析装机量最大的 Debian 家族：.deb ar 归档、dpkg 状态机与 APT SAT 依赖求解",
                KnowledgeScope.UniversalDistros,
                new List<ModuleDefinition>
                {
                    DebianAptEcosystemData.Create()
                }
            ),

            new(
                "Red Hat / Fedora 体系",
                "🎩",
                "深入企业级霸主：RPM 规范、DNF C 语言 libsolv 极速求解与事务历史原子回滚",
                KnowledgeScope.UniversalDistros,
                new List<ModuleDefinition>
                {
                    RedHatRpmDnfData.Create()
                }
            ),

            new(
                "Arch Linux 体系",
                "🏹",
                "极简主义哲学：.pkg.tar.zst、pacman 拒绝暗箱交互、PKGBUILD 规范与 AUR 社区",
                KnowledgeScope.UniversalDistros,
                new List<ModuleDefinition>
                {
                    ArchPacmanAurData.Create()
                }
            ),

            new(
                "openSUSE 体系",
                "🦎",
                "欧洲工业界标杆：Zypper 严谨求解、Patterns 场景套件与 Snapper Btrfs 开机回滚",
                KnowledgeScope.UniversalDistros,
                new List<ModuleDefinition>
                {
                    SuseZypperSnapperData.Create()
                }
            ),

            new(
                "极简与容器发行版",
                "🏔️",
                "云原生容器霸主：Alpine Linux 5MB 奇迹、apk 零缓存规范与 musl libc 架构差异",
                KnowledgeScope.UniversalDistros,
                new List<ModuleDefinition>
                {
                    AlpineApkMuslData.Create()
                }
            ),

            new(
                "函数式与不可变发行版",
                "❄️",
                "操作系统未来形态：Nix /nix/store 纯函数存储消灭依赖地狱与 Silverblue rpm-ostree",
                KnowledgeScope.UniversalDistros,
                new List<ModuleDefinition>
                {
                    NixGuixDeclarativeData.Create()
                }
            ),

            new(
                "通用沙箱与跨发行版打包",
                "📦",
                "终结碎片化分发：Flatpak vs Snap vs AppImage 内核隔离与分发模型全景深度横评",
                KnowledgeScope.UniversalDistros,
                new List<ModuleDefinition>
                {
                    UniversalPackageFormatsData.Create()
                }
            ),

            // =========================================================================
            // 专区三：🧠 通用内核底石与系统运行态 (LinuxKernelInternals)
            // 跨发行版通用的操作系统底层子系统与硬件通信机制
            // =========================================================================

            new(
                "内核虚拟文件系统",
                "📂",
                "透过 /proc, /sys 与 /dev 洞悉内核运行态、硬件拓扑与设备事件",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    ProcVirtualFsData.Create(),
                    SysDeviceModelData.Create(),
                    DeviceNodesUdevData.Create()
                }
            ),

            new(
                "显示服务与桌面规范",
                "🖥️",
                "深入理解 Linux 现代合成器与桌面菜单标准 (Wayland / X11 / XDG)",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    DisplayServerStackData.Create(),
                    XdgStandardsData.Create()
                }
            ),

            new(
                "进程与系统编排",
                "⚙️",
                "深入进程状态机、信号机制、Systemd 单元治理与 D-Bus 消息总线",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    ProcessLifecycleSignalsData.Create(),
                    SystemdAndJournalData.Create(),
                    DbusInterProcessData.Create()
                }
            ),

            new(
                "存储体系与联合挂载",
                "💾",
                "掌握 VFS 统一目录树、只读挂载防护、OverlayFS 联合分层与 Btrfs 写时复制",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    MountsAndOverlayFsData.Create(),
                    BtrfsAndFilesystemsData.Create()
                }
            ),

            new(
                "内存模型与资源限制",
                "📊",
                "破解 Page Cache 认知误区，掌握 ZRAM 压缩换页、OOM Killer 与 cgroups v2 配额硬限制",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    MemoryAndOomData.Create(),
                    CgroupsResourceControlData.Create()
                }
            ),

            new(
                "网络通信与套接字架构",
                "🌐",
                "掌握 UNIX 域套接字零拷贝 IPC、Linux 网络协议栈、路由拓扑与套接字诊断",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    UnixDomainSocketsData.Create(),
                    LinuxNetworkStackData.Create()
                }
            ),

            new(
                "安全体系与特权细分",
                "🛡️",
                "打破 root 二元特权垄断，掌握 POSIX Capabilities 权能细分与 ACL 文件访问控制列表",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    LinuxCapabilitiesData.Create(),
                    PosixPermissionsAclData.Create()
                }
            ),

            new(
                "音频架构与多媒体总线",
                "🎵",
                "全面剖析 ALSA 内核驱动、PipeWire 纳秒级图处理引擎与 WirePlumber 会话管理",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    AudioStackPipewireData.Create()
                }
            ),

            new(
                "内核模块与硬件抽象",
                "🧩",
                "深入可加载内核模块 (LKM) 动态链接、modprobe 依赖树与内核环形日志 (dmesg)",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    KernelModulesKmodData.Create()
                }
            ),

            new(
                "性能观测与动态追踪",
                "📈",
                "践行 Brendan Gregg USE 性能分析哲学，掌握 strace 系统调用拦截、perf PMU 采样与 eBPF",
                KnowledgeScope.LinuxKernelInternals,
                new List<ModuleDefinition>
                {
                    PerformanceTracingData.Create()
                }
            )
        };
    }
}
