using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Dev;

public static class LddElfData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-ldd-elf",
        Title = "ldd 与 readelf：程序缺少库时",
        Keywords = "ldd · readelf · ldconfig -p · nm · NEEDED · RUNPATH · not found",
        Level = ModuleLevel.Intermediate,
        Overview = "下载的便携程序启动时报 `error while loading shared libraries: libxxx.so.1: cannot open shared object file`，说明它依赖的某个共享库找不到。`ldd` 列出程序需要哪些库、各自从哪里加载、哪个缺失；`readelf` 不运行程序，直接读出它声明的依赖和自带的库搜索路径；`ldconfig -p` 查系统里有没有某个库。",

        Why =
        [
            "1. 动态链接：Linux 程序（ELF 格式）大多不把库打包进自身，启动时由动态链接器 `/lib64/ld-linux-x86-64.so.2` 找到并加载所需的 `.so` 文件。任何一个找不到，链接器就报上面的错误并退出，程序本身一行代码都没执行。",
            "2. 链接器按这个顺序找库：",
            "- 程序内嵌的 `RPATH`（旧式写法，只在没有 `RUNPATH` 时生效）。",
            "- 环境变量 `LD_LIBRARY_PATH` 中列出的目录。",
            "- 程序内嵌的 `RUNPATH`。便携程序常写成 `$ORIGIN/lib`，`$ORIGIN` 代表程序自己所在的目录，这样整个文件夹放到哪里都能运行。",
            "- 系统缓存 `/etc/ld.so.cache`（`ldconfig -p` 可查看），以及默认目录 `/usr/lib`。",
            "3. 为什么在 SteamOS 上更常见：SteamOS 只预装了 Steam 和桌面需要的库，许多为 Ubuntu 编译的程序依赖的库在这里没有；而只读系统又不适合用 pacman 补装。常见的解决办法是换用自带全部依赖的 AppImage，或在 Distrobox 容器中运行（容器里可以随意安装库）。",
            "4. 工具分工：",
            "- `ldd`：让动态链接器模拟一次加载，列出每个库解析到的路径，缺失的标为 `not found`。它会借助链接器处理目标文件，**不要对来历不明的程序使用**，只看依赖时改用 `readelf`。",
            "- `readelf -d`：只读取文件中的动态段，不执行任何东西。`NEEDED` 是直接依赖的库名，`RUNPATH`/`RPATH` 是内嵌的搜索路径。",
            "- `nm -D`：列出库导出的符号。`T` 表示库里实现了这个函数，`U` 表示它需要别的库提供。报错 `undefined symbol` 时用它确认库的版本对不对。",
        ],

        Consequences =
        [
            "• `ldd`、`readelf`、`nm`、`ldconfig -p` 都只读取文件，不改动系统。",
            "• 在命令前临时写 `LD_LIBRARY_PATH=...` 只影响这一次运行的程序；不要把它写进 `~/.bashrc`，全局设置容易让其他程序加载到错误版本的库。",
        ],

        Steps =
        [
            Step.Read(
                "列出程序依赖的库及加载位置",
                "ldd /usr/bin/git",
                "每行一个库：`库名 => 实际路径 (加载地址)`。").Then("约十行，如 `libpcre2-8.so.0 => /usr/lib/libpcre2-8.so.0 (0x...)`、`libc.so.6 => /usr/lib/libc.so.6`；第一行 `linux-vdso.so.1` 是内核提供的虚拟库，最后一行是链接器 `/lib64/ld-linux-x86-64.so.2`。"),
            Step.Read(
                "只看缺失的库",
                "ldd <程序路径> | grep 'not found'",
                "`<程序路径>` 换成启动报错的程序，例如 `~/Applications/<目录>/<可执行文件>`。").Then("有缺失时每个缺失的库一行，如 `libfuse.so.2 => not found`；没有输出说明依赖齐全，问题在别处。"),
            Step.Read(
                "不运行程序，读出它直接依赖的库",
                "readelf -d /usr/bin/git | grep NEEDED",
                "`NEEDED` 只列直接依赖；`ldd` 会把依赖的依赖也展开。").Then("几行 `0x0000000000000001 (NEEDED)  共享库：[libpcre2-8.so.0]`，依次是 `libz-ng.so.2`、`libc.so.6` 等。"),
            Step.Read(
                "查看便携程序内嵌的库搜索路径（示例）",
                "readelf -d ~/Applications/platform-tools/adb | grep -E 'RPATH|RUNPATH'",
                "这里以 Android platform-tools 中的 `adb` 为例，换成你自己的程序即可。没有内嵌路径时没有输出。").Then("一行 `(RUNPATH)  Library runpath: [$ORIGIN/:$ORIGIN/lib64:$ORIGIN/../lib64:...]`：程序会先在自己所在目录和旁边的 `lib64` 目录里找库。"),
            Step.Read(
                "查系统里有没有某个库",
                "ldconfig -p | grep <库名关键词>",
                "`<库名关键词>` 取报错信息里的库名主体，如 `libfuse`。`ldconfig -p` 列出链接器缓存中的全部库。").Then("找到时每个匹配一行，如 `libpcre2-8.so.0 (libc6,x86-64) => /usr/lib/libpcre2-8.so.0`（`libc6` 不带 `x86-64` 的是 `/usr/lib32` 下的 32 位版本）；没有输出说明系统里没有这个库。"),
            Step.Read(
                "确认库里有没有某个函数",
                "nm -D /usr/lib/libc.so.6 | grep -w malloc",
                "`-D` 读动态符号表，`-w` 整词匹配。`@@` 后面是符号版本。").Then("输出 `00000000000a6610 T malloc@@GLIBC_2.2.5`：`T` 表示 libc 实现并导出了 `malloc`。"),
            Step.Change(
                "临时让程序到指定目录找库",
                "LD_LIBRARY_PATH=<库目录> <程序路径>",
                "`<库目录>` 是放着所缺 `.so` 文件的目录（例如程序自带但没被找到的 `lib` 子目录）。只对这一次运行生效。").Then("程序正常启动，说明缺的正是那个目录中的库；关闭后一切恢复原状。仍然报错时，错误信息会指出下一个缺失的库。"),
        ],

        Verify =
        [
            "1. `readelf -h /usr/bin/ls | grep -E '类别|系统架构'` 输出 `ELF64` 与 `Advanced Micro Devices X86-64`（英文环境下为 `Class` 与 `Machine`）。",
            "2. `ldd /usr/bin/ls | grep -c 'not found'` 输出 `0`。",
        ],
    };
}
