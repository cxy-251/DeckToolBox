using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.LaunchAndIntegration;

public static class GameModeIntegrationData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "non-steam-games",
        Title = "非 Steam 程序接入游戏模式",
        Keywords = "添加非 Steam 游戏 · shortcuts.vdf · 启动脚本",
        Level = ModuleLevel.Basic,
        Overview = "游戏模式里只能启动 Steam 库中的条目。把便携应用、模拟器、Distrobox 导出的程序或脚本「添加为非 Steam 游戏」，就能在游戏模式中用手柄启动，并使用 Steam Input 和快捷菜单。",

        Why =
        [
            "1. 游戏模式不加载 KDE 桌面，也没有应用菜单，所有可启动的东西都来自 Steam 库。",
            "2. 非 Steam 快捷方式保存在 `~/.local/share/Steam/userdata/<Steam 用户 ID>/config/shortcuts.vdf`（二进制格式），由 Steam 客户端读写。**不要在 Steam 运行时直接编辑它**，Steam 退出时会覆盖你的修改。",
            "3. 添加后可获得：",
            "- Steam Input：为程序单独配置按键、触控板和陀螺仪映射。",
            "- 快捷菜单中的帧率限制、缩放、TDP 设置。",
            "- 休眠与恢复：按电源键挂起，唤醒后接着用。",
            "4. 为什么有时需要包装脚本：从 Steam 启动的进程会继承 Steam 注入的环境变量（`LD_PRELOAD` 中的覆盖层库等）。少数程序（部分 Electron / Chromium 应用）会因此黑屏或卡死，这时用一个脚本先清理环境再启动程序。",
        ],

        Consequences =
        [
            "• 添加、删除快捷方式都在 Steam 界面完成，不影响程序本身。",
            "• 包装脚本清除 `LD_PRELOAD` 后，该程序中无法使用 Steam 覆盖层（`Steam` 键菜单仍可用）。",
            "• 用 `distrobox-export --app` 导出到菜单的程序，也可以直接在「添加非 Steam 游戏」列表中勾选，Steam 会通过 `distrobox-enter` 启动它。",
        ],

        Steps =
        [
            Step.Change(
                "在 Steam 桌面客户端中添加程序",
                "Steam 桌面客户端 → 左下角「添加游戏」→「添加非 Steam 游戏」→ 勾选程序，或点「浏览」选择 ~/Applications 下的文件 →「添加所选程序」",
                "这是界面操作步骤，不是终端命令。添加后可在该条目的「属性」里改名称、启动选项和图标。").On(Hosts.SteamClient).Then("弹出程序列表窗口；勾选并确认后，程序出现在「库」的「非 Steam」分类中，可以像游戏一样启动、设置控制器布局。"),
            Step.Change(
                "为有问题的程序编写包装脚本",
                "mkdir -p ~/Applications/<程序名> && cat > ~/Applications/<程序名>/launch.sh <<'EOF'\n#!/usr/bin/env bash\n# 清除 Steam 注入的覆盖层，再启动真正的程序\nunset LD_PRELOAD\nexec \"$HOME/Applications/<程序名>/<可执行文件>\" \"$@\"\nEOF\nchmod +x ~/Applications/<程序名>/launch.sh",
                "`<<'EOF'` 到 `EOF` 之间是写入文件的内容。`exec` 让程序替换脚本进程，Steam 能正确追踪程序何时退出。").Then("没有输出。`~/Applications/<程序名>/` 下出现可执行的 `launch.sh`；把它添加为非 Steam 游戏后，Steam 启动的是这个脚本，由它清理环境再启动真正的程序。"),
            Step.Read(
                "确认快捷方式文件的位置",
                "ls -l ~/.local/share/Steam/userdata/*/config/shortcuts.vdf",
                "每个登录过的 Steam 账号一个目录。备份这个文件即可备份全部非 Steam 快捷方式。").Then("每个 Steam 账号一行，显示 `shortcuts.vdf` 的大小、修改时间和完整路径；从未添加过非 Steam 游戏的账号可能没有这个文件。"),
        ],

        Verify =
        [
            "1. 切回游戏模式（桌面上的「返回游戏模式」图标），在「库 → 非 Steam」分类中能看到新条目并正常启动。",
            "2. 程序运行时按 `…` 键能打开快捷菜单并调整帧率限制，说明它已处于 Gamescope 管理之下。",
        ],
    };
}
