using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class ProtonRuntimeDeepData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "proton-prefix",
        Title = "Proton 前缀与运行库修复",
        Keywords = "compatdata · 存档 · 运行库 · DLL 覆盖",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏闪退、缺运行库、Mod 不生效、存档在哪——这些问题都围绕同一个东西：游戏的 Proton 前缀。学会找到它、备份它、往里面装运行库，以及安全地重建它。",

        Why =
        [
            "1. 前缀是每个游戏独立的：一个游戏的前缀被改坏，不会影响其他游戏（结构见「Wine 与 Proton 原理」）。",
            "2. 为什么有的游戏缺运行库：",
            "• 少数游戏依赖 Proton 未完整实现的微软组件（某些 VC++ 运行库、.NET Framework、Media Foundation 视频解码等），表现为启动即退或播放过场动画时崩溃。",
            "• 最直接的做法：下载微软官方的安装程序（例如 `vc_redist.x64.exe`），用**游戏所用的那个 Proton** 在该游戏的前缀里运行它。Proton 目录下的 `proton` 脚本就是 Steam 启动游戏时调用的入口，只需设置两个环境变量告诉它前缀在哪、Steam 装在哪。",
            "• 其他组件（`d3dcompiler_47`、`xact` 等）通常借助 winetricks 安装。它依赖 `cabextract`，SteamOS 没有自带，可以在 Arch 的 Distrobox 容器里 `pacman -S winetricks cabextract` 后使用。",
            "3. DLL 覆盖与 Mod 加载：",
            "• 许多 Mod 加载器（BepInEx、UE4SS、ASI Loader、ReShade 等）把一个同名 DLL（如 `version.dll`、`dinput8.dll`、`winhttp.dll`、`dxgi.dll`）放在游戏目录里。",
            "• Wine 默认优先使用自己的内置实现 (builtin)，必须用 `WINEDLLOVERRIDES` 告诉它优先加载游戏目录里的原生版本 (native)：`n,b` 表示先 native 后 builtin。",
            "4. 前缀可以重建：删除前缀后，下次启动游戏时 Proton 会自动创建一个全新的。代价是前缀里的数据（包括本地存档、游戏设置）全部丢失。",
        ],

        Consequences =
        [
            "• 往前缀里装运行库、改 DLL 覆盖只影响这一个游戏。",
            "• **删除前缀会删除其中的存档**。删除前务必确认：游戏支持 Steam 云存档，或者你已备份 `drive_c/users/steamuser` 目录。",
            "• `WINEDLLOVERRIDES` 写错 DLL 名不会报错，只是不生效；覆盖了不该覆盖的系统 DLL 则可能导致游戏无法启动，删掉启动选项即可恢复。",
        ],

        Steps =
        [
            Step.Read(
                "按名称查找游戏的 AppID",
                "grep -l -i '\"name\".*<游戏名关键词>' ~/.local/share/Steam/steamapps/appmanifest_*.acf",
                "`<游戏名关键词>` 替换为游戏英文或本地化关键字（如 `hades`）。`-i` 忽略大小写，`-l` 只输出匹配文件路径。").Then("输出匹配的清单文件路径，如 `.../appmanifest_105600.acf`，文件名中的数字 105600 就是 AppID；没有匹配时没有输出。"),
            Step.Read(
                "查看前缀里的用户目录（存档常在这里）",
                "ls ~/.local/share/Steam/steamapps/compatdata/<AppID>/pfx/drive_c/users/steamuser/",
                "`<AppID>` 由上一步 manifest 文件名中的纯数字填入。存档多在 `AppData/Local`、`Documents` 等子目录。").Then("列出 `AppData`、`Documents`（或 `My Documents`）、`Saved Games` 等目录。"),
            Step.Change(
                "备份整个前缀的用户目录",
                "tar -czf ~/prefix-<AppID>-$(date +%F).tar.gz -C ~/.local/share/Steam/steamapps/compatdata/<AppID>/pfx/drive_c/users steamuser",
                "`<AppID>` 由第 1 步查得。`-c` 创建打包、`-z` gzip 压缩、`-C` 切换工作目录。解压恢复使用 `tar -xzf`。").Then("没有输出，家目录下出现 `prefix-<AppID>-<日期>.tar.gz`。原目录不变。"),
            Step.Read(
                "确认游戏使用的 Proton 版本",
                "ls ~/.local/share/Steam/steamapps/common/ | grep -i proton; cat ~/.local/share/Steam/steamapps/compatdata/<AppID>/version",
                "`<AppID>` 为游戏纯数字 ID。前一行列出已装 Proton 目录供下一步填入 `<Proton目录名>`，后一行输出当前前缀实际绑定的构建版本。").Then("先列出已安装的 Proton 目录名，最后一行是该前缀的版本，如 `11.0-100`，对应 `Proton 11.0`。"),
            Step.Change(
                "下载微软官方 VC++ 运行库安装程序",
                "curl -L -o ~/Downloads/vc_redist.x64.exe https://aka.ms/vs/17/release/vc_redist.x64.exe",
                "`-L` 跟随跳转，`-o` 指定保存位置。32 位游戏再下载 `vc_redist.x86.exe`。").Then("显示下载进度表，完成后 `~/Downloads/vc_redist.x64.exe` 大小约 25 MB。"),
            Step.Change(
                "用 Proton 在游戏前缀中运行安装程序",
                "STEAM_COMPAT_DATA_PATH=~/.local/share/Steam/steamapps/compatdata/<AppID> STEAM_COMPAT_CLIENT_INSTALL_PATH=~/.local/share/Steam \"$HOME/.local/share/Steam/steamapps/common/<Proton目录名>/proton\" run ~/Downloads/vc_redist.x64.exe /install /quiet /norestart",
                "`<AppID>` 由第 1 步查得；`<Proton目录名>`（如 `Proton - Experimental`）由上一步 `ls` 查得。执行前先退出游戏。`/quiet` 静默安装。").Then("终端输出若干 Wine 日志行，约半分钟后返回，不弹出安装界面。之后该前缀的 `system32` 中出现 `vcruntime140.dll` 等文件，游戏启动时就能找到这些运行库。"),
            Step.Change(
                "让游戏优先加载目录里的 Mod DLL",
                "WINEDLLOVERRIDES=\"winhttp,version,dinput8=n,b\" %command%",
                "`n,b` 表示先原生 (native) 后内置 (builtin)。只保留你的 Mod 实际使用的那个 DLL 名，多个用逗号分隔。").On(Hosts.SteamLaunchOptions).Then("游戏启动时优先加载游戏目录里的同名 DLL（例如 Mod 加载器的 `winhttp.dll`），Mod 开始生效；目录里没有这个 DLL 时照常使用 Wine 内置版本。"),
            Step.Danger(
                "删除前缀以彻底重建",
                "rm -rf ~/.local/share/Steam/steamapps/compatdata/<AppID>",
                "`<AppID>` 由第 1 步查得。**会连带删除本地未同步云端的存档**，操作前务必先执行备份步骤。").Then("没有输出，前缀目录被永久删除。下次启动游戏时 Steam 显示「正在准备启动」，Proton 花十几秒建立一个全新的前缀。"),
        ],

        Verify =
        [
            "1. 备份：`tar -tzf ~/prefix-<AppID>-*.tar.gz | head` 能列出 `steamuser/` 下的文件。",
            "2. Mod 加载：以 BepInEx 为例，启动一次游戏后游戏目录下出现 `BepInEx/LogOutput.log`，说明 DLL 覆盖已生效。",
            "3. 运行库：`ls ~/.local/share/Steam/steamapps/compatdata/<AppID>/pfx/drive_c/windows/system32/ | grep -i vcruntime` 能看到对应 DLL。",
        ],
    };
}
