using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Media;

public static class Exiv2Data
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-exiv2",
        Title = "exiv2：照片元数据",
        Keywords = "EXIF · 拍摄时间 · GPS · 按日期重命名 · 删除元数据",
        Level = ModuleLevel.Basic,
        Overview = "手机和相机拍的照片里嵌有 EXIF 元数据：拍摄时间、设备型号、镜头参数，常常还有 GPS 位置。exiv2 可以查看、修改、删除这些信息，也能按拍摄时间批量重命名照片。分享照片前删除位置信息，是保护隐私的好习惯。",

        Why =
        [
            "1. 元数据的类型：Exif（拍摄参数、时间、GPS）、IPTC（标题、关键词、版权）、XMP（Adobe 扩展格式）。",
            "2. 查看：",
            "- `exiv2 照片`：摘要（尺寸、相机、拍摄时间、曝光等）。",
            "- `exiv2 -pa 照片`：列出全部元数据字段（Exif、IPTC、XMP）。",
            "- `exiv2 -pa -g GPS 照片`：只显示含 GPS 的字段。",
            "3. 修改：`-M'set 字段名 值'` 设置某个字段；`-M'del 字段名'` 删除某个字段。",
            "4. 删除：`exiv2 -d a 照片` 删除全部元数据（`a` = all）；`-d e` 只删 Exif。",
            "5. 按拍摄时间重命名：`exiv2 -r '%Y%m%d_%H%M%S' mv 照片...`，把文件名改为拍摄时间，方便按时间排序与去重。",
        ],

        Consequences =
        [
            "• 修改和删除直接写入照片文件，**不会保留备份**；处理重要照片前先复制一份。",
            "• 删除元数据后，相册软件无法再按拍摄时间排序这些照片（会改用文件修改时间）。",
            "• 按时间重命名时，若两张照片拍摄时间完全相同，exiv2 会询问是否覆盖；遇到提示时选择不覆盖。",
        ],

        Steps =
        [
            Step.Change(
                "准备一张练习照片",
                "rm -rf /tmp/exif-demo && mkdir -p /tmp/exif-demo && cd /tmp/exif-demo && ffmpeg -loglevel error -f lavfi -i testsrc2=size=320x240 -frames:v 1 p.jpg && exiv2 -M'set Exif.Image.Make DeckCam' -M'set Exif.Photo.DateTimeOriginal 2026:09:01 10:00:00' p.jpg",
                "先删除上次练习留下的目录，重复练习时结果才与下面的说明一致。用 ffmpeg 生成一张测试图，再写入相机名和拍摄时间。").Then("没有输出，`p.jpg` 中写入了两项 Exif 信息。").Practice(),
            Step.Read(
                "查看照片摘要",
                "exiv2 p.jpg",
                "换成你自己照片的路径同样适用。").Then("显示 `File size`、`Image size : 320 x 240`、`Camera make : DeckCam`、`Image timestamp : 2026:09:01 10:00:00` 等摘要。"),
            Step.Read(
                "列出全部元数据字段",
                "exiv2 -pa p.jpg",
                "每行：字段名、类型、长度、值。").Then("列出 `Exif.Image.Make  Ascii  8  DeckCam` 与 `Exif.Photo.DateTimeOriginal ... 2026:09:01 10:00:00` 等。"),
            Step.Read(
                "检查照片是否带 GPS 位置（示例）",
                "exiv2 -pa -g GPS ~/Pictures/<照片>.jpg",
                "`-g` 只显示字段名中含 GPS 的条目。").Then("手机拍的照片通常列出 `Exif.GPSInfo.GPSLatitude`、`GPSLongitude` 等经纬度；没有输出表示不含位置。"),
            Step.Change(
                "按拍摄时间重命名",
                "exiv2 -r '%Y%m%d_%H%M%S' mv p.jpg && ls",
                "格式符与 date 命令相同。").Then("`ls` 显示 `20260901_100000.jpg`。").Practice(),
            Step.Change(
                "删除全部元数据（分享前）",
                "exiv2 -d a *.jpg && exiv2 -pa *.jpg | wc -l",
                "`-d a` 删除 Exif、IPTC、XMP 全部内容。").Then("最后输出 `0`：照片中已不含任何元数据，图像本身不变。").Practice(),
        ],

        Verify =
        [
            "1. 删除前后分别执行 `ls -l`，文件大小略有减小。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/exif-demo` 清理。",
        ],
    };
}
