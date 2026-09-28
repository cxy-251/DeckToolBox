using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NetworkingAndSockets;

public static class UnixDomainSocketsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "unix-sockets",
        Title = "UNIX 域套接字",
        Keywords = "AF_UNIX · SCM_RIGHTS · ss -x",
        Level = ModuleLevel.Advanced,
        Overview = "深入 Linux 本地高效进程通信底石 —— UNIX 域套接字，掌握其与网络 TCP 协议栈的本质差异、文件描述符传递 (SCM_RIGHTS) 与图形音频渲染通道。",

        Why =
        [
            "1. 绕过网络协议栈：",
            "• 很多初学者误用 127.0.0.1 环回 TCP 进行本机通信。TCP 需要经过封包、解包、TCP 状态机、滑动窗口与校验和计算，多次在内核态与用户态间复制内存，延迟极高。",
            "• UNIX 域套接字 (AF_UNIX / AF_LOCAL) 完全抛弃网络协议栈，数据在内核内存缓冲区中直接直通对端进程，延迟直接压低一个数量级，吞吐量比 TCP 环回快数倍！",
            "2. 挂载于文件系统的原生权限安全：",
            "• 在文件系统中表现为类型为 's' 的特殊文件（如 /run/user/1000/wayland-0、/run/user/1000/pipewire-0、/run/dbus/system_bus_socket）。",
            "• 只要利用 Linux 标准的 chmod/chown 权限，就能天然拦截未经授权的未受信任程序偷窥总线，天然免疫网络端口扫描风险。",
            "3. 杀手级内核特性：SCM_RIGHTS 传递文件描述符：",
            "• 普通 IPC 只能传数据，而 UNIX 域套接字允许一个进程将其打开的文件描述符 (FD) 直接原样“赠送”给另一个进程！",
            "• Wayland 图形与 PipeWire 音视频正是这样工作的：客户端把显存缓冲区 (DMA-BUF) 或共享内存 (memfd) 的文件描述符通过套接字传给合成器，双方直接访问同一块内存。**真正的「零拷贝」来自传递描述符**，而不是套接字本身：通过套接字发送的普通数据仍会在内核中复制一次。",
        ],

        Consequences =
        [
            "• 遗留套接字导致服务启动失败：如果守护进程被 kill -9 强杀，可能会在 /tmp 或 /run 遗留下无主孤儿 .sock 句柄文件。下次启动时由于文件已存在会报错 Address already in use，手动 rm 即可解决。",
            "• 掌握 /run 的临时性：套接字绝大部分存放在 /run (基于 tmpfs 内存文件系统) 下，设备重启自动清空重置，绝无历史脏数据遗留。",
        ],

        Steps =
        [
            Step.Read(
                "查看系统当前全部活跃的 UNIX 域套接字监听与连接清单",
                "ss -x -a | head -n 30",
                "使用比 netstat 更快更现代的 ss 工具，查看各服务的 socket 路径与对端 inode 节点。"),
            Step.Read(
                "查看当前用户桌面运行环境建立的核心套接字文件",
                "ls -la /run/user/$(id -u)/ | grep -E \"(wayland|pipewire|bus|pulse)\"",
                "定位 Wayland 显示服务、PipeWire 音频服务与 D-Bus 的物理 socket 节点。"),
            Step.Read(
                "排查指定服务（如 Wayland）当前正在通信的全部客户端进程",
                "ss -x -p | grep -m 20 wayland",
                "查看哪些应用窗口正连接在 Wayland 合成器通道上。"),
            Step.Read(
                "查看套接字的内核缓冲区深度与收发队列积压",
                "ss -x -i | grep -B 1 -A 2 -E 'wayland-0|system_bus_socket' | sed -n '1,25p'",
                "检查内核 socket 队列是否有堵塞，诊断进程卡死现象。"),
        ],

        Verify =
        [
            "执行 ls -la /run/user/$(id -u)/wayland-0，第一列属性首字母必为 's'（代表 socket），并且大小为 0 字节（它只充当内核接入点，不占物理磁盘），证实 UNIX 域套接字健康运作。",
        ],
    };
}
