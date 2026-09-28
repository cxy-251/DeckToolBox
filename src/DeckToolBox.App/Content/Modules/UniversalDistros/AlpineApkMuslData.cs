using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class AlpineApkMuslData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "alpine_apk_musl",
        Title = "极简与容器专精体系：Alpine Linux、apk 与 musl libc 极简哲学 (apk / musl / BusyBox)",
        Subtitle = "广域 Linux 发行版宇宙",
        CategoryName = "极简与容器发行版",
        Badge = "容器霸主",
        Overview = "深入解析云原生与 Docker 容器时代的绝对霸主 Alpine Linux，剖析仅 5MB 的极限体积成因、musl libc 与 glibc 的底层差异、BusyBox 极简工具链与 apk 零缓存标准。",

        WhyCard = new InfoCardItem(
            "📌",
            "云原生时代的瘦身奇迹：Alpine Linux 与 musl 架构精髓",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 为什么全球超过 80% 的生产级微服务容器都基于 Alpine？",
                "• 一个常规的 Ubuntu 基础容器镜像体积通常在 80MB~120MB，启动时占用可观内存且包含大量无用的文档与历史工具。",
                "• **Alpine 官方基础镜像仅有区区 5MB 左右！** 秒级完成千台服务器的镜像分发拉取，攻击面被压缩到极限，天然规避绝大多数通用 Linux CVE 漏洞。",

                "2. 极限瘦身的三大底层支柱：",
                "• **musl libc 替代 GNU glibc**：抛弃膨胀的 glibc，采用极致轻量且严格遵循 POSIX 标准的 `musl` 运行时库，静态开销降至极限。",
                "• **BusyBox 替代庞大的 GNU Coreutils**：单个 `busybox` 多合一二进制文件模拟了 `ls`、`grep`、`awk`、`sed` 等上百个常用命令，节约海量 Inode 与磁盘块。",
                "• **OpenRC 替代复杂的 Systemd**：放弃庞大的 systemd 全家桶，采用基于 C 语言与轻量级 Shell 脚本的 OpenRC 依赖引擎，0 内存浪费。",

                "3. 容器运维黄金范式：`apk add --no-cache`：",
                "• 在编写 Dockerfile 时，传统包管理需要先 update、再 install、最后手动执行 rm -rf 清除安装包缓存以防镜像层臃肿。",
                "• `apk` 原生支持 `--no-cache` 标志：边流式下载解包边瞬时销毁临时归档，不留任何磁盘垃圾，是全球容器镜像构建的标准动作。"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "musl libc 的兼容性陷阱与排查 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 二进制 ABI 动态链接不兼容：**凡是在 Ubuntu/CentOS 上基于 glibc 编译的非静态二进制程序，直接拷到 Alpine 上运行会直接报错 `file not found`！**（根本原因是找不到 `/lib64/ld-linux-x86-64.so.2` 动态链接器）。",
                "• 应急解决策略：如必须运行 glibc 闭源专有程序，需在 Alpine 中安装 `gcompat` 兼容层库 (`apk add --no-cache gcompat`)。"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看 Alpine 系统已安装软件包列表与各自磁盘占用",
                "echo 'Alpine 指令: apk info -vv | sort'",
                "查看 Alpine 本地数据库中的包列表、版本号与单行功能描述。"
            ),
            new(
                2,
                "执行标准的零缓存软件安装命令（Dockerfile 标杆写法）",
                "echo 'Alpine 指令: apk add --no-cache curl jq'",
                "容器镜像标准规范：流式下载解包并瞬时销毁临时归档，零磁盘垃圾残留。"
            ),
            new(
                3,
                "查看可执行文件所绑定的动态链接库 (验证 musl libc 归属)",
                "echo 'Alpine 指令: ldd /bin/busybox'",
                "验证动态链接器为 ld-musl-x86_64.so.1，证明其轻量 musl libc 底座。"
            ),
            new(
                4,
                "反向查询硬盘上的文件由哪一个 Alpine 包提供",
                "echo 'Alpine 指令: apk info -W /usr/bin/curl'",
                "秒级反向精准定位提供该文件的源包名与归属。"
            ),
            new(
                5,
                "当遭遇闭源商业软件闪退时，安装 glibc ABI 兼容转换层",
                "echo 'Alpine 指令: apk add --no-cache gcompat'",
                "安装 gcompat 转换层，使 Alpine 容器能够原生跑起针对标准 Linux 编译的动态库。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "健康度核验与体积评估 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. 体积核查：在 Alpine 容器中执行 `du -sh /`，若总系统体积控制在 15MB~30MB 之间，证明镜像维持在极高纯净度。",
                "2. 状态验证：执行 `apk audit`，检测全盘是否存在被非官方脚本非法覆写的系统关键文件。"
            }
        )
    };
}
