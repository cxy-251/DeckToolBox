using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Desktop;

public static class XdgMimeData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-xdg-mime",
        Title = "默认程序与 xdg-open",
        Keywords = "xdg-open · xdg-mime · mimeapps.list · xdg-user-dir",
        Level = ModuleLevel.Basic,
        Overview = "双击一个文件由哪个程序打开、点击网页链接用哪个浏览器，都由 MIME 类型与默认程序关联决定。xdg-open 按这套规则打开文件或网址，xdg-mime 查询和修改关联，xdg-user-dir 查询「下载」「图片」等标准目录的实际位置。这些是 freedesktop 标准，在各发行版和桌面上通用。",

        Why =
        [
            "1. MIME 类型：描述文件是什么，如 `text/plain`、`image/png`、`application/pdf`；网址协议也有对应的类型，如 `x-scheme-handler/https`（网页链接）、`x-scheme-handler/steam`。系统根据文件内容和扩展名判断类型。",
            "2. 默认程序：每个类型关联一个 `.desktop` 文件（即一个程序）。你的选择保存在 `~/.config/mimeapps.list`，优先于系统默认。",
            "3. 命令：",
            "- `xdg-open 文件或网址`：用默认程序打开，适合写在脚本中；在 KDE 上等同于 `kde-open`。",
            "- `xdg-mime query filetype 文件`：查询文件的 MIME 类型。",
            "- `xdg-mime query default 类型`：查询该类型的默认程序。",
            "- `xdg-mime default 程序.desktop 类型`：设置默认程序（写入 mimeapps.list）。",
            "4. 标准目录：`xdg-user-dir DOWNLOAD`、`PICTURES`、`DESKTOP`、`DOCUMENTS` 等返回实际路径，配置在 `~/.config/user-dirs.dirs`。脚本中用它代替写死的 `~/Downloads`，在中文目录名等情况下也能正确工作。",
            "5. `.desktop` 文件的名称可以在 `/usr/share/applications`、`~/.local/share/applications` 中查到（便携应用的菜单项见「XDG 目录与 .desktop 菜单」）。",
        ],

        Consequences =
        [
            "• 查询是只读的；`xdg-mime default` 修改的是你自己的关联，不影响其他用户，也可以在「系统设置 → 默认应用程序」「文件关联」中改回。",
            "• 把一个类型关联到不存在的 `.desktop` 文件，打开该类文件时会失败或回退到其他程序。",
        ],

        Steps =
        [
            Step.Read(
                "查询文件的类型与默认程序",
                "xdg-mime query filetype ~/.bashrc; xdg-mime query default text/plain",
                "先查类型，再查该类型的默认程序。").Then("输出 `text/plain` 与 `org.kde.kate.desktop`：文本文件默认用 Kate 打开。"),
            Step.Read(
                "查询几种常见类型的默认程序",
                "for t in x-scheme-handler/https application/pdf image/png video/mp4; do printf '%-26s %s\\n' \"$t\" \"$(xdg-mime query default $t)\"; done",
                "遍历查询 HTTPS 协议处理程序及常见音视频格式默认桌面项，读取 `mimeapps.list` 映射结果。").Then("每行「类型 默认程序」，如 `x-scheme-handler/https chrome.desktop`（网页用 Chrome）、`application/pdf okularApplication_pdf.desktop`、`image/png org.kde.gwenview.desktop`；空白表示没有设置。"),
            Step.Change(
                "用默认程序打开文件或网址",
                "xdg-open ~/Pictures; xdg-open https://store.steampowered.com",
                "目录用文件管理器打开，网址用默认浏览器打开。").Then("Dolphin 打开「图片」目录，默认浏览器打开 Steam 商店页面。"),
            Step.Change(
                "修改默认程序（示例：用 Kate 打开 JSON 文件）",
                "xdg-mime default org.kde.kate.desktop application/json && xdg-mime query default application/json",
                "类型用 `xdg-mime query filetype 某个.json` 查到。").Then("输出 `org.kde.kate.desktop`；之后在 Dolphin 中双击 `.json` 文件会用 Kate 打开。`~/.config/mimeapps.list` 中多了一行对应记录。"),
            Step.Read(
                "查询标准目录的实际路径",
                "for d in DESKTOP DOWNLOAD DOCUMENTS PICTURES MUSIC VIDEOS; do printf '%-10s %s\\n' $d \"$(xdg-user-dir $d)\"; done",
                "脚本中用它代替写死的路径。").Then("每行「名称 路径」，如 `DOWNLOAD /home/deck/Downloads`。"),
        ],

        Verify =
        [
            "1. `grep json ~/.config/mimeapps.list` 能看到刚才设置的关联。",
        ],
    };
}
