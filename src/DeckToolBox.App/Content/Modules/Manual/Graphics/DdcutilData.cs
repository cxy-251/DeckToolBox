using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Graphics;

public static class DdcutilData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-ddcutil",
        Title = "ddcutil：控制外接显示器",
        Keywords = "DDC/CI · 亮度 · 对比度 · 输入源 · VCP · 扩展坞",
        Level = ModuleLevel.Intermediate,
        Overview = "大多数外接显示器支持 DDC/CI 协议：电脑可以通过显示线直接调节显示器的亮度、对比度、切换输入源，而不必去按显示器背后的按钮。ddcutil 是 SteamOS 自带的命令行工具。能否使用取决于显示器与连接方式。",

        Why =
        [
            "1. 原理：显示线（HDMI、DP）中有一条低速的 I2C 通道，DDC/CI 在其上传输命令。每项可调参数有一个 VCP 编号：",
            "- `10`：亮度；`12`：对比度；`60`：输入源；`62`：音量；`D6`：电源模式。",
            "2. 常用命令：",
            "- `ddcutil detect`：列出检测到的显示器、所在 I2C 总线和型号。",
            "- `ddcutil capabilities`：显示器声明支持的 VCP 项与取值。",
            "- `ddcutil getvcp 10`：读取亮度；`ddcutil setvcp 10 50`：设为 50。",
            "- 多台显示器时用 `-d 编号` 或 `--bus N` 指定。",
            "3. 限制：",
            "• 内置屏 (eDP) 不支持 DDC/CI，会显示为 `Invalid display`，这是正常的；内置屏亮度用快捷菜单或系统设置调节。",
            "• **通过 USB-C 扩展坞或转接头连接时，很多设备不转发 DDC/CI 信号**，ddcutil 能看到显示器型号，却提示 `Invalid display` 或 `Display not found`。直接用 USB-C 转 DP 线连接成功率更高。",
            "• 显示器菜单中可能有「DDC/CI」开关，需要保持开启。",
            "4. 权限：访问 `/dev/i2c-*` 需要 `i2c` 组，普通用户部分操作需要 sudo。",
        ],

        Consequences =
        [
            "• 读取与调节亮度无害；切换输入源 (`60`) 会让画面立即切到其他接口，需要用显示器按钮切回。",
            "• 不要随意写入不了解的 VCP 项，部分显示器的出厂设置项写错后需要在显示器菜单中恢复。",
        ],

        Steps =
        [
            Step.Read(
                "检测显示器",
                "ddcutil detect 2>&1 | grep -E '^Display|^Invalid|I2C bus|DRM_connector|Model'",
                "只保留关键行。").Then("每台显示器一段：`Display 1`（可用）或 `Invalid display`（不可用），附带 I2C 总线、连接器（`card0-eDP-1` 为内置屏、`card0-DP-1` 为外接）和型号。内置屏总是 `Invalid display`。"),
            Step.Root(
                "读取外接显示器亮度",
                "sudo ddcutil getvcp 10",
                "只有 `Display 1` 这类可用显示器才能读取。").Then("输出 `VCP code 0x10 (Brightness): current value = 70, max value = 100`；不支持时提示 `Display not found`。"),
            Step.Root(
                "设置亮度",
                "sudo ddcutil setvcp 10 <0到100>",
                "例如 `sudo ddcutil setvcp 10 40`。").Then("显示器亮度立即改变，没有其他输出。"),
            Step.Root(
                "查看显示器支持的全部可调项",
                "sudo ddcutil capabilities",
                "输出较长。").Then("列出型号、支持的 VCP 代码及名称（如 `Feature: 10 (Brightness)`、`Feature: 60 (Input Source)` 及其可选值）。"),
        ],

        Verify =
        [
            "1. `sudo ddcutil getvcp 10` 读出的数值与显示器菜单中显示的亮度一致。",
        ],
    };
}
