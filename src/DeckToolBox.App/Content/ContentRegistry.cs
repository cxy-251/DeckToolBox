using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;
using DeckToolBox.App.Content.Modules.Essentials;
using DeckToolBox.App.Content.Modules.Manual.Files;
using DeckToolBox.App.Content.Modules.Manual.Text;
using DeckToolBox.App.Content.Modules.Manual.Shell;
using DeckToolBox.App.Content.Modules.Manual.Archive;
using DeckToolBox.App.Content.Modules.Manual.Process;
using DeckToolBox.App.Content.Modules.Manual.Systemd;
using DeckToolBox.App.Content.Modules.Manual.Disk;
using DeckToolBox.App.Content.Modules.Manual.Network;
using DeckToolBox.App.Content.Modules.Manual.Media;
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
        new("basics", "🧭", "入门必修", "Linux 入门必修",
            "任何 Linux 都通用的日常技能与系统自带命令：终端、文件、权限、存储、网络、设备、排障与自动化。新手从这里开始。",
        [
            new("终端基础", "⌨️", "命令行用法、文件操作、Shell 配置、编辑器与 tmux",
            [
                TerminalBasicsData.Create(),
                FilesAndTextData.Create(),
                ShellConfigData.Create(),
                EditorsData.Create(),
                TmuxData.Create(),
            ]),
            new("用户与权限", "🔑", "sudo、密码与用户组",
            [
                SudoAndUsersData.Create(),
            ]),
            new("存储与备份", "💽", "外接存储、压缩包与 rsync 备份、硬盘健康与固件",
            [
                RemovableDrivesData.Create(),
                ArchivesBackupData.Create(),
                DiskHealthFirmwareData.Create(),
            ]),
            new("网络与远程", "📡", "Wi-Fi 排查、SSH 与防火墙",
            [
                NetworkBasicsData.Create(),
                SshRemoteData.Create(),
                FirewallData.Create(),
            ]),
            new("设备与桌面控制", "🔌", "蓝牙、音频、电源休眠与 KDE 桌面的命令行控制",
            [
                BluetoothData.Create(),
                AudioControlData.Create(),
                PowerSleepData.Create(),
                KdeCliData.Create(),
            ]),
            new("命令行工具箱", "🧰", "文本处理、JSON、音视频、校验与加密",
            [
                TextProcessingData.Create(),
                JqJsonData.Create(),
                FfmpegData.Create(),
                ChecksumCryptoData.Create(),
            ]),
            new("系统信息、排障与自动化", "🛠️", "硬件信息、进程管理、查日志找原因、定时执行任务",
            [
                SystemInfoData.Create(),
                ProcessBasicsData.Create(),
                TroubleshootingData.Create(),
                ScheduledTasksData.Create(),
            ]),
        ]),

        new("manual", "📖", "命令手册", "系统自带命令手册",
            "SteamOS 自带的命令逐个讲解：做什么用、常用选项、照着就能运行的例子，以及每条命令执行后会看到什么、改变了什么。",
        [
            new("文件与目录", "📁", "查看、复制、移动、删除、查找文件与目录",
            [
                LsData.Create(),
                MkdirTouchData.Create(),
                CpData.Create(),
                MvData.Create(),
                RmTrashData.Create(),
                LnData.Create(),
                FindData.Create(),
                FdData.Create(),
                StatFileData.Create(),
                TreeDuData.Create(),
                ChmodChownData.Create(),
                DiffCmpData.Create(),
                PathRenameData.Create(),
            ]),
            new("查看与处理文本", "📄", "查看、搜索、替换、统计文本，处理编码与二进制",
            [
                ViewTextData.Create(),
                GrepData.Create(),
                RgData.Create(),
                SedData.Create(),
                AwkData.Create(),
                TextToolsData.Create(),
                HexStringsData.Create(),
                EncodingData.Create(),
            ]),
            new("Shell 与脚本", "🐚", "重定向与管道、变量与引号、脚本、后台任务、快捷键与其他 shell",
            [
                RedirectPipeData.Create(),
                BashVariablesData.Create(),
                BashScriptData.Create(),
                XargsParallelData.Create(),
                JobControlData.Create(),
                WatchTimeoutData.Create(),
                DateCalcData.Create(),
                KeysHistoryData.Create(),
                FishZshData.Create(),
            ]),
            new("压缩与归档", "🗜️", "tar、各压缩算法、zip、7z 与 rar",
            [
                TarData.Create(),
                CompressorsData.Create(),
                ZipData.Create(),
                SevenZipData.Create(),
            ]),
            new("进程与系统监控", "📈", "查看、搜索、暂停、结束进程，按进程统计网络、磁盘与耗电",
            [
                PsData.Create(),
                TopHtopData.Create(),
                PgrepKillData.Create(),
                LsofFuserData.Create(),
                PerProcessIoData.Create(),
                LoadLoginData.Create(),
            ]),
            new("systemd 工具", "⚙️", "服务管理、日志查询、开机分析、临时任务与会话",
            [
                SystemctlData.Create(),
                JournalctlData.Create(),
                SystemdAnalyzeData.Create(),
                SystemdRunData.Create(),
                LoginctlData.Create(),
            ]),
            new("磁盘与文件系统", "💾", "识别磁盘、挂载镜像、分区格式化、写 U 盘、检测假卡、btrfs 与加密文件夹",
            [
                DiskIdentifyData.Create(),
                MountLoopData.Create(),
                MkfsFsckData.Create(),
                DdFlashData.Create(),
                F3Data.Create(),
                BtrfsCmdData.Create(),
                GocryptfsData.Create(),
            ]),
            new("网络", "🌐", "网络连接进阶、下载与请求、局域网共享、手机互联、SSH 转发、VPN 与二维码",
            [
                NmcliData.Create(),
                CurlWgetData.Create(),
                LanShareData.Create(),
                KdeConnectData.Create(),
                SshAdvancedData.Create(),
                VpnQrData.Create(),
            ]),
            new("多媒体与文档", "🎬", "声卡与混音器、语音合成、照片元数据、PDF 处理、音频编码",
            [
                AlsaToolsData.Create(),
                EspeakData.Create(),
                Exiv2Data.Create(),
                PdfToolsData.Create(),
                AudioCodecData.Create(),
            ]),
        ]),

        new("deck", "🎮", "Steam Deck 本机", "Steam Deck 本机 (SteamOS 3.x)",
            "只适用于 SteamOS / Steam Deck 的机制与操作：只读系统、装软件的正确方式、硬件与游戏兼容层。",
        [
            new("上手与系统更新", "🔒", "桌面模式基础、SteamOS 只读根分区、A/B 更新与恢复手段",
            [
                DesktopModeBasicsData.Create(),
                SteamOsImmutabilityData.Create(),
            ]),
            new("SteamOS 专有命令", "🎛️", "steamosctl 与 steamos-* 系列命令：在终端里完成快捷菜单和设置里的操作",
            [
                SteamosctlData.Create(),
                SteamosCommandsData.Create(),
            ]),
            new("安装软件的正确方式", "📦", "在只读系统上安装软件：便携应用与 Distrobox 容器；不使用 Flatpak",
            [
                PortableAppsData.Create(),
                DistroboxContainerData.Create(),
                PackageManagementData.Create(),
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
            new("先看这里", "🧭", "识别发行版，以及同一操作在各家包管理器中的写法",
            [
                PackageManagerRosettaData.Create(),
            ]),
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
