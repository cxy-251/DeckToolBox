using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class ArchivesBackupData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "archives-backup",
        Title = "压缩包与 rsync 备份",
        Keywords = "tar · zip · 7z · rsync",
        Level = ModuleLevel.Basic,
        Overview = "解压各种压缩包、把目录打包，以及用 rsync 把存档、配置备份到 SD 卡或移动硬盘。rsync 只复制有变化的部分，第二次备份几乎瞬间完成。",

        Why =
        [
            "1. 常见压缩格式与对应工具（SteamOS 均已自带）：",
            "- `.tar.gz` / `.tgz` / `.tar.xz` / `.tar.zst`：`tar`。tar 先把多个文件打成一个包，再用 gzip / xz / zstd 压缩。",
            "- `.zip`：`unzip` 解压，`zip` 压缩。",
            "- `.7z`、`.rar` 等：`7z`。",
            "2. tar 常用选项：`-c` 创建、`-x` 解压、`-t` 列出内容、`-f` 指定文件名、`-C` 指定目录；`-a` 根据扩展名自动选择压缩算法。",
            "3. rsync 的优势：",
            "• 比较源和目标，只传输新增和改动的文件，适合定期备份。",
            "• `-a` 归档模式：保留目录结构、权限和修改时间。",
            "• 源目录末尾**有没有斜杠**含义不同：`rsync -a 源/ 目标/` 复制源目录的内容；`rsync -a 源 目标/` 会在目标里再创建一层 `源` 目录。",
            "4. 值得备份的东西：`~/.local/share/Steam/userdata/`（Steam 本地配置、非 Steam 快捷方式）、各游戏的 Proton 前缀存档目录、`~/.config`、你自己的文档。",
        ],

        Consequences =
        [
            "• 解压会覆盖目标目录中的同名文件。先用 `-t` / `unzip -l` 看一下包里有什么，或者解压到一个新建的空目录。",
            "• rsync 默认只增不删；加 `--delete` 后，目标中「源里已经没有」的文件会被删除，让目标与源完全一致。**先加 `-n`（演练）看会删掉什么。**",
            "• 目标设备若是 exFAT，rsync 无法保留权限和符号链接，会有提示；备份到 ext4 格式的设备可完整保留。",
        ],

        Steps =
        [
            Step.Read(
                "不解压，先看压缩包里有什么",
                "tar -tf <文件>.tar.gz | head; unzip -l <文件>.zip | head; 7z l <文件>.7z | head",
                "三种格式各一条，按你手上的文件选用。"),
            Step.Change(
                "解压到指定目录",
                "mkdir -p ~/extracted && tar -xf <文件>.tar.gz -C ~/extracted",
                "`tar -xf` 能自动识别 gz/xz/zst 压缩。zip 用 `unzip <文件>.zip -d ~/extracted`，7z 用 `7z x <文件>.7z -o\"$HOME/extracted\"`。"),
            Step.Change(
                "把一个目录打包压缩",
                "tar -caf ~/backup-$(date +%F).tar.zst -C ~ Documents",
                "`-a` 根据扩展名 `.tar.zst` 选择 zstd 压缩，速度快、压缩率高。"),
            Step.Read(
                "演练：看看 rsync 将会复制什么",
                "rsync -avhn ~/Documents/ /run/media/deck/<卷标>/backup/Documents/",
                "`-n` 只演练不执行；`-v` 列出文件；`-h` 人类可读的大小。"),
            Step.Change(
                "执行备份",
                "rsync -avh --info=progress2 ~/Documents/ /run/media/deck/<卷标>/backup/Documents/",
                "`--info=progress2` 显示总体进度。再次执行时只复制变化的文件。"),
            Step.Danger(
                "镜像同步：让目标与源完全一致",
                "rsync -avh --delete ~/Documents/ /run/media/deck/<卷标>/backup/Documents/",
                "目标中多出来的文件会被删除。先把同样的命令加上 `-n` 执行一次，确认删除列表。"),
        ],

        Verify =
        [
            "1. `tar -tf ~/backup-*.tar.zst | head` 能列出 `Documents/` 下的文件。",
            "2. 再执行一次同样的 rsync 备份命令，输出里几乎没有文件被传输，说明增量同步生效。",
        ],
    };
}
