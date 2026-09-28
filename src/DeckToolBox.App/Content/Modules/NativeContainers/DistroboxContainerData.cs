using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NativeContainers;

public static class DistroboxContainerData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "distrobox",
        Title = "Distrobox 用户容器",
        Keywords = "distrobox · podman · export",
        Level = ModuleLevel.Intermediate,
        Overview = "SteamOS 自带 Distrobox 和 Podman。用它创建一个 Arch、Ubuntu、Fedora 等发行版的容器，在里面随意用包管理器装命令行工具，不碰只读系统，系统更新也不会丢。",

        Why =
        [
            "1. 解决什么问题：",
            "• SteamOS 根分区只读，解锁后用 `pacman` 装的东西会在系统更新后消失（见「只读系统与 A/B 更新」）。",
            "• Distrobox 用 Podman 在你的家目录里运行一个完整发行版的用户空间，镜像和容器数据保存在 `~/.local/share/containers/`，属于 home 分区。",
            "2. 和普通 Docker 容器的区别：Distrobox 刻意让容器和宿主「融为一体」：",
            "- 容器里的家目录就是你真实的 `/home/deck`，文件直接可见。",
            "- 容器里的图形程序直接显示在 KDE 桌面上，音频走宿主的 PipeWire，GPU 可用。",
            "- 容器里的用户名、UID 与宿主一致，`sudo` 在容器内无需密码，只影响容器本身。",
            "3. `distrobox-export`：把容器里的程序「导出」给宿主使用：",
            "• `--bin` 在宿主的 `~/.local/bin` 生成一个小脚本，执行时自动进入容器运行该命令。",
            "• `--app` 在宿主的 `~/.local/share/applications` 生成 `.desktop` 文件，程序出现在 KDE 应用菜单中。想在游戏模式中使用，再手动把它添加为非 Steam 游戏。",
            "4. 也是学习其他发行版的最佳实验环境：本应用「其他发行版」专区的每个模块都可以在对应容器里亲手操作。",
        ],

        Consequences =
        [
            "• **容器不是安全沙箱**：它能读写你的整个家目录。不要在容器里运行不信任的程序；创建容器时加 `--home <目录>` 可以给它一个独立的家目录，减少对真实家目录的暴露。",
            "• 创建容器需要联网下载镜像，常见基础镜像几十到几百 MB；之后装的软件都计入 `~/.local/share/containers` 的占用。",
            "• 容器里装的东西只存在于该容器：删除容器会连同其中安装的软件一起删掉，但你家目录里的文件不受影响。",
            "• 容器内的配置文件（例如 `~/.bashrc`）和宿主共用，修改会互相影响。",
        ],

        Steps =
        [
            Step.Read(
                "查看已有的容器",
                "distrobox list",
                "列出名称、状态和所用镜像。"),
            Step.Change(
                "创建一个 Arch Linux 容器",
                "distrobox create --name arch -i docker.io/library/archlinux:latest",
                "Arch 与 SteamOS 同源，装出的软件行为最接近本机。首次创建需下载镜像。"),
            Step.Change(
                "进入容器",
                "distrobox enter arch",
                "第一次进入会做初始化，稍等片刻。提示符变化后就在容器里了，输入 `exit` 回到宿主。"),
            Step.Root(
                "在容器里安装软件（示例：编译工具与 git）",
                "sudo pacman -Syu --needed base-devel git",
                "这里的 `sudo` 只作用于容器内部，不会影响 SteamOS。").On(Hosts.Distrobox),
            Step.Change(
                "把容器里的命令导出给宿主",
                "distrobox-export --bin /usr/bin/git --export-path ~/.local/bin",
                "在容器内执行。之后在宿主终端输入 `git` 就会自动调用容器里的版本。").On(Hosts.Distrobox),
            Step.Change(
                "把容器里的图形程序导出到应用菜单",
                "distrobox-export --app <程序名>",
                "在容器内执行。取消导出用 `distrobox-export --app <程序名> --delete`。").On(Hosts.Distrobox),
            Step.Change(
                "停止容器",
                "distrobox stop arch",
                "不用时停止以释放内存，下次 `enter` 会自动启动。"),
            Step.Danger(
                "删除容器",
                "distrobox rm arch",
                "删除容器及其中安装的全部软件；家目录里的文件不受影响。导出的命令和菜单项需手动删除。"),
        ],

        Verify =
        [
            "1. `distrobox list` 中容器状态为 `Up`（运行中）或 `Exited`（已停止）。",
            "2. 在宿主终端执行 `command -v git`，输出 `~/.local/bin/git`，运行 `git --version` 能得到容器里的版本。",
            "3. 系统更新重启后，`distrobox enter arch` 仍然可以进入，里面装的软件都在。",
        ],
    };
}
