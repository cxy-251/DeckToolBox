using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class NixGuixDeclarativeData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "nix_guix_declarative",
        Title = "函数式与不可变软件分发：Nix 纯函数隔离、声明式系统与 rpm-ostree (Nix / Flakes / Silverblue)",
        Subtitle = "广域 Linux 发行版宇宙",
        CategoryName = "函数式与不可变发行版",
        Badge = "未来范式",
        Overview = "深入探索代表操作系统未来形态的“声明式与纯函数软件分发”范式，剖析 Nix 的 /nix/store 内容寻址模型、多版本绝对隔离无冲突、系统瞬时原子回滚与 Fedora Silverblue 的 rpm-ostree 联合分层。",

        WhyCard = new InfoCardItem(
            "📌",
            "彻底终结依赖地狱：纯函数式包管理与不可变系统的颠覆式创新",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 传统 Linux 软件包的阿喀琉斯之踵：“依赖地狱”：",
                "• 传统发行版将所有软件的动态库粗暴散落在 `/usr/lib/` 下。一旦软件 A 强制依赖 `libssl.so.1.1`，而软件 B 要求 `libssl.so.3.0`，系统就会陷入无法两全的死锁崩溃状态。",

                "2. Nix 纯函数式包管理的终极解法 (`/nix/store/<哈希>-<包名>`)：",
                "• Nix 视编译构建为一个纯数学函数：**输入 (源代码 + 编译器版本 + 依赖哈希) => 严格确定性的唯一下输出**。",
                "• 每一个包都独占存放在 `/nix/store/<32位加密哈希>-<包名>-<版本>/` 目录下。你的机器上可以同时并行安装 10 个互不干扰的 Python 解释器、不同版本的 GCC 与 OpenSSL，不同软件各取所需，**依赖冲突在数学意义上被永久消灭！**",

                "3. NixOS 声明式操作系统 (Infrastructure as Code)：",
                "• 整台操作系统的所有配置（内核模块、开机自启服务、桌面主题、用户权限）全部声明在唯一的配置文件 (`configuration.nix`) 中。",
                "• 执行 `nixos-rebuild switch` 会原子级生成一个全新的系统代际 (Generation)。开机菜单列出历史全部代际，配置改崩随时秒级倒流回上一个完好版本！",

                "4. 红帽企业级不可变系统代表：Fedora Silverblue (`rpm-ostree`)：",
                "• 采用类似 Git 的 **OSTree** 机制管理根文件系统。系统以只读挂载运行，用户使用 `rpm-ostree install` 安装包时，系统是在后台由底层树状提交 (Tree Commit) 自动组合出一个新只读分支，重启后无缝切入，兼顾传统 RPM 兼容性与绝对系统免疫力。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "磁盘垃圾回收与非 FHS 路径边界 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 磁盘存储按需回收：由于 Nix 升级从不就地覆盖旧文件，多代际历史会导致 `/nix/store` 体积变大，需定期执行 `nix-collect-garbage -d` 回收无主垃圾。",
                "• 抛弃传统 FHS 规范：Nix 系统没有 `/bin/bash` 或 `/usr/lib`（只保留 `/bin/sh`），直接运行外部写死硬编码绝对路径的二进制可能会报错找不到解释器，需通过 `patchelf` 工具修复动态链接器路径。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看 Nix Store 内容寻址目录结构与哈希隔离形态",
                "echo 'Nix 指令: ls -ld /nix/store/* | head -n 25'",
                "观察 /nix/store 下每个包独占的 32 位唯一加密哈希路径，彻底消灭依赖冲突。"
            ),
            new(
                2,
                "体验秒级免安装临时开发沙箱环境 (nix-shell 魔法)",
                "echo 'Nix 指令: nix-shell -p python311 nodejs_20'",
                "临时进入具备指定语言环境的子 shell，退出后全局系统一尘不染，无任何依赖残留。"
            ),
            new(
                3,
                "查看当前用户环境已部署的软件代际历史清单 (Generations)",
                "echo 'Nix 指令: nix-env --list-generations'",
                "查看系统历史快照时间线与当前激活指针，掌握原子回退依据。"
            ),
            new(
                4,
                "安全执行垃圾回收并彻底擦除所有未被引用的历史包块",
                "echo 'Nix 指令: nix-collect-garbage -d'",
                "彻底擦除所有未被当前代际引用的历史废弃包块，安全释放磁盘空间。"
            ),
            new(
                5,
                "查看 Fedora Silverblue 不可变系统的 ostree 部署状态与分层包",
                "echo 'Silverblue 指令: rpm-ostree status'",
                "在 Fedora Silverblue 上查看当前活跃只读提交与分层安装的 RPM 列表。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "环境隔离度与回滚验证 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 隔离有效性验证：在 nix-shell 内部执行 `which python3`，路径必指向 `/nix/store/.../bin/python3`；退出 shell 后再次执行，全局环境完全未被污染，证实函数式隔离机制生效。",
                "2. 回滚验证：执行 `nix-env --rollback`，系统符号链接瞬间切换至上一代版本，证实零停机秒级回退能力。"
            }
        )
    };
}
