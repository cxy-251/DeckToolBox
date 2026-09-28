using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.MemoryAndCgroups;

public static class CgroupsResourceControlData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "cgroups",
        Title = "cgroups v2 资源控制",
        Keywords = "cgroup · systemd-run · MemoryMax · CPUQuota",
        Level = ModuleLevel.Advanced,
        Overview = "cgroups 让内核按「进程组」分配和限制 CPU、内存、IO。systemd 把每个服务、每个应用都放进各自的 cgroup；你也可以用 systemd-run 给任意一条命令加上内存和 CPU 上限，让后台任务不拖慢前台游戏。",

        Why =
        [
            "1. cgroup v2 是一棵统一的层级树，挂载在 `/sys/fs/cgroup`。每个目录是一个组，组里的进程共同受该组的资源限制约束。",
            "2. 常用控制接口（每个组目录下的文件）：",
            "- `memory.max`：内存硬上限。组内进程总内存超出时，内核只在这个组内回收或杀进程，不波及系统其他部分。",
            "- `cpu.max`：CPU 配额，例如 `50000 100000` 表示每 100 ms 最多使用 50 ms，即半个核心。",
            "- `io.max` / `io.weight`：磁盘读写的上限与权重。",
            "- `pids.max`：组内进程数上限，可防止 fork 炸弹。",
            "3. systemd 的组织方式：",
            "- `system.slice`：系统服务。",
            "- `user.slice/user-1000.slice/user@1000.service/`：你的用户会话，其下的 `app.slice` 放着从桌面启动的各个应用。",
            "• 所以在 systemd 系统上，通常不直接写 `/sys/fs/cgroup`，而是通过 `systemctl set-property` 或 `systemd-run` 让 systemd 代为管理。",
            "4. `systemd-run --user --scope -p MemoryMax=… -p CPUQuota=… <命令>`：为这条命令临时创建一个 scope 单元，命令结束后自动清理。",
        ],

        Consequences =
        [
            "• 查看 cgroup 信息只读无害。",
            "• 用 `systemd-run --user` 施加的限制只作用于那一条命令，普通用户即可操作，结束后不留痕迹。",
            "• 内存上限设得过低时，该命令会被内核 OOM 终止（退出码 137），系统其他进程不受影响。",
            "• 修改系统服务的资源属性（`sudo systemctl set-property`）默认是永久的，会写入 `/etc/systemd/system.control/`；加 `--runtime` 则重启后失效。",
        ],

        Steps =
        [
            Step.Read(
                "查看内核启用的 cgroup 控制器",
                "cat /sys/fs/cgroup/cgroup.controllers",
                "通常包含 `cpuset cpu io memory hugetlb pids`。"),
            Step.Read(
                "查看当前 shell 所在的 cgroup",
                "cat /proc/$$/cgroup",
                "v2 下只有一行，形如 `0::/user.slice/.../app.slice/...`。"),
            Step.Read(
                "以树形查看你的会话中各应用的 cgroup",
                "systemd-cgls --no-pager /user.slice/user-$(id -u).slice/user@$(id -u).service/app.slice | head -n 40",
                "每个桌面应用、每个用户服务都有自己的节点。"),
            Step.Read(
                "按资源占用实时排序各个 cgroup",
                "systemd-cgtop",
                "类似 top，但按 cgroup 统计。按 `m` 按内存排序，`q` 退出。"),
            Step.Change(
                "限制一条命令最多使用 1 GB 内存和半个 CPU 核心",
                "systemd-run --user --scope -p MemoryMax=1G -p CPUQuota=50% <命令>",
                "例如压缩大文件、编译代码时使用，前台游戏不会被抢占资源。"),
            Step.Read(
                "查看某个 scope 的实际限制值",
                "systemctl --user show -p MemoryMax,CPUQuotaPerSecUSec $(systemctl --user list-units --type=scope --no-legend | awk '/run-/{print $1; exit}')",
                "在上一步的命令运行期间，从另一个终端执行。"),
        ],

        Verify =
        [
            "1. 执行 `systemd-run --user --scope -p MemoryMax=200M sh -c 'cat /proc/self/cgroup'`，输出路径以 `run-...scope` 结尾，说明命令运行在独立 cgroup 中。",
            "2. 执行 `systemd-run --user --scope -p CPUQuota=25% sh -c 'while :; do :; done'`，在 `htop` 中能看到该进程的 CPU 占用稳定在 25% 左右（`Ctrl+C` 结束）。",
        ],
    };
}
