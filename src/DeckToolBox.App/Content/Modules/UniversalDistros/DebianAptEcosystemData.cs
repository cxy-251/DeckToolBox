using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class DebianAptEcosystemData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "debian-apt",
        Title = "Debian / Ubuntu：APT",
        Keywords = "apt · dpkg · 软件源",
        Level = ModuleLevel.Intermediate,
        Overview = "全面剖析装机量最大的 Debian / Ubuntu 软件包生态，解析 .deb 归档解剖学、dpkg 状态机、apt 的 SAT 依赖求解器、APT Pinning 权重控制与现代 PPA 源签名链。",

        Why =
        [
            "1. `.deb` 二进制包的物理本质 (Unix ar 归档)：",
            "• 一个 `.deb` 包在底层不是什么专有黑盒，它就是一个标准的 Unix `ar` 归档文件，解开后内部永远由固定三部分构成：",
            "  - `debian-binary`：纯文本声明，固定为格式版本号 '2.0'。",
            "  - `control.tar.*`：包的灵魂！包含 Package 元数据声明、依赖版本条件 (Depends/Recommends)、文件 MD5 校验和以及维护者执行脚本 (preinst/postinst/prerm/postrm)。",
            "  - `data.tar.*`：真正的二进制有效载荷。里面的目录结构与 Linux 根目录完全一一对应，解压即是安装！",
            "2. `dpkg` (底层执行器) 与 `apt` (高级依赖解决器) 的清晰分工：",
            "• `dpkg` 仅负责单机本地解包、校验与执行安装脚本，它**完全没有网络拉取功能，更不会自动帮你递归下载依赖包**！若依赖缺失，dpkg 会直接报错并将包挂起在未配置状态。",
            "• `apt` (Advanced Package Tool) 则是顶层指挥官：它定期同步镜像源的 `Packages` 索引，在内存中构建依赖有向无环图 (DAG)，通过 SAT 求解器计算出全局无冲突的最优安装链路，按拓扑排序依次调用 dpkg 落地执行。",
            "3. 现代软件源与 PPA 的安全签名链演进：",
            "• 过去粗暴的 `apt-key add` 会将密钥存入全局信任库，任何第三方源都能伪造系统核心库。现代 Debian/Ubuntu 强推 GPG 密钥存放在 `/etc/apt/keyrings/`，并在源定义中使用 `Signed-By` 精准限定签名信任范围。",
            "4. 细粒度版本权重控制：APT Pinning (`/etc/apt/preferences`)：",
            "• 当混合使用 Stable/Testing 源时，通过为特定源设置 Pin-Priority 优先级（如 1001 强制降级，500 默认优先，-1 彻底禁用），实现随心所欲的版本锁定。",
        ],

        Consequences =
        [
            "• 警惕盲目引入未受信任的 PPA：部分老旧 PPA 可能包含与系统底层 libc 不兼容的依赖，覆盖后可能引发著名的“依赖地狱 (Dependency Hell)”。",
            "• 依赖半损坏时的自愈神技：当安装被意外打断导致 dpkg 锁死时，执行 `sudo apt --fix-broken install` (或 `apt -f install`)，apt 会自动尝试修补并收割所有未闭环的依赖关系。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前系统所有已安装软件包清单与状态 (dpkg -l)",
                "dpkg -l | head -n 30",
                "查看本地 dpkg 数据库中的包清单与状态，ii 代表期望安装且配置成功。").On(Hosts.Debian),
            Step.Read(
                "查询指定软件包安装到系统中的全部物理文件路径清单",
                "dpkg -L <包名>",
                "查看指定软件包向文件系统释放的所有二进制文件、配置文件与帮助手册路径。").On(Hosts.Debian),
            Step.Read(
                "反向查询硬盘上的某个文件是由哪个软件包释放的 (dpkg -S)",
                "dpkg -S /usr/bin/curl",
                "反向精准查询硬盘上的某个特定命令或库是由哪个软件包维护提供的。").On(Hosts.Debian),
            Step.Read(
                "查询指定软件包在所有已配置源中的候选版本与优先级 (APT Policy)",
                "apt-cache policy <包名>",
                "排查版本决策依据，洞悉 APT Pinning 优先级与仓库版本候选列表。").On(Hosts.Debian),
            Step.Read(
                "解剖并查看一个 .deb 文件内部的 control 控制信息与文件列表",
                "dpkg-deb -I package.deb ; dpkg-deb -c package.deb",
                "解剖 .deb 归档，分别查看 control 控制脚本与 data 物理数据目录树。").On(Hosts.Debian),
            Step.Root(
                "现代标准：安全添加第三方软件源的标杆语法",
                "echo '标准范式: echo \"deb [signed-by=/etc/apt/keyrings/app.gpg] https://repo.example.com stable main\" | sudo tee /etc/apt/sources.list.d/app.list'",
                "现代规范：使用 signed-by 指定专属密钥路径，限定 GPG 信任范围，杜绝全局密钥污染。"),
        ],

        Verify =
        [
            "1. 包状态健全度：执行 `dpkg -s <包名>`，输出中 `Status: install ok installed` 是唯一代表健康完成的状态；若为 `half-installed` 或 `unpacked` 则表明配置脚本未执行完，需执行 `dpkg --configure -a`。",
            "2. 缓存纯净度核验：长期运行的 Debian 系统可通过 `sudo apt clean` 清理 `/var/cache/apt/archives/` 下累积的历史 `.deb` 缓存，常可腾出数 GB 磁盘。",
        ],
    };
}
