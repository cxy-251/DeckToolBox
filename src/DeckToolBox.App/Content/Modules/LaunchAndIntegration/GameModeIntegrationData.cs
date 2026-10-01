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
            "5. `shortcuts.vdf` 的格式：它是二进制 KeyValues，每个字段以类型字节开头（`0x00` 子节点、`0x01` 字符串、`0x02` 32 位整数），键名与字符串以空字节结尾。每个快捷方式记录 `appid`、`AppName`、`Exe`、`StartDir`、`icon`、`LaunchOptions` 等。`appid` 是 Steam 添加时分配的数字，大于 2147483648；用 Proton 运行的非 Steam 程序，其前缀目录就是 `compatdata/<这个 appid>/`，自定义封面也按它命名。网上流传的「Exe 路径 + 名称做 CRC32」只是旧版的生成方式，与新版 Steam 写入的值对不上，应以文件里实际记录的为准。",
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
                "`<程序名>` 自定义应用目录名；`<可执行文件>` 为该目录下的二进制文件名。`<<'EOF'` 写入脚本，`exec` 替换进程以便 Steam 追踪退出状态。").Then("没有输出。`~/Applications/<程序名>/` 下出现可执行的 `launch.sh`；把它添加为非 Steam 游戏后，Steam 启动的是这个脚本，由它清理环境再启动真正的程序。"),
            Step.Read(
                "确认快捷方式文件的位置",
                "ls -l ~/.local/share/Steam/userdata/*/config/shortcuts.vdf",
                "每个登录过的 Steam 账号一个目录。备份这个文件即可备份全部非 Steam 快捷方式。").Then("每个 Steam 账号一行，显示 `shortcuts.vdf` 的大小、修改时间和完整路径；从未添加过非 Steam 游戏的账号可能没有这个文件。"),
            Step.Read(
                "列出每个非 Steam 快捷方式的名称、程序路径与启动选项",
                "tr '\\000\\001\\002' '\\n\\n\\n' < $(ls ~/.local/share/Steam/userdata/*/config/shortcuts.vdf | head -n 1) | grep -a -A1 -E '^(AppName|Exe|LaunchOptions)$' | grep -v '^--$'",
                "`tr` 把三种分隔字节都换成换行，键名和值就各占一行；`grep -A1` 取键名及其下一行的值。有多个 Steam 账号时 `head -n 1` 只取第一个。").Then("每个快捷方式三组，如 `AppName` / `Telegram`、`Exe` / `/home/deck/Applications/Telegram/Telegram`、`LaunchOptions` / `LD_PRELOAD=\"\" %command%`；没有设置启动选项的，值那一行是下一个键名 `IsHidden`。"),
            Step.Read(
                "读出每个非 Steam 快捷方式的 appid",
                "f=$(ls ~/.local/share/Steam/userdata/*/config/shortcuts.vdf | head -n 1); for off in $(grep -aob $'\\x02appid\\x00' \"$f\" | cut -d: -f1); do id=$(od -An -tu4 -j $((off+7)) -N4 \"$f\" | tr -d ' '); name=$(tail -c +$((off+12)) \"$f\" | tr '\\000\\001' '\\n\\n' | sed -n '3p'); echo \"$id  $name\"; done",
                "`grep -aob` 找出每个 `appid` 字段的字节位置；值是紧跟在后面的 4 字节小端整数，`od -tu4` 把它读成十进制；名称是随后的 `AppName` 字段。").Then("每个快捷方式一行，如 `2689736841  Telegram`。用 Proton 运行过的程序，在 `~/.local/share/Steam/steamapps/compatdata/` 下能找到同名的数字目录。"),
        ],

        Verify =
        [
            "1. 切回游戏模式（桌面上的「返回游戏模式」图标），在「库 → 非 Steam」分类中能看到新条目并正常启动。",
            "2. 程序运行时按 `…` 键能打开快捷菜单并调整帧率限制，说明它已处于 Gamescope 管理之下。",
        ],
    };
}
