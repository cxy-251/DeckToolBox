# 🧰 DeckToolBox

一本可以动手操作的 Linux 使用手册，运行在 Steam Deck（SteamOS 3.x）上，同样适用于其他 Linux 发行版。

每个知识模块都分成四段：

| 段落 | 内容 |
| --- | --- |
| 📌 原理 | 为什么是这样，底层机制是什么 |
| ⚠️ 后果与边界 | 操作会改变什么、能否撤销、有哪些坑 |
| 💻 操作步骤 | 可复制的真实命令，标注风险等级与执行位置 |
| 🔍 验证 | 如何确认操作生效 |

应用**从不替你执行命令**：点击「复制」，粘贴到 Konsole，看清楚再回车。

## 内容结构

| 专区 | 面向 | 模块 |
| --- | --- | --- |
| 🧭 入门必修 | 任何 Linux 都通用的日常技能与系统自带命令 | 终端、文件与管道、Shell 配置、nano/vim、tmux、sudo、外接存储、压缩与 rsync 备份、硬盘健康与固件、网络排查、SSH、防火墙、蓝牙、音频、电源休眠、KDE 命令行、文本处理、jq、ffmpeg、校验与加密、系统信息、进程、排障流程、systemd 定时器 |
| 🎮 Steam Deck 本机 | 只适用于 SteamOS 的机制与命令 | 桌面模式、只读系统与 A/B 更新、steamosctl（终端里的快捷菜单）、steamos-* 命令集、便携应用、Distrobox、不用 Flatpak 的清理与替代、存储清理、传感器与功耗、Gamescope、手柄输入链、Wine/Proton、Proton 前缀、启动选项、非 Steam 游戏、Steam Runtime |
| 🐧 其他发行版 | 各发行版家族的包管理 | 命令对照表、Debian/Ubuntu、Fedora/RHEL、Arch、openSUSE、Alpine、Nix 与原子化发行版、Flatpak/Snap/AppImage 对比 |
| 🧠 系统原理 | 所有发行版通用的底层机制 | 进程与信号、systemd、D-Bus、/proc、/sys、udev、挂载与 OverlayFS、文件系统、内存与 OOM、cgroups、网络栈、UNIX 套接字、权限与 ACL、Capabilities、显示栈、XDG、PipeWire、内核模块、性能追踪 |

「其他发行版」的每个模块第一步都会用 Deck 自带的 Distrobox 创建对应发行版的容器，所以在 Deck 上就能亲手练习 apt / dnf / zypper / apk。

### 指令标签

- 🟢 **只读**：只查询，不改变任何状态
- 🔵 **修改用户数据**：改动家目录、用户服务或 Steam 设置，可撤销
- 🟡 **需要 sudo**：需要管理员权限（Deck 上需先用 `passwd` 设置密码）
- 🔴 **高危**：删除数据或难以撤销
- 🟣 **执行位置**：不在本机终端执行的指令（Distrobox 容器内、Steam 启动选项、其他发行版等）
- **含 &lt;占位符&gt;**：复制后需要先把 `<AppID>` 之类替换成实际值

## 运行

```bash
./run.sh
```

需要 .NET 10 SDK（脚本会把 `~/Applications/dotnet` 加入 PATH）。`Ctrl+F` 可跨全部专区搜索模块和命令。

## 维护

技术栈：C# / .NET 10 / Avalonia 11.3。所有内容都以 C# 代码的形式写在 `src/DeckToolBox.App/Content/` 中，由框架统一渲染。

```
src/DeckToolBox.App/
├── Framework/
│   ├── Models/              KnowledgeScope、CategoryDefinition、ModuleDefinition、CommandStep（含 Risk 与 Host）
│   ├── ViewModels/          主窗口、导航、搜索、模块渲染
│   ├── Views/               MainWindow、ModuleContentView
│   ├── Controls/RichText.cs 文本中的 **加粗** 与 `代码` 渲染
│   └── ContentValidator.cs  内容静态检查
└── Content/
    ├── ContentRegistry.cs   专区 → 分类 → 模块 目录
    └── Modules/<分组>/*.cs  每个模块一个文件
```

新增模块：复制一个现有模块文件，修改内容，然后在 `ContentRegistry.cs` 对应分类里加一行 `XxxData.Create()`。步骤用 `Step.Read / Change / Root / Danger(标题, 命令, 说明)` 声明，不在本机终端执行的加 `.On(Hosts.Xxx)`。

提交前运行两个检查：

```bash
./run.sh --check              # 静态检查：sudo/rm 的风险标记、echo 伪指令、标记配对、四段是否齐全
./run.sh --commands <模块Id>  # 按转义后的真实内容打印指令，便于在终端逐条核对
```

写内容的原则：

1. 指令必须在真实系统上验证过；SteamOS 与通用 Linux 的差异要写明（例如 SteamOS 没有 man 手册、没有 cron、`dmesg` 受限）。
2. 不写营销腔和无法验证的数据；性能、比例类说法要有依据或写成范围。
3. 风险标记宁高勿低；会删除用户数据的操作必须写清后果与备份方法。
4. 只讲系统原生能力，不涉及安装 Python、Node.js 等开发运行时。
