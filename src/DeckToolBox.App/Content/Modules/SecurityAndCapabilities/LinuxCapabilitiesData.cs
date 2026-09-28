using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SecurityAndCapabilities;

public static class LinuxCapabilitiesData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "capabilities",
        Title = "Linux Capabilities",
        Keywords = "getcap · capsh · setcap",
        Level = ModuleLevel.Advanced,
        Overview = "彻底打破传统 UNIX 粗暴的 root/非 root 二元权限模型，深入解析 Linux POSIX Capabilities 权能分割机制、线程五大权能集与生产环境无 SUID 提权实践。",

        Why =
        [
            "1. 传统 UNIX 模型的重大安全隐患 (All-or-Nothing)：",
            "• 在传统 UNIX/Linux 体系中，权限判断仅由有效用户 ID (EUID == 0) 决定。一个程序要么是普通用户（权限处处受限），要么是 root（拥有修改内核、格式化硬盘、窥探任意进程内存的绝对无上特权）。",
            "• 过去普通程序需要执行特权操作时（例如 `ping` 需要原始套接字、Web 服务器需要绑定 80 端口），只能把程序设为 SUID-root (`chmod u+s`)。程序一旦有漏洞，攻击者就能直接获得 root 权限。",
            "2. Linux Capabilities 的细粒度权能解耦：",
            "• 从 Linux 2.2 开始，内核把 root 的特权拆分成 40 多种独立的权能 (Capabilities)：",
            "  - `CAP_NET_BIND_SERVICE`：仅允许绑定 < 1024 特权网络端口，剥夺其他任何破坏能力。",
            "  - `CAP_NET_RAW`：仅允许构造 RAW / PACKET 原始套接字（抓包工具需要；现代发行版的 `ping` 改用无需特权的 ICMP 数据报套接字，已经连这个权能都不需要了）。",
            "  - `CAP_SYS_NICE`：仅允许调整进程 nice 优先级与实时调度策略 (SCHED_FIFO / SCHED_RR)。SteamOS 上的 `gamescope` 和 `kwin_wayland` 通过文件权能获得它，用来提升自己的调度优先级，减少画面卡顿，而无需以 root 身份运行。",
            "  - `CAP_SYS_PTRACE`：允许通过 ptrace 系统调用调试或追踪其他进程（gdb / strace 核心权能）。",
            "  - `CAP_SYS_ADMIN`：被称为“新一代 root”，涵盖挂载文件系统、配置网络命名空间等广泛底层操作，容器运行时重点防护对象。",
            "3. 进程的五大权能集合 (Capability Sets)：",
            "• 内核在 `task_struct` 中为每个线程维护了 5 组 64-bit 位图：",
            "  - **Permitted (P)**：该进程能使用的最大权能超集。",
            "  - **Effective (E)**：内核在做权限检查时实际判定的权能生效位。",
            "  - **Inheritable (I)**：通过 execve 系统调用执行新程序时可以继承的权能位。",
            "  - **Bounding (B)**：权能边界集，限制进程及其子进程能获得的权能上限（只能收紧，不可扩张）。",
            "  - **Ambient (A)**：Linux 4.3 引入，允许非 root 用户启动的子进程无缝继承已授予的特权，是现代非特权容器的关键支撑。",
        ],

        Consequences =
        [
            "• `getcap`、`getpcaps`、读取 `/proc/<PID>/status` 都是只读操作。",
            "• `setcap` 给程序文件赋予权能（保存在文件的 `security.capability` 扩展属性里），等于部分放权给它。**绝不要给 bash、python 等解释器赋予权能**，否则任何脚本都能借它获得该特权。",
            "• `setcap -r <文件>` 可以随时移除文件权能。SteamOS 上系统文件的权能会在系统更新时恢复为镜像中的状态，不要去改系统程序。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前 shell 的五组权能位图",
                "grep ^Cap /proc/$$/status",
                "`CapInh` 可继承、`CapPrm` 允许、`CapEff` 生效、`CapBnd` 边界、`CapAmb` 环境。").Then("输出五行，如 `CapInh: 0000000000800000`、`CapEff: 0000000000000000`，每行一个 16 进制位图。"),
            Step.Read(
                "把位图解码成权能名称",
                "capsh --decode=$(awk '/^CapBnd/{print $2}' /proc/$$/status)",
                "边界集 `CapBnd` 表示「上限」而非「已拥有」。把 `CapBnd` 换成 `CapEff` 可以看实际生效的权能（普通用户为空）。").Then("输出 `0x000001ffffffffff=cap_chown,cap_dac_override,...` 一长串权能名。"),
            Step.Read(
                "列出系统中带文件权能的程序",
                "getcap -r /usr/bin 2>/dev/null",
                "`newuidmap`/`newgidmap` 让 Podman、Distrobox 无需 root 运行容器；`gamescope`、`kwin_wayland` 借 `cap_sys_nice` 提升调度优先级。").Then("每行「程序路径 权能=标志」，如 `/usr/bin/gamescope cap_sys_nice=eip`、`/usr/bin/newuidmap cap_setuid=ep`。"),
            Step.Read(
                "对比：还剩哪些程序依赖 SUID",
                "find /usr/bin -perm -4000 -type f 2>/dev/null",
                "它们需要切换身份，无法用单个权能替代。").Then("每行一个程序路径，如 `/usr/bin/passwd`、`/usr/bin/sudo`、`/usr/bin/su`。"),
            Step.Root(
                "实验：给一个临时副本授予、查看、再移除权能",
                "cp /usr/bin/true /tmp/cap-demo && sudo setcap cap_net_bind_service=+ep /tmp/cap-demo && getcap /tmp/cap-demo && sudo setcap -r /tmp/cap-demo && getcap /tmp/cap-demo; rm -f /tmp/cap-demo",
                "操作的是 `/tmp` 下的副本，不影响任何系统文件。").Then("输入 sudo 密码后，第一次 `getcap` 输出 `/tmp/cap-demo cap_net_bind_service=ep`；移除后第二次 `getcap` 没有输出；最后删除副本。"),
        ],

        Verify =
        [
            "1. 普通用户执行 `grep CapEff /proc/$$/status`，结果为 `0000000000000000`：你的 shell 没有任何生效的特权。",
            "2. `getcap /usr/bin/gamescope` 输出 `cap_sys_nice=eip`，印证 Gamescope 以普通用户运行、只额外拥有调整调度优先级的能力。",
        ],
    };
}
