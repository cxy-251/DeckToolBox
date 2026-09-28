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
        Overview = "传统 Unix 只区分「root」和「普通用户」。Linux 把 root 的特权拆成几十种权能 (capability)，程序只需获得它确实需要的那一项。SteamOS 上的 Gamescope、Podman 等程序正是这样在不使用 root 的前提下获得必要权限的。",

        Why =
        [
            "1. 传统模型的问题：",
            "• 权限检查只看有效用户 ID 是否为 0。程序要么什么都不能做，要么什么都能做。",
            "• 过去普通程序需要执行特权操作时（例如 `ping` 需要原始套接字、Web 服务器需要绑定 80 端口），只能把程序设为 SUID-root (`chmod u+s`)。程序一旦有漏洞，攻击者就能直接获得 root 权限。",
            "2. 权能：从 Linux 2.2 开始，内核把 root 的特权拆分成 40 多种独立的权能 (Capabilities)，常见的有：",
            "- `CAP_NET_BIND_SERVICE`：绑定 1024 以下的端口。",
            "- `CAP_NET_RAW`：使用原始套接字（抓包工具需要；现代发行版的 `ping` 改用无需特权的 ICMP 数据报套接字，已经不需要它）。",
            "- `CAP_SYS_NICE`：提高进程优先级、使用实时调度。SteamOS 上的 `gamescope` 和 `kwin_wayland` 通过文件权能获得它，用来提升自己的调度优先级，减少画面卡顿，而无需以 root 身份运行。",
            "- `CAP_SYS_PTRACE`：调试、追踪其他进程。",
            "- `CAP_SYS_ADMIN`：挂载文件系统等大量管理操作，范围很广，接近完整的 root。",
            "3. 进程的五组权能：",
            "- `Permitted`：进程可以使用的上限；`Effective`：当前实际生效的。",
            "- `Inheritable`、`Ambient`：执行新程序时如何传递给子进程。",
            "- `Bounding`：整个进程树能获得的权能上限，只能缩小，不能扩大。",
            "4. 文件权能：用 `setcap` 把权能附加到程序文件上（存在扩展属性 `security.capability` 中），任何人运行该程序时自动获得这些权能。",
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
            "2. `getcap /usr/bin/gamescope` 输出 `cap_sys_nice=eip`：Gamescope 以普通用户运行，只额外拥有调整调度优先级的能力。",
        ],
    };
}
