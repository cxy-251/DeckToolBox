using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class RedHatRpmDnfData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "fedora-dnf",
        Title = "Fedora / RHEL：DNF",
        Keywords = "dnf · rpm · history undo · rpm -V",
        Level = ModuleLevel.Intermediate,
        Overview = "Red Hat 家族（Fedora、RHEL、CentOS Stream、Rocky、AlmaLinux）使用 RPM 包格式与 DNF 包管理器。DNF 的事务历史可以撤销某次安装或升级，rpm -V 可以校验文件是否被改动。",

        Why =
        [
            "1. RPM 包：由签名、头部元数据（`Requires`、`Provides`、`Conflicts`、脚本）和 cpio 格式的文件载荷组成，载荷通常用 zstd 压缩。",
            "2. rpm 与 dnf 的分工与 Debian 类似：`rpm` 操作本地单个包和数据库，`dnf` 负责联网、依赖求解与事务。已安装包数据库在较新的版本中使用 SQLite（`/usr/lib/sysimage/rpm`）。",
            "3. DNF 的演进：",
            "• 早期的 YUM 由 Python 编写，依赖求解慢；DNF 改用 openSUSE 开发的 C 库 **libsolv** 做 SAT 求解，速度大幅提升。",
            "• Fedora 41 起默认使用 **DNF5**（C++ 重写），命令基本兼容，个别子命令输出格式有变化。",
            "• RHEL 8/9 的「模块流」(`dnf module`) 曾用于并存多个软件版本，已被 Red Hat 弃用，新版本不再依赖它。",
            "4. 事务历史：DNF 把每次安装、升级、删除记录为一个事务，可以查看详情，也可以用 `history undo` 反向执行。",
            "5. 常用附加源：EPEL（RHEL 上的额外软件）、RPM Fusion（Fedora 上因专利或许可不能进官方源的软件，如部分多媒体编解码器）。",
        ],

        Consequences =
        [
            "• `dnf search/info/history list`、`rpm -q*`、`rpm -V` 都是只读的。",
            "• `dnf history undo` 只回滚包的增删与版本，不会回滚软件运行后写入的数据或配置迁移；被撤销的旧版本包还必须能从源里下载到。",
            "• 混用多个第三方源时，它们可能提供同名的包互相替换，出现冲突时优先查看包来自哪个源（`dnf info` 的 `From repo` / `Repository` 字段）。",
        ],

        Steps =
        [
            Step.Change(
                "在 Deck 上创建一个 Fedora 练习容器并进入",
                "distrobox create --name fedora -i registry.fedoraproject.org/fedora:latest && distrobox enter fedora",
                "之后的步骤都在这个容器里执行。"),
            Step.Read(
                "搜索与查看软件包",
                "dnf search <关键词>; dnf info <包名>",
                "`dnf info` 显示版本、来源仓库与说明。").On(Hosts.Fedora),
            Step.Root(
                "安装软件包",
                "sudo dnf install <包名>",
                "会列出事务内容，确认后输入 y。").On(Hosts.Fedora),
            Step.Read(
                "查看事务历史",
                "dnf history list | head -n 15",
                "每一行是一个事务，第一列是事务 ID。`dnf history info <事务ID>` 查看详情。").On(Hosts.Fedora),
            Step.Danger(
                "撤销某次事务",
                "sudo dnf history undo <事务ID>",
                "反向执行该事务：它安装的包会被删除、升级的包会被降级。执行前先用 `history info` 看清内容。").On(Hosts.Fedora),
            Step.Read(
                "查询：某个文件属于哪个包 / 包里有哪些文件",
                "rpm -qf $(command -v <命令名>); rpm -ql <包名>",
                "`dnf provides <路径或命令>` 还能查询尚未安装的包。").On(Hosts.Fedora),
            Step.Read(
                "校验包的文件是否被改动",
                "rpm -V coreutils",
                "无输出表示所有文件与包中一致。输出如 `S.5....T.` 表示大小 (S)、摘要 (5)、修改时间 (T) 发生了变化。").On(Hosts.Fedora),
            Step.Root(
                "升级系统并清理缓存",
                "sudo dnf upgrade --refresh && sudo dnf clean all",
                "`--refresh` 强制刷新仓库元数据。").On(Hosts.Fedora),
        ],

        Verify =
        [
            "1. `rpm -q <包名>` 输出完整版本号，说明已安装。",
            "2. 执行 `history undo` 后，`dnf history list` 顶部会多出一条新事务，其动作与被撤销的事务相反。",
        ],
    };
}
