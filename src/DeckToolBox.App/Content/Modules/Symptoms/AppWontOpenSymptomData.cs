using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Symptoms;

public static class AppWontOpenSymptomData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sym-app-wont-open",
        Title = "下载的程序打不开",
        Keywords = "AppImage · 便携程序 · 双击没反应 · 权限 · 缺少库 · not found · 非 Steam",
        Level = ModuleLevel.Basic,
        Overview = "下载的 AppImage 或便携程序双击没反应，或者添加到 Steam 后在游戏模式里启动不了。双击运行看不到任何错误信息，所以第一步总是在终端里运行它，看它说了什么。",

        Why =
        [
            "常见原因：",
            "1. **没有执行权限**：从浏览器下载的文件默认不可执行。",
            "2. **缺少共享库**：程序是为其他发行版编译的，依赖的某个 `.so` 在 SteamOS 上没有。",
            "3. **下载的版本不对**：例如下到了 ARM (aarch64) 版本，或者其实是 Windows 程序。",
            "4. **只在游戏模式中失败**：Steam 注入的覆盖层与部分程序冲突，需要清空 `LD_PRELOAD`。",
        ],

        Consequences =
        [
            "• 第 1、3 步只读；第 2 步在终端里实际运行程序，效果与双击相同。",
        ],

        Steps =
        [
            Step.Read(
                "看看这是什么文件、有没有执行权限",
                "ls -l <程序路径>; file <程序路径> | cut -c1-120",
                "`<程序路径>` 如 `~/Applications/Foo.AppImage`。").Then("**`ls` 的权限中没有 `x`**（如 `-rw-r--r--`）→ 执行 `chmod +x <程序路径>` 后再试。**`file` 显示 `ARM aarch64`** → 下错了版本，重新下载 x86_64 版。**显示 `PE32+ executable ... MS Windows`** → 这是 Windows 程序，要用 Proton 运行（添加为非 Steam 游戏并强制使用 Proton）。正常的应为 `ELF 64-bit LSB ... x86-64`。"),
            Step.Change(
                "在终端里运行，看错误信息",
                "<程序路径>",
                "直接输入路径回车。图形程序会正常打开窗口；失败时终端里会留下原因。按 `Ctrl+C` 结束。").Then("**`error while loading shared libraries: libxxx.so.1`** → 缺少库，做第 3 步。**`dlopen(): error loading libfuse.so.2`** → AppImage 需要 FUSE 2，用 `<程序路径> --appimage-extract` 解包后运行其中的 `AppRun`。**正常打开了** → 程序本身没问题，若只在游戏模式失败，见「非 Steam 程序接入游戏模式」。"),
            Step.Read(
                "找出缺少哪些库",
                "ldd <程序路径> | grep 'not found'",
                "对 AppImage 无效（它是一个包装），只适用于普通可执行文件。").Then("每个缺失的库一行，如 `libxxx.so.1 => not found`。这类程序在 SteamOS 上最省事的办法是改用 AppImage 版本，或放到 Distrobox 容器中安装运行。"),
        ],

        Verify =
        [
            "• 在终端运行时窗口正常打开、终端里没有报错；双击或从 Steam 启动同样正常。",
        ],

        Related = ["portable-apps", "man-ldd-elf", "non-steam-games", "distrobox", "man-chmod-chown"],
    };
}
