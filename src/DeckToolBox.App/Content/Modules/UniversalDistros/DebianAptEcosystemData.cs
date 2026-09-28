using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class DebianAptEcosystemData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "debian-apt",
        Title = "Debian / Ubuntu：APT",
        Keywords = "apt · dpkg · 软件源 · signed-by",
        Level = ModuleLevel.Intermediate,
        Overview = "Debian 及其衍生版（Ubuntu、Linux Mint、Pop!_OS 等）是装机量最大的发行版家族。核心是两层工具：底层的 dpkg 安装单个 .deb，上层的 apt 负责下载和解决依赖。",

        Why =
        [
            "1. `.deb` 包的结构：本质是一个 `ar` 归档，里面三个成员：",
            "- `debian-binary`：格式版本号 `2.0`。",
            "- `control.tar.*`：元数据（包名、版本、`Depends` 依赖）和维护脚本（`preinst`/`postinst`/`prerm`/`postrm`）。",
            "- `data.tar.*`：要安装的文件，目录结构与根目录一一对应。",
            "2. dpkg 与 apt 的分工：",
            "• `dpkg` 只在本地安装、删除、查询单个包，不联网，也不会自动安装依赖；依赖缺失时安装会停在「未配置」状态。",
            "• `apt` 从软件源下载索引（`apt update`），计算出满足全部依赖的安装方案，再按顺序调用 dpkg。",
            "3. 软件源与签名：",
            "• 源定义在 `/etc/apt/sources.list.d/`。新版本（Debian 12、Ubuntu 24.04 起）默认使用 deb822 格式的 `.sources` 文件。",
            "• 旧教程里的 `apt-key add` 已废弃：它把第三方密钥放进全局信任，任何源都能用它签名。现在的做法是把密钥放在 `/etc/apt/keyrings/`，并在源定义里用 `Signed-By` 指定只有这个源使用它。",
            "4. 版本优先级 (APT Pinning)：`/etc/apt/preferences.d/` 中可以为某个源或包设定优先级，用于锁定版本或混用 stable / testing 源。",
        ],

        Consequences =
        [
            "• `apt search/show/policy`、`dpkg -l/-L/-S` 都是只读查询。",
            "• 添加第三方源（PPA 或厂商源）等于信任它能替换你系统里的任何包，只添加确有需要、来源可信的源。",
            "• `apt autoremove` 会删除「自动安装且不再被依赖」的包，执行前看清列表。",
            "• 安装被打断后若出现 dpkg 锁或半安装状态，先 `sudo dpkg --configure -a`，再 `sudo apt -f install`。",
            "• 在 Distrobox 容器里练习时，这些操作只影响容器，不会碰到 SteamOS。",
        ],

        Steps =
        [
            Step.Change(
                "在 Deck 上创建一个 Ubuntu 练习容器并进入",
                "distrobox create --name ubuntu -i docker.io/library/ubuntu:24.04 && distrobox enter ubuntu",
                "之后的步骤都在这个容器里执行。练习完可用 `distrobox rm ubuntu` 删除。"),
            Step.Root(
                "刷新软件源索引",
                "sudo apt update",
                "安装或升级之前先执行。").On(Hosts.Debian),
            Step.Read(
                "搜索并查看软件包信息",
                "apt search <关键词>; apt show <包名>",
                "`apt show` 会列出版本、依赖、大小和说明。").On(Hosts.Debian),
            Step.Root(
                "安装软件包",
                "sudo apt install <包名>",
                "会列出将要额外安装的依赖，确认后输入 Y。").On(Hosts.Debian),
            Step.Read(
                "查询：包里有哪些文件 / 某个文件属于哪个包",
                "dpkg -L <包名>; dpkg -S $(command -v <命令名>)",
                "排查「这个命令从哪来」时非常有用。").On(Hosts.Debian),
            Step.Read(
                "查看候选版本与来自哪个源",
                "apt policy <包名>",
                "显示已安装版本、候选版本以及各个源的优先级。").On(Hosts.Debian),
            Step.Root(
                "升级整个系统",
                "sudo apt update && sudo apt full-upgrade",
                "`full-upgrade` 允许为了升级而增删依赖；日常更新用它即可。").On(Hosts.Debian),
            Step.Root(
                "清理不再需要的依赖与下载缓存",
                "sudo apt autoremove --purge && sudo apt clean",
                "`--purge` 同时删除这些包的系统级配置文件。").On(Hosts.Debian),
            Step.Root(
                "按现行规范添加第三方软件源（模板）",
                "sudo install -d -m 0755 /etc/apt/keyrings\ncurl -fsSL <密钥 URL> | sudo gpg --dearmor -o /etc/apt/keyrings/<名称>.gpg\nprintf 'Types: deb\\nURIs: <仓库 URL>\\nSuites: <发行代号>\\nComponents: main\\nSigned-By: /etc/apt/keyrings/<名称>.gpg\\n' | sudo tee /etc/apt/sources.list.d/<名称>.sources\nsudo apt update",
                "按软件厂商文档替换占位符。密钥只对这一个源生效。").On(Hosts.Debian),
        ],

        Verify =
        [
            "1. `dpkg -s <包名> | grep Status` 输出 `install ok installed` 表示安装完整；`half-installed`、`unpacked` 表示需要执行 `sudo dpkg --configure -a`。",
            "2. `apt policy <包名>` 中 `Installed` 与 `Candidate` 相同，表示已是可用的最新版本。",
        ],
    };
}
