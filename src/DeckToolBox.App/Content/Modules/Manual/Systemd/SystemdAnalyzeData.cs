using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Systemd;

public static class SystemdAnalyzeData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-systemd-analyze",
        Title = "systemd-analyze：开机耗时分析",
        Keywords = "blame · critical-chain · plot · calendar · verify",
        Level = ModuleLevel.Intermediate,
        Overview = "开机变慢了？systemd-analyze 能告诉你开机总共花了多久、哪些服务启动最慢、哪条依赖链决定了进入桌面的时间，还能生成一张启动过程的时间线图。它也用来检查定时器表达式和单元文件的写法。",

        Why =
        [
            "1. `systemd-analyze`（不带参数）：开机总耗时，拆分为固件 (firmware)、引导程序 (loader)、内核 (kernel)、用户空间 (userspace) 四段。",
            "2. `blame`：按启动耗时列出每个单元。**注意：耗时长不一定拖慢了开机**，许多服务是并行启动的，或在后台慢慢完成；像 `esp.mount` 这类按需挂载的单元，显示的时间可能包含了等待访问的时间。",
            "3. `critical-chain`：从目标（默认 `graphical.target`）往回追溯的关键路径，`@` 后是该单元就绪的时刻，`+` 后是它自身的耗时。这条链上的单元才是真正决定开机速度的。",
            "4. `plot > 文件.svg`：生成 SVG 时间线图，用浏览器打开可以看到每个单元的启动时段。",
            "5. 其他用途：",
            "- `calendar '表达式'`：检查定时器的时间表达式，显示下一次触发时间。",
            "- `verify 单元文件`：检查自己写的单元文件有无语法错误。",
            "- `--user`：分析用户会话（登录后桌面组件）的启动。",
        ],

        Consequences =
        [
            "• 全部是只读分析。",
            "• SteamOS 的开机服务由 Valve 配置，不要为了快几秒随意禁用系统服务；可以检查的是你自己添加的服务（例如代理、同步工具）是否拖慢了开机。",
        ],

        Steps =
        [
            Step.Read(
                "查看开机总耗时",
                "systemd-analyze",
                "解析内核与 systemd 启动时戳；拆解固件（firmware）、加载器（loader）、内核（kernel）与用户空间（userspace）各自耗时。").Then("输出如 `Startup finished in 4.1s (firmware) + 1.4s (loader) + 1.5s (kernel) + 10.7s (userspace) = 17.8s` 和 `graphical.target reached after 10.7s in userspace`。"),
            Step.Read(
                "按耗时排序各单元",
                "systemd-analyze blame --no-pager | head -n 10",
                "耗时长不代表拖慢了开机，要结合下一步判断。").Then("每行「耗时 单元名」，最慢的在最上面，如 `9.053s atomupd.service`。"),
            Step.Read(
                "查看决定开机时间的关键链",
                "systemd-analyze critical-chain --no-pager",
                "只有这条链上的单元才真正延迟了进入桌面。").Then("以树形从 `graphical.target @10.685s` 往回列出依赖链，每个单元带 `@就绪时刻` 与 `+自身耗时`，例如某个 `...service @5.9s +4.7s` 说明它占用了 4.7 秒。"),
            Step.Read(
                "查看用户会话中各组件的启动耗时",
                "systemd-analyze --user blame --no-pager | head -n 8",
                "登录桌面后启动的用户服务。").Then("列出 `pipewire.service`、`plasma-powerdevil.service` 等用户服务的启动耗时。"),
            Step.Change(
                "生成启动时间线图",
                "systemd-analyze plot > ~/boot.svg && xdg-open ~/boot.svg",
                "SVG 可以用浏览器或图片查看器打开。").Then("家目录生成 `boot.svg` 并用默认程序打开：横轴为时间，每个单元一条彩色横条，一眼能看出哪些服务在并行、哪些在排队。"),
            Step.Read(
                "检查定时器表达式",
                "systemd-analyze calendar 'Mon..Fri 09:00' --iterations=3",
                "`--iterations` 显示接下来几次触发。").Then("输出规范化写法 `Mon..Fri *-*-* 09:00:00` 和接下来三个工作日 9 点的具体时间。"),
        ],

        Verify =
        [
            "1. `systemd-analyze critical-chain sshd.service` 显示 sshd 在开机过程中何时就绪。",
        ],
    };
}
