using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class AlpineApkMuslData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "alpine-apk",
        Title = "Alpine：apk 与 musl",
        Keywords = "apk · musl libc · BusyBox · OpenRC",
        Level = ModuleLevel.Intermediate,
        Overview = "Alpine Linux 以极小体积著称，基础容器镜像只有几 MB，因此在容器镜像中非常流行。它和主流发行版有三处根本差异：musl libc、BusyBox 和 OpenRC，这些差异决定了哪些程序能直接在上面运行。",

        Why =
        [
            "1. 为什么这么小：",
            "- **musl libc** 取代 GNU glibc：更小、更简单的 C 标准库实现。",
            "- **BusyBox** 取代 GNU coreutils 等工具：一个可执行文件实现 `ls`、`grep`、`sed`、`awk` 等上百个命令的精简版本。",
            "- **OpenRC** 取代 systemd 作为服务管理器（容器里通常用不到）。",
            "2. apk 包管理器：速度快、操作简单。`apk add --no-cache` 在安装时不把索引缓存留在镜像里，常用于编写 Dockerfile，保持镜像层最小。",
            "3. musl 与 glibc 的二进制不兼容：",
            "• 在 glibc 系统上编译的动态链接程序依赖 `/lib64/ld-linux-x86-64.so.2` 这个动态链接器，Alpine 上没有它。直接运行会报一个令人困惑的错误：`not found`，尽管文件明明存在。",
            "• `gcompat` 兼容层能让部分 glibc 程序运行，但不保证所有程序都行；更可靠的做法是使用为 musl 编译的版本或静态链接的程序。",
            "4. BusyBox 工具是精简版：部分 GNU 扩展参数不可用，脚本在 Alpine 上报错时，可以安装完整的 `coreutils`、`grep` 等包。",
        ],

        Consequences =
        [
            "• 查询命令只读。在 Distrobox 容器里的一切操作只影响该容器。",
            "• 从其他发行版复制过来的预编译二进制，大概率无法在 Alpine 上直接运行，这是 libc 不同导致的，不是权限问题。",
            "• 基础镜像小不等于更安全：安全性取决于及时更新和实际装了什么。",
        ],

        Steps =
        [
            Step.Change(
                "在 Deck 上创建一个 Alpine 练习容器并进入",
                "distrobox create --name alpine -i docker.io/library/alpine:latest && distrobox enter alpine",
                "之后的步骤都在这个容器里执行。"),
            Step.Root(
                "更新索引并安装软件包",
                "sudo apk update && sudo apk add <包名>",
                "Dockerfile 中常写作 `apk add --no-cache <包名>`，省去 `update` 且不留缓存。").On(Hosts.Alpine),
            Step.Read(
                "列出已安装的包",
                "apk info -v | sort | head -n 30",
                "`-v` 带上版本号。").On(Hosts.Alpine),
            Step.Read(
                "查询某个文件属于哪个包",
                "apk info --who-owns /bin/busybox",
                "很多命令其实是指向 busybox 的符号链接。").On(Hosts.Alpine),
            Step.Read(
                "确认程序链接的是 musl",
                "ldd /bin/busybox",
                "动态链接器显示为 `ld-musl-x86_64.so.1`。").On(Hosts.Alpine),
            Step.Read(
                "对比：在 Deck 主机上查看同类程序链接的是 glibc",
                "ldd /usr/bin/ls",
                "在 SteamOS 主机（容器外）执行，可以看到 `ld-linux-x86-64.so.2` 和 `libc.so.6`。"),
            Step.Root(
                "安装 glibc 兼容层",
                "sudo apk add gcompat",
                "只在必须运行为 glibc 编译的程序时使用。").On(Hosts.Alpine),
        ],

        Verify =
        [
            "1. `cat /etc/alpine-release` 显示 Alpine 版本号。",
            "2. `ls -l /bin/ls` 显示它是指向 `/bin/busybox` 的符号链接。",
            "3. `apk audit` 列出与包数据库不一致的文件（无输出表示没有改动）。",
        ],
    };
}
