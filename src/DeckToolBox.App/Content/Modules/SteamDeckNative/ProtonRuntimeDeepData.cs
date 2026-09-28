using System.Collections.Generic;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SteamDeckNative;

public static class ProtonRuntimeDeepData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "proton_runtime_deep",
        Title = "Proton 兼容层深水区与 Windows 运行库补丁 (Protontricks / DLL 劫持 / 虚拟C盘)",
        Subtitle = "Proton 兼容层与运行库补丁",
        CategoryName = "Proton 兼容层与运行库补丁",
        Badge = "转译实战",
        Overview = "深入解析 Proton 独立沙箱前缀目录架构 (compatdata / pfx)、Protontricks 运行库静默补齐原理与 WINEDLLOVERRIDES 动态链接库劫持生效法则。",

        WhyCard = new InfoCardItem(
            "📌",
            "揭秘 Proton 虚拟 C 盘隔离机制与 Windows 运行库痛点",
            "#1F6FEB",
            "#58A6FF",
            new[]
            {
                "1. 每一个游戏拥有一套独立的虚拟 Windows 系统 (WINEPREFIX)：",
                "• Steam 为每个 Windows 游戏在 `~/.steam/steam/steamapps/compatdata/<AppID>/pfx/` 目录下建立了一套专属的沙箱前缀 (Prefix)。",
                "• 里面包含完整的 `drive_c/windows/`、`drive_c/users/steamuser/AppData/` 以及文本形式的 Windows 注册表文件 (`system.reg` / `user.reg`)。这保证了一个游戏的注册表损坏或存档崩溃绝不会连累其他任何游戏！",

                "2. 很多单机大作或汉化补丁闪退的真正原因：",
                "• 很多游戏依赖特定版本的微软运行库（如 Visual C++ 2015-2022、DirectX 9、.NET 4.8 或 Media Foundation 视频解码）。Proton 自带的逆向开源实现并非 100% 涵盖所有专有未公开 API，遇到空指针调用时游戏便会瞬间闪退。",
                "• **Protontricks** 工具能够直接调用微软官方的可再发行组件包 (Redistributable)，将原装正版的微软官方 DLL 库与注册表项静默安装注入到指定 AppID 的虚拟 C 盘中，瞬间起死回生！",

                "3. `WINEDLLOVERRIDES` 动态链接库劫持与 Mod 加载机制：",
                "• 为什么所有单机游戏 Mod (如 Unity BepInEx、Unreal UE4SS、老滚 ASI Loader) 都要求把一个伪装的 `version.dll` 或 `dinput8.dll` 放进游戏目录？",
                "• 默认情况下，Proton 启动游戏时会优先加载 Wine 自身内部编写的虚拟系统库 (`b`, Builtin)。",
                "• 必须在启动项中加入 `WINEDLLOVERRIDES=\"dinput8=n,b;version=n,b\" %command%`：**强制指示转译器优先加载游戏根目录下用户放置的原生 DLL (`n`, Native)**，加载成功后再回退到内置库 (`b`, Builtin)，这是让所有注入型 Mod 在 Linux 掌机上顺利跑起来的唯一金钥匙！"
            }
        ),

        ConsequenceCard = new InfoCardItem(
            "⚠️",
            "前缀修改与重置的安全隔离性 (Consequences)",
            "#D29922",
            "#D29922",
            new[]
            {
                "• 极致的隔离安全性：所有使用 protontricks 注入的 DLL 和注册表修改完全禁锢在 `compatdata/<AppID>` 内部，绝不污染 SteamOS 宿主系统。",
                "• 终极无损自愈能力：若某个游戏的前缀被 Mod 或插件彻底改坏导致打不开，只需直接删除对应的 `~/.steam/steam/steamapps/compatdata/<AppID>` 文件夹；下次在 Steam 中点击开始游戏时，Proton 会在 10 秒内自动重新克隆生成一个全新、纯净出厂状态的 Windows 运行环境！"
            }
        ),

        Steps = new List<StepCommandItem>
        {
            new(
                1,
                "查看当前掌机已生成的全部 Windows 游戏沙箱前缀目录清单",
                "ls -lh ~/.steam/steam/steamapps/compatdata/ 2>/dev/null | head -n 25",
                "每个数字文件夹即对应 Steam 商店中该游戏的唯一数字身份标识 (AppID)。"
            ),
            new(
                2,
                "查看指定游戏沙箱内部的真实 Windows 虚拟目录树与注册表文件",
                "ls -la ~/.steam/steam/steamapps/compatdata/*/pfx/ 2>/dev/null | head -n 25",
                "查看 drive_c, dosdevices (盘符软链), system.reg 与 user.reg 存储形态。"
            ),
            new(
                3,
                "使用 protontricks 为指定游戏静默安装缺失的微软 VC++ 运行库（示例）",
                "echo '安装命令模板: protontricks <AppID> vcrun2022 d3dx9 (可自动为游戏注入微软官方 2022 运行库与 DirectX 9 组件)'",
                "为指定 AppID 虚拟 C 盘静默安装正版微软 VC++ 与 DirectX 9 运行库，解决老游戏与特定引擎闪退问题。"
            ),
            new(
                4,
                "实战掌握加载单机 Mod 与动态链接库劫持的标准 Steam 启动项",
                "echo '启动项模板: WINEDLLOVERRIDES=\"version=n,b;dinput8=n,b;dxgi=n,b\" %command%'",
                "强制转译器优先加载游戏根目录下用户放置的原生 Mod DLL (Native)，是单机 Mod 生效的通用标准启动项。"
            ),
            new(
                5,
                "一键重置损坏的游戏虚拟 C 盘环境（故障秒级自愈）",
                "echo '自愈指令: rm -rf ~/.steam/steam/steamapps/compatdata/<AppID> (下次启动自动原地重建干净环境)'",
                "直接物理删除损坏的虚拟 C 盘前缀，Steam 会在下次启动时自动重新克隆生成干净出厂状态的 Windows 环境。"
            )
        },

        VerificationCard = new InfoCardItem(
            "🔍",
            "前缀环境与 DLL 劫持判定 (Verification)",
            "#238636",
            "#3FB950",
            new[]
            {
                "1. DLL 劫持成功验证：启动包含 Mod 的游戏后，查看游戏根目录下是否生成了对应的日志文件（如 `BepInEx/LogOutput.log`），若生成且包含 `Loaded plugins`，证实 WINEDLLOVERRIDES 成功驱动了 Native DLL 加载。",
                "2. 运行库安装验证：使用 `ls -l ~/.steam/steam/steamapps/compatdata/<AppID>/pfx/drive_c/windows/system32/vcruntime140.dll`，若文件存在且为微软原始二进制，证实运行库注入成功。"
            }
        )
    };
}
