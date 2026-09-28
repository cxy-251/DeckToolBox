using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.NativeContainers;

public static class WineProtonRuntimeData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "wine-proton",
        Title = "Wine 与 Proton 原理",
        Keywords = "Wine · DXVK · VKD3D-Proton · ntsync",
        Level = ModuleLevel.Intermediate,
        Overview = "Windows 游戏能在 Deck 上运行，靠的是 Proton：Valve 基于 Wine 的兼容层，加上把 DirectX 翻译成 Vulkan 的 DXVK / VKD3D-Proton。理解它的结构，排查问题时就知道该看哪里。",

        Why =
        [
            "1. Wine 不是模拟器：",
            "• 游戏的 x86 指令直接在 CPU 上原生执行，没有指令模拟开销。",
            "• Wine 重新实现了 Windows 的 DLL 与系统接口：游戏调用 `CreateFileW` 时，Wine 把它转换成 Linux 的 `open`；窗口、输入、音频同理。",
            "2. 前缀 (WINEPREFIX)：每个游戏一套独立的「Windows 环境」，位于 `steamapps/compatdata/<AppID>/pfx/`：",
            "- `drive_c/`：虚拟 C 盘，存档常在 `drive_c/users/steamuser/` 下的 `AppData` 或 `Documents` 里。",
            "- `system.reg`、`user.reg`：纯文本格式的注册表。",
            "- `dosdevices/`：盘符映射，`c:` 指向 `drive_c`，`z:` 指向 Linux 根目录 `/`。",
            "3. Proton 在 Wine 之上加入的关键组件：",
            "- **DXVK**：把 Direct3D 9/10/11 翻译为 Vulkan。",
            "- **VKD3D-Proton**：把 Direct3D 12 翻译为 Vulkan。",
            "- **同步原语加速**：Windows 游戏大量使用互斥锁、事件等同步对象。早期用 esync/fsync 模拟，新内核提供 `ntsync` 驱动（`/dev/ntsync`），语义更贴近 Windows，新版 Proton 可以直接使用。",
            "4. 版本并存：Steam 可同时安装多个 Proton（Experimental、10.0、9.0…），每个游戏可在「属性 → 兼容性」里单独指定。",
        ],

        Consequences =
        [
            "• 查看前缀目录、Proton 版本都是只读操作。",
            "• 为某个游戏更换 Proton 版本可能让已有前缀不兼容，出现问题时可退回原版本。",
            "• 性能通常接近 Windows，个别游戏更快或更慢；反作弊、部分 DRM 仍可能导致无法运行，这不是配置能解决的问题，可在 ProtonDB 查看其他玩家的报告。",
        ],

        Steps =
        [
            Step.Read(
                "查看已安装的 Proton 版本",
                "ls ~/.local/share/Steam/steamapps/common/ | grep -i proton",
                "Proton 也是 Steam「库 → 工具」里的条目，可以像游戏一样安装和卸载。").Then("每行一个 Proton 目录名，例如 `Proton 10.0`、`Proton - Experimental`、`Proton Hotfix`。"),
            Step.Read(
                "列出已安装游戏的 AppID 与名称",
                "grep -H '\"name\"' ~/.local/share/Steam/steamapps/appmanifest_*.acf | sed -E 's#.*appmanifest_([0-9]+)\\.acf:[[:space:]]*\"name\"[[:space:]]*\"(.*)\"#\\1  \\2#'",
                "只包含内置存储上的库；SD 卡上的游戏把路径换成 `/run/media/deck/<卡名>/steamapps/`。").Then("每行「AppID  名称」，例如 `105600  Terraria`。Proton、Steam Runtime 等工具也有 AppID，会一起列出。"),
            Step.Read(
                "查看某个游戏前缀的结构",
                "ls ~/.local/share/Steam/steamapps/compatdata/<AppID>/pfx/",
                "`<AppID>` 替换为上一步查到的数字。").Then("列出 `dosdevices  drive_c  system.reg  user.reg  userdef.reg` 等条目。"),
            Step.Read(
                "确认内核提供 ntsync",
                "ls -l /dev/ntsync",
                "字符设备存在即表示内核支持。").Then("显示一行设备信息，如 `crw-rw-rw- 1 root root 10, 260 ... /dev/ntsync`；若提示「没有那个文件或目录」则内核不支持。"),
            Step.Change(
                "启用 DXVK 自带的性能浮层",
                "DXVK_HUD=fps,frametimes,gpuload %command%",
                "填到游戏的「属性 → 通用 → 启动选项」。只对 D3D9~11 游戏生效。").On(Hosts.SteamLaunchOptions).Then("游戏画面左上角出现 DXVK 的文字浮层：帧率、帧时间曲线和 GPU 负载。D3D12 或原生游戏不显示。"),
            Step.Change(
                "记录 Proton 运行日志",
                "PROTON_LOG=1 %command%",
                "排查完记得删掉这个启动选项。").On(Hosts.SteamLaunchOptions).Then("游戏照常运行；退出后家目录出现 `steam-<AppID>.log`，可能有几 MB 到上百 MB，记录了加载的组件与报错。"),
        ],

        Verify =
        [
            "1. 开启 `PROTON_LOG=1` 运行一次游戏后，`ls ~/steam-*.log` 能看到日志；用 `grep -i -E 'err:|fixme:.*dll' ~/steam-<AppID>.log | head` 查找缺失 DLL 等线索。",
            "2. 开启 DXVK_HUD 后，游戏画面左上角出现帧率、帧时间曲线和 GPU 负载。",
        ],
    };
}
