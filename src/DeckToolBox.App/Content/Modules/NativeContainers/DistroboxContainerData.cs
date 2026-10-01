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
                "列出名称、状态和所用镜像。").Then("输出一张表：`ID | NAME | STATUS | IMAGE`。还没有容器时只显示表头。"),
            Step.Change(
                "创建一个 Arch Linux 容器",
                "distrobox create --name arch -i docker.io/library/archlinux:latest",
                "Arch 与 SteamOS 同源，装出的软件行为最接近本机。").Then("先下载镜像（显示下载进度，几百 MB，需要一两分钟），最后提示 `Distrobox 'arch' successfully created` 以及进入它的命令。此时容器已建好但尚未启动。"),
            Step.Change(
                "进入容器",
                "distrobox enter arch",
                "之后再进入只需一两秒。").Then("第一次进入会打印一串 `Setting up ...` 初始化步骤（约一分钟），之后提示符变为 `(deck@arch ~)$`。你的家目录内容照常可见；输入 `exit` 回到 SteamOS。"),
            Step.Root(
                "在容器里安装软件（示例：编译工具与 git）",
                "sudo pacman -Syu --needed base-devel git",
                "这里的 `sudo` 只作用于容器内部，不需要密码，也不会影响 SteamOS。").On(Hosts.Distrobox).Then("pacman 先同步软件源，列出将要安装的包和总大小，询问 `:: 进行安装吗？ [Y/n]`，回车确认后下载并安装。完成后容器里就能用 `git`、`gcc`。"),
            Step.Change(
                "把容器里的命令导出给宿主",
                "distrobox-export --bin /usr/bin/git --export-path ~/.local/bin",
                "在容器内执行。`--bin` 将容器内二进制封装为宿主包装脚本，宿主执行时自动拉起容器并在内部运行。").On(Hosts.Distrobox).Then("提示导出成功，并在宿主的 `~/.local/bin/git` 生成一个小脚本。回到 SteamOS 终端输入 `git`，会自动进入容器运行它（第一次稍慢）。"),
            Step.Change(
                "把容器里的图形程序导出到应用菜单",
                "distrobox-export --app <程序名>",
                "在容器内执行。`<程序名>` 填写容器内安装的软件命令名或 `.desktop` 名称。取消导出加 `--delete`。").On(Hosts.Distrobox).Then("提示导出成功。KDE 应用菜单中出现该程序（名称后带容器名），点击即可在容器里启动它。"),
            Step.Change(
                "停止容器",
                "distrobox stop arch",
                "下次 `enter` 会自动启动。").Then("询问是否确认停止，输入 `y` 后容器停止，`distrobox list` 中状态变为 `Exited`，释放它占用的内存。"),
            Step.Danger(
                "删除容器",
                "distrobox rm arch",
                "导出的命令和菜单项需手动删除。").Then("询问是否确认删除，输入 `y` 后容器及其中安装的软件被删除，`distrobox list` 中不再有它。家目录中的文件不受影响。"),
        ],

        Verify =
        [
            "1. `distrobox list` 中容器状态为 `Up`（运行中）或 `Exited`（已停止）。",
            "2. 在宿主终端执行 `command -v git`，输出 `~/.local/bin/git`，运行 `git --version` 能得到容器里的版本。",
            "3. 系统更新重启后，`distrobox enter arch` 仍然可以进入，里面装的软件都在。",
        ],
    };
}
