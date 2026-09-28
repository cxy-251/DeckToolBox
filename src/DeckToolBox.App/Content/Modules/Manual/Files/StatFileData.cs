using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class StatFileData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-stat-file",
        Title = "stat 与 file：查看文件详情与类型",
        Keywords = "stat · file · 时间戳 · 文件类型",
        Level = ModuleLevel.Basic,
        Overview = "stat 显示一个文件的全部元数据：大小、权限、所有者、inode 和四种时间。file 通过读取文件开头的内容判断它到底是什么类型，而不看扩展名。下载的文件打不开、扩展名可疑时，先用 file 看一眼。",

        Why =
        [
            "1. stat 显示的四种时间：",
            "- 访问时间 (atime)：最后一次读取。SteamOS 以 `relatime` 挂载，只在特定条件下更新，参考价值有限。",
            "- 修改时间 (mtime)：内容最后一次改变，`ls -l` 显示的就是它。",
            "- 变更时间 (ctime)：内容或属性（权限、所有者、链接数）最后一次改变。",
            "- 创建时间 (birth)：文件创建的时间，ext4 和 btrfs 支持。",
            "2. `stat -c` 自定义输出：`%a` 八进制权限、`%U` 所有者、`%s` 字节数、`%y` 修改时间、`%n` 文件名。",
            "3. file 的原理：根据文件开头的特征字节（magic number）识别格式，例如 `ELF` 表示 Linux 可执行程序、`PK` 开头是 zip、`#!/bin/bash` 开头是 shell 脚本。扩展名改成什么都不影响判断结果。",
            "4. `file -i` 显示 MIME 类型与字符编码，可以判断一个文本文件是 UTF-8 还是其他编码。",
        ],

        Consequences =
        [
            "• stat 与 file 都是只读的。",
            "• file 对无法识别的数据显示 `data`；识别结果是推断，不是绝对保证。",
        ],

        Steps =
        [
            Step.Read(
                "查看文件的全部元数据",
                "stat ~/.bashrc",
                "对目录同样适用。").Then("显示 `文件`、`大小`、`块`、`Inode`、`硬链接`、`权限：(0644/-rw-r--r--)`、`Uid`/`Gid`，以及访问、修改、变更、创建四个时间。"),
            Step.Read(
                "只取需要的字段",
                "stat -c '%a %U %s %y %n' ~/.bashrc ~/.bash_profile",
                "`-c` 自定义格式，每个文件一行。").Then("每行形如 `644 deck 695 2026-09-28 18:08:... /home/deck/.bashrc`。"),
            Step.Read(
                "判断文件类型",
                "file /usr/bin/ls /usr/bin/steamos-session-select ~/.bashrc ~/Pictures",
                "可以一次查看多个文件。").Then("依次显示：`ELF 64-bit LSB pie executable`（程序）、`Bourne-Again shell script`（脚本）、`ASCII text` 或 `Unicode text, UTF-8 text`（文本）、`directory`（目录）。"),
            Step.Read(
                "查看文本文件的编码",
                "file -i ~/.bashrc",
                "`-i` 输出 MIME 类型与字符集。").Then("输出 `text/plain; charset=utf-8`（文件中含中文等非英文字符时）或 `charset=us-ascii`（只有英文字符时）。"),
            Step.Read(
                "检查下载目录中各文件的真实类型",
                "file ~/Downloads/* | head -n 20",
                "不看扩展名，只看内容。").Then("每个文件一行，如 `game.zip: Zip archive data`、`video.mp4: ISO Media, MP4 Base Media`；扩展名与实际类型不符的文件很容易看出来。"),
        ],

        Verify =
        [
            "1. 执行 `cp ~/.bashrc /tmp/test.png && file /tmp/test.png`，仍显示为文本（`ASCII text` 或 `UTF-8 text`）而不是图片：file 不受扩展名影响。",
        ],
    };
}
