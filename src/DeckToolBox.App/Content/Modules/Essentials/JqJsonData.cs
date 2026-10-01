using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class JqJsonData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "jq-json",
        Title = "JSON 输出与 jq",
        Keywords = "jq · ip -j · lsblk -J · findmnt -J · sqlite3",
        Level = ModuleLevel.Intermediate,
        Overview = "很多现代命令能输出 JSON：ip、lsblk、findmnt、busctl、systemctl……再用系统自带的 jq 提取字段，比用 awk 切列更可靠，列顺序变了、字段里有空格也不会出错。",

        Why =
        [
            "1. 哪些命令能输出 JSON：",
            "- `ip -j addr`、`ip -j route`：网络接口与路由。",
            "- `lsblk -J`、`findmnt -J`：存储设备与挂载。",
            "- `busctl --json=short`、`systemctl --output=json`：D-Bus 与 systemd。",
            "- `pw-dump`：PipeWire 的完整对象图。",
            "2. jq 的基本写法：",
            "- `.`：原样美化输出，最常用的第一步。",
            "- `.[]`：展开数组的每个元素；`.name`：取字段。",
            "- `select(条件)`：过滤；`\\(.字段)`：在字符串中插入字段值。",
            "- `-r`：输出纯文本（去掉引号），方便继续交给其他命令。",
            "- `..`：递归遍历所有层级，适合嵌套结构（例如 lsblk 的分区在磁盘的 `children` 里）。",
            "3. sqlite3：很多程序用 SQLite 文件保存数据（浏览器历史、KDE 的部分数据库等）。`sqlite3 <文件> '.tables'` 可以查看结构。**只读查看时先复制一份**，避免与正在运行的程序争用文件。",
        ],

        Consequences =
        [
            "• jq 只读取输入，不修改任何文件。",
            "• jq 表达式放在单引号里，避免 shell 展开其中的 `$`、`|`。",
            "• 用 sqlite3 修改其他程序的数据库可能破坏其数据，本模块只做查看。",
        ],

        Steps =
        [
            Step.Read(
                "美化查看一段 JSON",
                "ip -j addr | jq '.[1]'",
                "`.[1]` 取数组的第 2 个元素（从 0 开始计数）。").Then("以缩进、着色的格式显示第二个网卡（通常是 `wlan0`）的完整信息：`ifname`、`operstate`、`address`、`addr_info` 数组等。"),
            Step.Read(
                "提取：每个网卡的名称与 IPv4 地址",
                "ip -j -4 addr | jq -r '.[] | \"\\(.ifname)\\t\\(.addr_info[0].local)\"'",
                "`\\(...)` 在字符串中插入字段，`\\t` 是制表符，`-r` 输出不带引号的纯文本。").Then("每行「网卡名<Tab>IP」，如 `lo 127.0.0.1`、`wlan0 192.168.0.21`。"),
            Step.Read(
                "过滤：所有 ext4 分区及其挂载点",
                "lsblk -J -o NAME,SIZE,FSTYPE,MOUNTPOINTS | jq -r '.. | objects | select(.fstype? == \"ext4\") | \"\\(.name) \\(.size) \\(.mountpoints | join(\",\"))\"'",
                "`..` 递归遍历，无论分区嵌套在哪一层都能找到。").Then("每行「分区名 大小 挂载点」，如 `nvme0n1p8 466.3G /var/tmp,/var/log,...,/home`、`mmcblk0p1 953.6G /run/media/deck/<卷标>`。"),
            Step.Read(
                "统计：当前有多少个真实文件系统挂载点",
                "findmnt -J --real | jq '[.. | objects | select(.target?)] | length'",
                "`[...]` 把结果收集成数组，`length` 计数。").Then("输出一个数字，如 `18`。"),
            Step.Read(
                "查看 PipeWire 中所有音频输出设备的名称",
                "pw-dump | jq -r '.[] | select(.info.props.\"media.class\"? == \"Audio/Sink\") | .info.props.\"node.description\"'",
                "字段名中含点号时用引号包起来。").Then("每行一个输出设备名，如 `ACP/ACP3X/ACP6x Audio Coprocessor Speaker`、`... Headphones`；接上 HDMI 或蓝牙耳机后会多出相应的行。"),
            Step.Change(
                "查看一个 SQLite 数据库的表结构（示例）",
                "cp <数据库文件> /tmp/db-copy.sqlite && sqlite3 /tmp/db-copy.sqlite '.tables'",
                "`<数据库文件>` 换成要查看的 SQLite 文件。先复制再查看，原文件不受影响；进一步可用 `sqlite3 /tmp/db-copy.sqlite '.schema <表名>'` 查看表结构。").Then("列出数据库中的所有表名。原数据库文件不受影响。"),
        ],

        Verify =
        [
            "1. 第 2 步输出 `wlan0` 及其 IP，与 `ip -br addr` 显示的一致。",
            "2. 第 3 步列出的分区与 `lsblk -f` 中 FSTYPE 为 ext4 的行一致。",
        ],
    };
}
