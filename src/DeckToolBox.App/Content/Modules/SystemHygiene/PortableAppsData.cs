using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SystemHygiene;

public static class PortableAppsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "portable-apps",
        Title = "便携应用与 AppImage",
        Keywords = "~/Applications · AppImage",
        Level = ModuleLevel.Basic,
        Overview = "建立完全自包含的独立应用存放目录，解压即用、删除目录即卸载，摆脱包管理器束缚。",

        Why =
        [
            "1. 摆脱系统版本依赖：现代 Linux 掌机（如 SteamOS）采用不可变只读根分区。传统包管理器升级会被覆盖抹去，而 Flatpak 又过于封闭。自包含便携应用不受任何系统更新影响。",
            "2. 自包含与高内聚（AppImage / 静态二进制）：绿色应用将核心运行时库打包在自身内部，依靠 rpath 或 AppRun 引导，不污染系统全局动态链接库。",
            "3. 极简生命周期管理：安装只需解压到子文件夹，卸载只需直接 rm 对应文件夹，真正做到“零注册表、零深层残留、100% 掌控”。",
        ],

        Consequences =
        [
            "• 优势：应用完全存放在用户持久分区 (/home)，系统无论怎么 OTA 大版本更新，软件及用户数据永远安全无损。",
            "• 注意事项：绿色软件需要用户手动赋予执行权限 (chmod +x)；AppImage 格式若遇到缺少 libfuse 的系统，可使用 --appimage-extract-and-run 参数无阻运行。",
        ],

        Steps =
        [
            Step.Change(
                "创建标准便携应用根目录",
                "mkdir -p ~/Applications",
                "在用户持久分区建立集中管理的便携软件目录。"),
            Step.Change(
                "下载独立软件后以专属目录存放（以示例应用为例）",
                "mkdir -p ~/Applications/MyTool && chmod +x ~/Applications/MyTool/start.sh",
                "赋予执行权限，每个软件独立为子文件夹。"),
            Step.Read(
                "针对 AppImage 格式的原生免 FUSE 运行技巧",
                "./App.AppImage --appimage-extract-and-run",
                "直接在 /tmp 中自解包运行，避免底层内核 FUSE 驱动版本冲突。"),
        ],

        Verify =
        [
            "在终端执行检查指令：ls -la ~/Applications",
            "预期输出：看到结构整齐的各绿色应用子目录（如 Telegram, Discord, Chrome 等均独立成文件夹）。",
        ],
    };
}
