using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class RedHatRpmDnfData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "redhat_rpm_dnf",
        Title = "Red Hat / Fedora 体系：RPM 打包规范与 DNF libsolv 极速求解 (rpm / dnf / AppStream)",
        Subtitle = "广域 Linux 发行版宇宙",
        CategoryName = "Red Hat / Fedora 体系",
        Badge = "企业标杆",
        Overview = "深入解析企业级 Linux 霸主 Red Hat (RHEL / Fedora / Rocky) 体系，剖析 .rpm 结构、SPEC 打包脚本全景、从 YUM 到 DNF 的 C 语言 libsolv SAT 求解革命与事务级原子回滚。",

        WhyCard = new InfoCardItem(
            "📌",
            "企业级 Linux 的稳定性基石：RPM 格式与 DNF 现代依赖算法",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. `.rpm` 二进制包的内部结构 (Header + cpio 归档)：",
                "• 一个标准的 RPM 包由四部分构成：Lead 引导头、GPG 数字签名区、Header 元数据区以及最终的 Payload 有效载荷（采用 cpio 归档，并使用 zstd/xz 深度压缩）。",
                "• RPM 的元数据直接支持丰富的语义标签，包括 `Provides` (声明提供的虚包能力)、`Requires` (硬依赖)、`Conflicts` (互斥) 与触发器脚本 (Triggers)。",

                "2. 性能革命：从臃肿的 Python 版 YUM 到 C 语言 DNF (libsolv)：",
                "• 早期 CentOS 6/7 上的 YUM 采用纯 Python 编写，在仓库庞大时依赖图求解缓慢无比，内存暴涨数十倍甚至引发 OOM 卡死。",
                "• 现代 Fedora 与 RHEL 8/9 统一采用 **DNF** (Dandified YUM)：依赖解析核心完全换成了 openSUSE 贡献的高性能 C 语言库 `libsolv`。通过布尔可满足性 (SAT) 算法，数万个包的依赖拓扑可以在毫秒级极速解算完毕！",

                "3. 企业级多版本共存利器：AppStream 模块流 (Module Streams)：",
                "• 企业用户往往希望系统底层内核与 glibc 维持十年的超长稳定支持 (LTS)，但又需要灵活升级上层开发环境（如按需在 Node.js 18、20 或 PostgreSQL 14、16 间无缝切换）。",
                "• AppStream 模块流允许通过 `dnf module enable <name>:<stream>`，在不破坏系统基础依赖树的前提下随时平滑切换软件主版本！",

                "4. 杀手级企业容灾机制：事务数据库与一键回滚 (`dnf history undo`)：",
                "• DNF 将每一次软件安装、卸载、更新都记录在底层的 SQLite 事务数据库中。若某次系统补丁升级后导致业务崩溃，只需一条命令即可将整个事务完整撤销回滚到升级前的精确状态！"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "企业源冲突与清理运维边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 严禁随意混杂不兼容的第三方源：在 RHEL/CentOS 上同时启用 EPEL、RPM Fusion 与 Remi 源时需配置优先权 (Priority)，否则可能发生基础核心库被低质构建覆盖的灾难。",
                "• 事务回滚的局限性：`dnf history undo` 能精准回滚包文件的增删变动，但如果某个软件在 postinst 阶段执行了外部数据库迁移脚本，这类外部状态需人工备份兜底。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看全量系统历史运维事务记录与操作编号 (dnf history)",
                "echo 'RHEL/Fedora 指令: dnf history'",
                "查看 DNF 历史事务编号、涉及包数量、执行时间与结果。"
            ),
            new(
                2,
                "原子级撤销指定历史事务并完整回滚系统状态 (Undo Transaction)",
                "echo 'RHEL/Fedora 指令: sudo dnf history undo <事务ID>'",
                "原子级撤销指定历史事务，系统自动降级或卸载对应包，秒级自愈。"
            ),
            new(
                3,
                "使用 rpm 查询系统当前已安装的所有包清单 (rpm -qa)",
                "echo 'RHEL/Fedora 指令: rpm -qa | sort | head -n 30'",
                "直接从本地 Berkeley DB 检索全部已装 RPM 包，速度极快。"
            ),
            new(
                4,
                "反查硬盘上任意系统文件属于哪一个 RPM 软件包提供",
                "echo 'RHEL/Fedora 指令: rpm -qf /usr/bin/dnf'",
                "反查硬盘上任意二进制文件属于哪一个 RPM 包维护提供。"
            ),
            new(
                5,
                "校验系统关键二进制文件是否被外部恶意篡改 (rpm -V 完整性核查)",
                "echo 'RHEL/Fedora 指令: rpm -V coreutils'",
                "比对文件的 MD5/SHA256、权限、大小与时间戳，核查核心文件完整性。"
            ),
            new(
                6,
                "查看可用的 AppStream 模块流与多版本运行环境切换选项",
                "echo 'RHEL/Fedora 指令: dnf module list'",
                "一览系统可供独立启用的多版本语言与数据库运行时清单。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "完整性校验与判定输出指标 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 文件篡改核验判定：执行 `rpm -V <包名>` 时，若输出为空，代表该包释放的所有二进制文件与官方出厂状态 100% 绝对一致；若出现 `S.5....T.`，则代表 Size、MD5 与 mtime 已被修改，是排查木马或系统损坏的法宝。",
                "2. 缓存清理命令：执行 `sudo dnf clean all` 可一键安全清空 `/var/cache/dnf/` 下的所有仓库元数据与临时 RPM 归档。"
            }
        )
    };
}
