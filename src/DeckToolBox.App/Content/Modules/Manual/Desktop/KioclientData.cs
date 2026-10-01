using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Desktop;

public static class KioclientData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-kioclient",
        Title = "kioclient：用 KDE 的方式操作文件",
        Keywords = "kioclient · KIO · trash:/ · smb:// · sftp:// · desktop:/",
        Level = ModuleLevel.Intermediate,
        Overview = "Dolphin 能直接浏览回收站、SMB 共享、SFTP、手机（MTP）等位置，靠的是 KDE 的 KIO 框架。kioclient 让你在命令行中使用同一套能力：对这些特殊地址执行复制、移动、列出、读取，与 Dolphin 中的操作效果一致。",

        Why =
        [
            "1. KIO 地址：普通路径写作 `file:///home/deck/...`（也可以直接写本地路径），另外支持：",
            "- `trash:/`：回收站。",
            "- `desktop:/`：桌面目录。",
            "- `smb://主机/共享`：Windows/NAS 共享。",
            "- `sftp://用户@主机/路径`：通过 SSH 访问的远程文件。",
            "- `mtp:/`：USB 连接的安卓手机。",
            "2. 常用命令（`kioclient --commands` 列出全部）：",
            "- `ls 地址`：列出目录。",
            "- `cat 地址`：输出文件内容。",
            "- `copy 源 目标`、`move 源 目标`：复制、移动（`move 文件 trash:/` 即放入回收站）。",
            "- `remove 地址`：删除。",
            "- `exec 地址`：用默认程序打开。",
            "3. 与 Dolphin 共享登录信息：SMB、SFTP 的密码如果已在 Dolphin 中保存到 KWallet，kioclient 可以直接使用。",
            "4. `--overwrite`：目标已存在时覆盖；`--noninteractive`：不弹出对话框，适合脚本。",
        ],

        Consequences =
        [
            "• `ls`、`cat` 只读；`copy`、`move`、`remove` 会修改文件，与在 Dolphin 中操作相同。",
            "• `remove` 直接删除，不经过回收站；需要可恢复时用 `move 文件 trash:/`。",
            "• 访问远程地址时可能弹出登录对话框。",
        ],

        Steps =
        [
            Step.Read(
                "读取一个文件",
                "kioclient cat file:///etc/hostname",
                "效果与 `cat /etc/hostname` 相同。").Then("输出本机的主机名。"),
            Step.Read(
                "列出桌面目录与回收站",
                "kioclient ls desktop:/; echo ---; kioclient ls trash:/",
                "特殊地址也能像普通目录一样列出。").Then("先列出桌面上的条目（如 `Return.desktop`、`steam.desktop`），再列出回收站中的文件（名称前带数字前缀）。"),
            Step.Change(
                "把文件放进回收站并恢复",
                "echo 测试 > ~/kio-demo.txt && kioclient move ~/kio-demo.txt trash:/ && kioclient ls trash:/ | grep kio-demo && kioclient move 'trash:/0-kio-demo.txt' ~/kio-demo.txt && rm ~/kio-demo.txt",
                "放入回收站后立即移回，最后删除测试文件。").Then("中间输出 `0-kio-demo.txt`，说明它进入过回收站；之后被移回家目录并删除，回收站中不留痕迹。"),
            Step.Read(
                "列出 SMB 共享中的文件（示例）",
                "kioclient ls 'smb://<主机名或IP>/<共享名>'",
                "`<主机名或IP>` 为局域网服务器地址；`<共享名>` 可通过 `smbclient -L <主机名或IP> -N` 查得。首次访问弹出认证框。").Then("列出共享根目录中的文件与文件夹，与在 Dolphin 中打开该地址看到的一致。"),
            Step.Change(
                "从 SFTP 复制文件到本机（示例）",
                "kioclient copy 'sftp://<用户>@<主机>/home/<用户>/<文件>' ~/Downloads/",
                "`<用户>` 与 `<主机>` 为远端 SSH 用户名及 IP；`<文件>` 为远端对应路径下的文件。对方需开启 sshd。").Then("文件被复制到 `~/Downloads`；需要密码时弹出对话框。"),
        ],

        Verify =
        [
            "1. `kioclient cat desktop:/steam.desktop | head -n 3` 能输出桌面上 Steam 图标的内容。",
        ],
    };
}
