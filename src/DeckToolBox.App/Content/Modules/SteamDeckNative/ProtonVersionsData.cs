using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class ProtonVersionsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "proton-versions",
        Title = "Proton 版本与 GE-Proton",
        Keywords = "Proton Experimental · bleeding-edge · Hotfix · GE-Proton · compatibilitytools.d · CompatToolMapping",
        Level = ModuleLevel.Intermediate,
        Overview = "Steam 里同时存在多种 Proton：带版本号的正式版、Experimental、Hotfix，以及社区维护的 GE-Proton。本模块说明它们的区别、怎样查看每个游戏用的是哪个版本，以及不借助其他工具、用几条命令手动安装 GE-Proton。",

        Why =
        [
            "1. Valve 发布的版本（都在 `steamapps/common/` 下，作为「工具」由 Steam 下载）：",
            "- **正式版**（如 `Proton 11.0`、`Proton 10.0`）：经过较长测试，版本号内的更新以修复为主。旧版本（如 `5.13`、`7.0`）保留给只在旧版上正常的游戏。",
            "- **Proton Experimental**：持续合入新修复，新发售游戏的兼容问题通常先在这里解决。它的「测试版」页面中还有 `bleeding-edge` 分支，跟随开发进度更新，适合排查最新问题，也更可能出新问题。",
            "- **Proton Hotfix**：Valve 为个别游戏临时指定的版本，一般由 Steam 自动选用，不需要手动设置。",
            "2. 游戏用哪个版本：默认使用「设置 → 兼容性」中选的版本，或 Valve 为该游戏测试后指定的版本；在游戏「属性 → 兼容性」中勾选强制使用某个工具后，选择记录在 `~/.local/share/Steam/config/config.vdf` 的 `CompatToolMapping` 中（AppID `0` 表示全局默认）。",
            "3. 前缀记录的版本：每个游戏的前缀目录 `compatdata/<AppID>/version` 写着最后一次使用的 Proton 版本（如 `11.0-100`）。换到更新的版本时 Proton 会升级前缀；再换回旧版本，旧版本未必能正确处理升级过的前缀。",
            "4. GE-Proton：社区维护的版本，加入了一些 Valve 版本没有的修复与解码支持，部分游戏在它上面表现更好。Steam 启动时扫描 `~/.local/share/Steam/compatibilitytools.d/`，每个含有 `compatibilitytool.vdf` 的子目录都会作为一个兼容工具出现在列表中，所以只要把发布的压缩包解压到这里即可，不需要其他图形工具。",
        ],

        Consequences =
        [
            "• 查看版本与配置都是只读操作。",
            "• 每个 GE-Proton 解压后约 1 GB 多，放在 `/home`，不影响系统分区；不用时删除对应目录即可。",
            "• 切换版本可能升级游戏前缀。换回旧版本后游戏无法启动时，先备份存档（见「Wine 与 Proton 原理」），再删除前缀让 Steam 重建。",
            "• GE-Proton 不由 Valve 测试；遇到问题向游戏或 Valve 反馈前，先换回官方版本确认能否重现。",
        ],

        Steps =
        [
            Step.Read(
                "列出已安装的 Proton",
                "ls -d ~/.local/share/Steam/steamapps/common/Proton* ~/.local/share/Steam/compatibilitytools.d/*/ 2>/dev/null",
                "前一部分是 Valve 的版本，后一部分是手动安装的版本；`2>/dev/null` 隐藏「目录不存在」的报错。").Then("每行一个目录，如 `.../common/Proton 10.0`、`.../common/Proton 11.0`、`.../common/Proton - Experimental`、`.../common/Proton Hotfix`；装过 GE-Proton 时还有 `.../compatibilitytools.d/GE-Proton11-7-x86_64/`。"),
            Step.Read(
                "查看各 Proton 的具体构建",
                "for d in ~/.local/share/Steam/steamapps/common/Proton*/; do printf '%-28s %s\\n' \"$(basename \"$d\")\" \"$(cat \"$d/version\" 2>/dev/null || echo '（没有 version 文件：卸载后的残留目录）')\"; done",
                "每个 Proton 目录里的 `version` 文件记录构建时间戳与版本名。").Then("每行「目录名  时间戳 版本」，如 `Proton 11.0   1788504981 proton-11.0-2c-x86_64`、`Proton - Experimental   ... experimental-11.0-20260924-x86_64`。卸载过的版本可能只剩 `dist.lock` 等少量文件，显示为残留目录，可以连同目录一起删除。"),
            Step.Read(
                "查看哪些游戏被强制指定了兼容工具",
                "sed -n '/\"CompatToolMapping\"/,/^\\t\\t\\t\\t}/p' ~/.local/share/Steam/config/config.vdf | grep -E '^\\s*\"[0-9]+\"$|\"name\"' | tr -s '\\t' ' '",
                "`sed` 截出 `CompatToolMapping` 这一段，再只保留 AppID 与工具名两种行。").Then("AppID 与工具名交替出现，如 `\"1535560\"` / `\"name\" \"proton_experimental\"`、`\"1622780\"` / `\"name\" \"proton_513\"`；原生 Linux 游戏可能显示 `steamlinuxruntime`。没有强制指定过的游戏不会出现。"),
            Step.Read(
                "查看 Proton Experimental 是否在测试分支",
                "sed -n '/\"UserConfig\"/,/}/p' ~/.local/share/Steam/steamapps/appmanifest_1493710.acf",
                "Proton Experimental 的 AppID 是 1493710。选了测试分支时这里会有 `BetaKey`。").Then("未参加测试时只输出空的 `\"UserConfig\"` 与 `{ }`；切到 bleeding-edge 后多出一行 `\"BetaKey\"  \"bleeding-edge\"`。"),
            Step.Read(
                "查询 GE-Proton 的最新版本",
                "curl -fsSL https://api.github.com/repos/GloriousEggroll/proton-ge-custom/releases/latest | jq -r '.tag_name, (.assets[] | select(.name | test(\"x86_64\\\\.tar\\\\.gz$\")) | .browser_download_url)'",
                "发布页同时提供 x86_64 与 aarch64 两种包，`jq` 只挑出 Deck 需要的 x86_64 版本。").Then("两行：版本号如 `GE-Proton11-7`，以及下载地址 `https://github.com/GloriousEggroll/proton-ge-custom/releases/download/GE-Proton11-7/GE-Proton11-7-x86_64.tar.gz`。"),
            Step.Change(
                "下载 GE-Proton 并校验",
                "cd ~/Downloads && v=<版本号> && curl -LO \"https://github.com/GloriousEggroll/proton-ge-custom/releases/download/$v/$v-x86_64.tar.gz\" -LO \"https://github.com/GloriousEggroll/proton-ge-custom/releases/download/$v/$v-x86_64.sha512sum\" && sha512sum -c \"$v-x86_64.sha512sum\"",
                "`<版本号>` 填上一步的结果，如 `GE-Proton11-7`。`-LO` 跟随跳转并按原文件名保存；同时下载官方的 SHA-512 校验文件并核对。").Then("先显示两个文件的下载进度（压缩包约 500 MB），最后一行 `GE-Proton11-7-x86_64.tar.gz: 成功`。显示「失败」说明下载不完整，删掉重新下载。"),
            Step.Change(
                "解压到 Steam 的兼容工具目录",
                "mkdir -p ~/.local/share/Steam/compatibilitytools.d && tar -xzf ~/Downloads/<版本号>-x86_64.tar.gz -C ~/.local/share/Steam/compatibilitytools.d/ && ls ~/.local/share/Steam/compatibilitytools.d/",
                "`-C` 指定解压目标目录。解压完成后压缩包和校验文件可以删除。").Then("解压需要几十秒，没有进度输出；最后 `ls` 列出 `GE-Proton11-7-x86_64` 这样的目录，其中有 `proton`、`compatibilitytool.vdf`、`files/` 等。"),
            Step.Change(
                "退出 Steam 让它重新扫描兼容工具",
                "steam -shutdown",
                "Steam 只在启动时读取 `compatibilitytools.d`。桌面模式中退出后从菜单重新打开 Steam；游戏模式中 Steam 会自动重启。").Then("Steam 窗口关闭、进程退出。再次启动后，游戏「属性 → 兼容性」的下拉列表中出现 `GE-Proton11-7`。"),
        ],

        Verify =
        [
            "1. `ls ~/.local/share/Steam/compatibilitytools.d/*/proton` 列出 GE-Proton 目录中的 `proton` 脚本。",
            "2. 为某个游戏选用 GE-Proton 并启动一次后，`cat ~/.local/share/Steam/steamapps/compatdata/<AppID>/version` 显示的版本随之改变。",
        ],
    };
}
