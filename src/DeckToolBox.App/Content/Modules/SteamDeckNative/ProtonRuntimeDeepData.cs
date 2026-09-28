using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class ProtonRuntimeDeepData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "proton-prefix",
        Title = "Proton 前缀与运行库修复",
        Keywords = "compatdata · 存档 · protontricks · DLL 覆盖",
        Level = ModuleLevel.Intermediate,
        Overview = "游戏闪退、缺运行库、Mod 不生效、存档在哪——这些问题都围绕同一个东西：游戏的 Proton 前缀。学会找到它、备份它、往里面装运行库，以及安全地重建它。",

        Why =
        [
            "1. 前缀是每个游戏独立的：一个游戏的前缀被改坏，不会影响其他游戏（结构见「Wine 与 Proton 原理」）。",
            "2. 为什么有的游戏缺运行库：",
            "• 少数游戏依赖 Proton 未完整实现的微软组件（某些 VC++ 运行库、.NET Framework、Media Foundation 视频解码等），表现为启动即退或播放过场动画时崩溃。",
            "• **protontricks** 可以把官方的可再发行组件安装进指定游戏的前缀。它不是系统自带的，需要从 Flathub 安装（`com.github.Matoking.protontricks`）。",
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
            Step.Change(
                "从 Flathub 安装 protontricks",
                "flatpak install flathub com.github.Matoking.protontricks",
                "需要已配置 flathub 源（见「Flatpak 应用管理」）。"),
            Step.Change(
                "为游戏安装 VC++ 运行库（示例）",
                "flatpak run com.github.Matoking.protontricks <AppID> vcrun2022",
                "其他常用组件：`d3dcompiler_47`、`dotnet48`、`xact`。装之前先在 ProtonDB 确认该游戏确实需要。"),
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
