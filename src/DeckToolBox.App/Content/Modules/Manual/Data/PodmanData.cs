using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Data;

public static class PodmanData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-podman",
        Title = "podman：管理容器与镜像空间",
        Keywords = "podman ps -a · images · system df · rm · rmi · prune",
        Level = ModuleLevel.Intermediate,
        Overview = "Distrobox 在底层使用 podman。容器用多了，镜像和已停止的容器会占用不少空间，而 distrobox list 看不到这些细节。podman 命令可以查看每个容器、镜像实际占多少空间，并清理不再需要的部分。它与 Docker 的命令基本相同，无需 root。",

        Why =
        [
            "1. 三类对象：",
            "- 镜像 (image)：只读的系统模板，如 `archlinux:latest`，多个容器可以共用同一个镜像。",
            "- 容器 (container)：基于镜像创建的实例，你在里面安装的软件、做的修改都保存在容器的可写层中。Distrobox 创建的每个环境就是一个容器。",
            "- 卷 (volume)：独立于容器保存数据的存储。",
            "2. 存储位置：无 root 运行时在 `~/.local/share/containers/storage/`，占用家目录空间。",
            "3. 常用命令：",
            "- `podman ps -a`：所有容器（`-a` 包括已停止的）；`--format` 自定义显示列。",
            "- `podman images`：本地镜像及大小。",
            "- `podman system df`：镜像、容器、卷各占多少空间，以及可以回收多少 (`RECLAIMABLE`)。",
            "- `podman rm 容器`：删除容器；`podman rmi 镜像`：删除镜像（仍有容器在用时会拒绝）。",
            "- `podman image prune`：删除没有标签、没有被使用的悬空镜像；`podman system prune`：一次清理已停止的容器、悬空镜像和未使用的网络。",
            "4. 对 Distrobox 创建的容器，推荐用 `distrobox rm` 删除，它会一并处理导出的菜单项提示等细节。",
        ],

        Consequences =
        [
            "• 查看类命令只读。",
            "• **删除容器会删除你在其中安装的全部软件和修改**（家目录中的文件不受影响，因为 Distrobox 容器直接使用你的家目录）。",
            "• `podman system prune` 会删除所有已停止的容器，其中可能包括你暂时不用但还想保留的 Distrobox 环境；执行前先用 `podman ps -a` 确认。",
        ],

        Steps =
        [
            Step.Read(
                "列出所有容器（包括已停止的）",
                "podman ps -a --size --format '{{.Names}}\\t{{.Image}}\\t{{.Status}}\\t{{.Size}}'",
                "`--format` 选择要显示的列；显示 `.Size` 必须同时加 `--size`，需要计算，稍慢。").Then("每个容器一行：名称、所用镜像、状态（`Up ...` 运行中或 `Exited ... ago` 已停止）以及占用空间，形如 `1.74GB (virtual 2.15GB)`：前一个数是容器自身的可写层（你在里面安装的内容），`virtual` 还包括它所用的镜像。"),
            Step.Read(
                "列出本地镜像",
                "podman images",
                "一个镜像可以被多个容器共用。").Then("表格列出镜像仓库名、标签、ID、创建时间和大小，如 `docker.io/library/archlinux  latest  ...  408 MB`。"),
            Step.Read(
                "查看空间占用与可回收空间",
                "podman system df",
                "判断清理能省下多少空间。").Then("三行：`Images`、`Containers`、`Local Volumes`，每行有总数、活动数、大小和 `RECLAIMABLE`（可回收）；已停止容器的空间会全部计入可回收。"),
            Step.Read(
                "查看容器占用存储的目录大小",
                "du -sh ~/.local/share/containers/storage 2>/dev/null",
                "容器数据实际保存的位置。").Then("输出一个总大小，如 `2.2G`，这部分空间计入家目录分区。"),
            Step.Danger(
                "删除一个不再需要的容器",
                "podman rm <容器名>",
                "Distrobox 创建的容器建议用 `distrobox rm <容器名>`。运行中的容器需先 `podman stop`。").Then("输出被删除的容器名；`podman system df` 中容器占用随之减少。"),
            Step.Danger(
                "清理悬空镜像",
                "podman image prune",
                "只删除没有标签且没有被任何容器使用的镜像，会先询问确认。").Then("先显示 `WARNING! This command removes all dangling images.` 并询问 `Are you sure you want to continue? [y/N]`，确认后列出被删除的镜像 ID；没有可清理的镜像时不输出任何内容。"),
        ],

        Verify =
        [
            "1. 清理后再次执行 `podman system df`，`RECLAIMABLE` 列减少。",
        ],
    };
}
