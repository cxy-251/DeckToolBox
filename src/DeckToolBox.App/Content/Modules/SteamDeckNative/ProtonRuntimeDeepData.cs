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
                "输出的文件名 `appmanifest_<AppID>.acf` 中的数字就是 AppID。"),
            Step.Read(
                "查看前缀里的用户目录（存档常在这里）",
                "ls ~/.local/share/Steam/steamapps/compatdata/<AppID>/pfx/drive_c/users/steamuser/",
                "重点看 `AppData/Local`、`AppData/LocalLow`、`AppData/Roaming`、`Documents`（部分版本中显示为 `My Documents`）。"),
            Step.Change(
                "备份整个前缀的用户目录",
                "tar -czf ~/prefix-<AppID>-$(date +%F).tar.gz -C ~/.local/share/Steam/steamapps/compatdata/<AppID>/pfx/drive_c/users steamuser",
                "在做任何修改或删除前先备份。恢复时用 `tar -xzf` 解压回同一位置。"),
            Step.Read(
                "确认游戏使用的 Proton 版本",
                "ls ~/.local/share/Steam/steamapps/common/ | grep -i proton; cat ~/.local/share/Steam/steamapps/compatdata/<AppID>/version",
                "`version` 文件记录了创建该前缀的 Proton 版本，下一步要用同一个版本。"),
            Step.Change(
                "下载微软官方 VC++ 运行库安装程序",
                "curl -L -o ~/Downloads/vc_redist.x64.exe https://aka.ms/vs/17/release/vc_redist.x64.exe",
                "`aka.ms` 是微软官方的短链接，指向最新的 VC++ 2015~2022 x64 运行库。32 位游戏再下载 `vc_redist.x86.exe`。"),
            Step.Change(
                "用 Proton 在游戏前缀中运行安装程序",
                "STEAM_COMPAT_DATA_PATH=~/.local/share/Steam/steamapps/compatdata/<AppID> STEAM_COMPAT_CLIENT_INSTALL_PATH=~/.local/share/Steam \"$HOME/.local/share/Steam/steamapps/common/<Proton目录名>/proton\" run ~/Downloads/vc_redist.x64.exe /install /quiet /norestart",
                "`<Proton目录名>` 例如 `Proton - Experimental`、`Proton 10.0`（含空格，已用引号包住）。执行前先退出该游戏。`/quiet` 静默安装，不弹界面。"),
            Step.Change(
                "让游戏优先加载目录里的 Mod DLL",
                "WINEDLLOVERRIDES=\"winhttp,version,dinput8=n,b\" %command%",
                "只保留你的 Mod 实际使用的那个 DLL 名，多个用逗号分隔。").On(Hosts.SteamLaunchOptions),
            Step.Danger(
                "删除前缀以彻底重建",
                "rm -rf ~/.local/share/Steam/steamapps/compatdata/<AppID>",
                "**会删除本地存档**，先完成上面的备份。下次启动游戏时自动生成全新前缀。"),
        ],

        Verify =
        [
            "1. 备份：`tar -tzf ~/prefix-<AppID>-*.tar.gz | head` 能列出 `steamuser/` 下的文件。",
            "2. Mod 加载：以 BepInEx 为例，启动一次游戏后游戏目录下出现 `BepInEx/LogOutput.log`，说明 DLL 覆盖已生效。",
            "3. 运行库：`ls ~/.local/share/Steam/steamapps/compatdata/<AppID>/pfx/drive_c/windows/system32/ | grep -i vcruntime` 能看到对应 DLL。",
        ],
    };
}
