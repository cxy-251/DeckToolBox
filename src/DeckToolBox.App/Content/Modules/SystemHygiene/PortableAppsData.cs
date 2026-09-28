using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SystemHygiene;

public static class PortableAppsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "portable-apps",
        Title = "便携应用与 AppImage",
        Keywords = "~/Applications · ~/.local/bin · AppImage",
        Level = ModuleLevel.Basic,
        Overview = "不经过任何包管理器，把软件放在自己的目录里：AppImage 单文件、解压即用的压缩包、单个可执行文件。放在 /home 下，系统更新永远不会动它们。",

        Why =
        [
            "1. 为什么便携应用在 SteamOS 上好用：",
            "• 它们只是 `/home` 里的普通文件，不写入只读的系统分区，不依赖包数据库，系统更新后照常可用。",
            "• 卸载就是删除它的文件或目录，没有注册表，也不会有散落各处的残留（配置目录除外，通常在 `~/.config/<程序名>`）。",
            "2. 两个约定俗成的位置：",
            "- `~/Applications/`：图形程序、AppImage、解压出来的程序目录，每个软件一个子目录。KDE 的 AppImage 集成也默认识别这里。",
            "- `~/.local/bin/`：命令行工具的单个可执行文件。它通常已在 `PATH` 中（见「环境变量与 Shell 配置」），放进去就能在终端直接输入命令名运行。",
            "3. AppImage 的原理：",
            "• AppImage 是一个自带运行库的只读镜像（SquashFS）。运行时通过 FUSE 把镜像挂载到 `/tmp` 下的临时目录，再启动里面的程序；关闭程序后自动卸载。",
            "• SteamOS 同时带有 `libfuse.so.2` 和 `libfuse3`，常见的 AppImage 可以直接运行。",
        ],

        Consequences =
        [
            "• 下载的文件默认没有执行权限，需要 `chmod +x` 一次。",
            "• 便携应用**不会自动更新**，需要自己关注新版本；也没有沙箱，能读写你的整个 home 目录，只运行来源可信的程序。",
            "• AppImage 不会自动出现在应用菜单里，需要写一个 `.desktop` 文件（见「XDG 目录与 .desktop 菜单」），或添加进 Steam 在游戏模式中使用。",
            "• 删除程序目录不会删除它的配置，彻底清理时记得检查 `~/.config`、`~/.local/share` 下同名目录。",
        ],

        Steps =
        [
            Step.Change(
                "创建便携应用目录",
                "mkdir -p ~/Applications ~/.local/bin",
                "`-p` 表示目录已存在时不报错，可重复执行。").Then("没有输出。家目录下出现 `Applications` 目录，`~/.local/bin` 也确保存在；目录已存在时什么也不改变。"),
            Step.Change(
                "给下载的 AppImage 加上执行权限",
                "chmod +x ~/Applications/<程序名>.AppImage",
                "`+x` 表示加上可执行权限。").Then("没有输出。`ls -l` 中该文件的权限从 `-rw-r--r--` 变为 `-rwxr-xr-x`，之后双击或在终端输入路径就能运行它。"),
            Step.Read(
                "查看 AppImage 自带的运行参数",
                "~/Applications/<程序名>.AppImage --appimage-help",
                "所有 type 2 AppImage 都支持 `--appimage-*` 系列参数。").Then("打印 `--appimage-extract`、`--appimage-mount`、`--appimage-version` 等参数的说明，不会启动程序本身。"),
            Step.Change(
                "解包 AppImage（FUSE 不可用或想查看内容时）",
                "cd ~/Applications && ./<程序名>.AppImage --appimage-extract",
                "解出到当前目录的 `squashfs-root/`。").Then("终端逐行打印解出的文件路径，完成后当前目录下出现 `squashfs-root/`，其中的 `AppRun` 就是启动入口。原 AppImage 文件不变。"),
            Step.Change(
                "安装单文件命令行工具",
                "install -m 755 <下载的文件> ~/.local/bin/<命令名>",
                "`install` 一步完成复制和设置权限。").Then("没有输出。文件被复制到 `~/.local/bin/<命令名>` 并设为可执行，之后在任何目录输入命令名即可运行。原文件不变。"),
            Step.Read(
                "列出已有的便携应用及其占用",
                "du -sh ~/Applications/* ~/.local/bin/* 2>/dev/null | sort -h",
                "`sort -h` 按人类可读的大小（K/M/G）排序。").Then("每个程序一行，左边是占用大小，按从小到大排列，最后一行是最大的。"),
        ],

        Verify =
        [
            "1. 执行权限：`ls -l ~/Applications` 中对应文件的权限以 `-rwx` 开头。",
            "2. 命令行工具：`command -v <命令名>` 输出 `/home/deck/.local/bin/<命令名>`，说明它已在 PATH 中生效。",
            "3. 更新不丢失：系统更新重启后，这些文件和它们的配置都原样保留。",
        ],
    };
}
