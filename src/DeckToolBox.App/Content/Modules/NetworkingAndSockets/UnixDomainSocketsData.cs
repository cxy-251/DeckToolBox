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
        Overview = "本机进程之间通信最常用的方式是 UNIX 域套接字：Wayland、PipeWire、D-Bus 都通过它连接。它表现为文件系统中的一个特殊文件，可以用普通文件权限控制谁能连接，还能在进程之间传递打开的文件描述符。",

        Why =
        [
            "1. 绕过网络协议栈：",
            "• 本机两个进程也可以通过 `127.0.0.1` 的 TCP 通信，但数据要经过完整的 TCP/IP 处理（校验、序号、确认、拥塞控制）。",
            "• UNIX 域套接字 (AF_UNIX) 只在内核内部把数据从一端复制到另一端，没有协议开销，延迟更低、吞吐更高。",
            "2. 以文件的形式存在：",
            "• 套接字在文件系统中显示为类型 `s` 的文件，如 `/run/user/1000/wayland-0`、`/run/user/1000/pipewire-0`、`/run/dbus/system_bus_socket`。",
            "• 能否连接由这个文件的权限决定；它不监听网络端口，局域网中的其他设备无法访问。",
            "3. 传递文件描述符 (SCM_RIGHTS)：一个进程可以把它打开的文件、共享内存或显存缓冲区的描述符发送给另一个进程，对方获得访问同一对象的能力。",
            "• Wayland 图形与 PipeWire 音视频正是这样工作的：客户端把显存缓冲区 (DMA-BUF) 或共享内存 (memfd) 的文件描述符通过套接字传给合成器，双方直接访问同一块内存。**真正的「零拷贝」来自传递描述符**，而不是套接字本身：通过套接字发送的普通数据仍会在内核中复制一次。",
        ],

        Consequences =
        [
            "• 查看套接字的命令只读。",
            "• 服务异常退出后可能留下套接字文件，重新启动时报 `Address already in use`；确认服务已完全停止后删除该文件即可。",
            "• 大部分套接字位于 `/run`（内存文件系统），重启后自动清空。",
        ],

        Steps =
        [
            Step.Read(
                "列出 UNIX 域套接字",
                "ss -x -a | head -n 30",
                "`-x` 只看 UNIX 域套接字，`-a` 包括监听中的。").Then("表格前 30 行：类型（`u_str` 流、`u_dgr` 数据报）、状态、队列、本地路径与 inode、对端 inode。"),
            Step.Read(
                "查看桌面会话使用的套接字文件",
                "ls -la /run/user/$(id -u)/ | grep -E \"(wayland|pipewire|bus|pulse)\"",
                "`/run/user/1000` 是你的运行时目录，重启后清空。").Then("列出 `bus`、`pipewire-0`、`pulse`、`wayland-0` 等条目，权限首字母为 `s`（套接字）或 `d`（目录）。"),
            Step.Read(
                "查看哪些程序连接着 Wayland 合成器",
                "ss -x -p | grep -m 20 wayland",
                "`grep -m 20` 最多显示 20 行。").Then("每行一个连接到 Wayland 合成器的连接，行尾 `users:((\"程序名\",pid=...))` 指出是哪个程序（只显示你自己的进程）。"),
            Step.Read(
                "查看套接字的收发队列",
                "ss -x -i | grep -B 1 -A 2 -E 'wayland-0|system_bus_socket' | sed -n '1,25p'",
                "`-i` 显示内部信息。").Then("相关套接字的连接行及其附加信息；`Recv-Q`、`Send-Q` 两列长期不为 0 表示有数据积压，对方可能没有及时读取。"),
        ],

        Verify =
        [
            "1. `ls -l /run/user/$(id -u)/wayland-0` 第一个字符为 `s`，大小为 0。",
            "2. `ss -x -p | grep -c wayland` 大于 0，说明有程序正通过套接字连接 Wayland 合成器。",
        ],
    };
}
