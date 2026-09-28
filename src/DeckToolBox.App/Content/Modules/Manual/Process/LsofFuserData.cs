using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Process;

public static class LsofFuserData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-lsof-fuser",
        Title = "lsof 与 fuser：谁在占用",
        Keywords = "lsof · fuser -vm · target is busy · 端口占用",
        Level = ModuleLevel.Intermediate,
        Overview = "卸载 SD 卡时提示 target is busy、删除文件后空间没有释放、端口被占用导致程序启动失败——这些问题都要先找出是哪个进程在使用它。lsof 列出进程打开的文件，fuser 反过来查某个文件或挂载点被谁占用。",

        Why =
        [
            "1. 在 Linux 中，打开的普通文件、目录、设备、网络连接、管道都是「文件」，每个进程持有的都能被列出来。",
            "2. lsof（list open files）：",
            "- `lsof 文件` / `lsof +D 目录`：谁打开了该文件 / 目录下的文件。",
            "- `lsof -p PID`：某个进程打开的所有文件；`-c 名称`：按进程名。",
            "- `lsof -i :端口`、`lsof -i TCP`：网络连接。",
            "- `lsof +L1`：已被删除但仍被打开的文件（它们仍占用磁盘空间，直到进程关闭它们）。",
            "- `-w`：屏蔽警告。SteamOS 上有 Podman 容器时，lsof 会对容器的挂载点输出一堆 `can't stat()` 警告，加 `-w` 即可。",
            "- 输出列：`FD` 为文件描述符及打开方式（`r` 读、`w` 写、`u` 读写；`cwd` 当前目录、`txt` 程序本身、`mem` 映射到内存的库）。",
            "3. fuser：",
            "- `fuser -v 文件`：列出使用该文件的进程。",
            "- `fuser -vm 挂载点`：列出使用该文件系统上任何文件的进程，排查卸载失败时最有用。访问方式中 `c` 表示当前目录在那里，`f` 表示打开了文件。",
            "4. 普通用户只能看到自己进程的详细信息；root 进程（如 sshd）占用的端口需要 sudo 才能看到。",
        ],

        Consequences =
        [
            "• lsof、fuser 查询是只读的。",
            "• `fuser -k` 会结束所有占用者（默认 SIGKILL），可能结束你不想结束的程序，尽量先手动关闭它们。",
        ],

        Steps =
        [
            Step.Change(
                "制造一个被占用的文件",
                "sleep 120 > /tmp/busy.txt & sleep 0.3; lsof -w /tmp/busy.txt",
                "后台 sleep 的输出重定向到文件，于是它一直打开着这个文件。").Then("一行结果：`sleep 12345 deck 1w REG ... /tmp/busy.txt`：sleep 以写方式 (`1w`，文件描述符 1 即标准输出) 打开了它。"),
            Step.Read(
                "用 fuser 反查",
                "fuser -v /tmp/busy.txt",
                "`-v` 显示详细信息。").Then("输出表头 `用户 进程号 权限 命令` 与一行 `deck 12345 F.... sleep`。"),
            Step.Read(
                "排查 SD 卡为什么无法卸载",
                "fuser -vm /run/media/deck/* 2>&1",
                "`-m` 表示该挂载点所在的整个文件系统。").Then("每个挂载点下列出正在使用它的进程，如 `deck 811936 f.... zed-editor`（打开了其中的文件）或 `c`（当前目录在里面）。关闭这些程序后即可正常卸载。"),
            Step.Read(
                "查看一个进程打开的文件",
                "lsof -w -p $(pgrep -n steam) | head -n 12",
                "`-p` 指定 PID。").Then("列出 steam 的当前目录 (`cwd`)、程序本身 (`txt`)、加载的库 (`mem`) 以及打开的日志文件、套接字等。"),
            Step.Read(
                "查看自己进程的网络连接",
                "lsof -w -i -n -P | head -n 10",
                "`-n` 不解析主机名、`-P` 不把端口号转成服务名，速度更快。").Then("列出你的进程建立的 TCP/UDP 连接，如 `steam ... TCP 192.168.0.21:xxxxx->xx.xx.xx.xx:443 (ESTABLISHED)`。"),
            Step.Read(
                "找出已删除但仍占用空间的文件",
                "lsof -w +L1 2>/dev/null | head",
                "`+L1` 表示链接数小于 1，即已经没有文件名的文件。").Then("列出这类文件及持有它们的进程。以 `/memfd:` 开头、标注 `(deleted)` 的是程序在内存中创建的匿名文件，不占磁盘，可以忽略；需要关注的是路径位于 `/home` 等真实磁盘上的条目，关闭对应进程后空间才会释放。"),
            Step.Change(
                "清理练习进程",
                "kill %1 2>/dev/null; rm -f /tmp/busy.txt",
                "结束第一步启动的后台 sleep。").Then("没有输出，占用消失，文件被删除。"),
        ],

        Verify =
        [
            "1. 清理后再执行 `fuser -v /tmp/busy.txt`，提示文件不存在或没有输出。",
        ],
    };
}
