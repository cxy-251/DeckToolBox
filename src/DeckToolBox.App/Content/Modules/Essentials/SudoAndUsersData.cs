using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class SudoAndUsersData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "sudo-users",
        Title = "sudo、密码与用户",
        Keywords = "passwd · sudo · id · wheel",
        Level = ModuleLevel.Basic,
        Overview = "Linux 把普通用户和管理员 (root) 严格分开。sudo 让普通用户临时以 root 身份执行单条命令。在 Steam Deck 上，第一次使用 sudo 之前必须先给 deck 用户设置密码。",

        Why =
        [
            "1. 用户、组与 root：",
            "• 每个用户有一个数字 ID (UID)，root 的 UID 是 0，可以做任何事。你的日常账户（Deck 上是 `deck`，UID 1000）只能修改自己的文件。",
            "• 组 (group) 用来批量授权。能使用 sudo 的用户通常在 `wheel` 组（Arch 系、SteamOS）或 `sudo` 组（Debian / Ubuntu）中。",
            "2. sudo 的工作方式：",
            "• `sudo 命令` 会要求输入**你自己的**密码（不是 root 的密码），验证后以 root 身份执行这一条命令。",
            "• 验证通过后几分钟内再用 sudo 不必重复输入密码。",
            "• 输入密码时屏幕上**什么都不显示**（没有星号），这是正常的，输完直接回车。",
            "3. Steam Deck 的特殊之处：出厂时 `deck` 用户没有密码，因此 `sudo` 一定失败。需要先在桌面模式终端执行 `passwd` 设置一个。",
            "4. 为什么不一直用 root：root 下的一次手误（例如路径写错的 `rm -r`）可以破坏整个系统。只在确有需要时对单条命令使用 sudo，是最基本的安全习惯。",
        ],

        Consequences =
        [
            "• `passwd` 修改的是 deck 用户的密码，系统更新不会重置它。**务必记住**：忘记密码后只能通过恢复镜像等方式重置。",
            "• 设置密码后，SSH 登录、某些系统设置也会使用这个密码。",
            "• 不要执行来源不明、要求 sudo 的命令或脚本；带 sudo 的指令在本应用中都有「需要 sudo」标记。",
            "• 不要用 `sudo` 运行图形程序或修改家目录里的文件，否则会产生属于 root 的文件，之后你自己的程序无法再写入它们。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前用户与所属组",
                "id",
                "包含 `wheel` 说明该用户允许使用 sudo。").Then("输出一行，如 `uid=1000(deck) gid=1000(deck) 组=1000(deck),998(wheel)`。"),
            Step.Read(
                "查看当前用户是否已设置密码",
                "passwd -S",
                "只能查询自己的账户。").Then("输出一行，如 `deck P 2025-05-27 0 99999 7 -1`：第二列 `P` 表示已设置密码，`NP` 表示没有密码，`L` 表示已锁定；第三列是上次修改日期。"),
            Step.Change(
                "为 deck 用户设置或修改密码",
                "passwd",
                "为当前用户设置登录密码。").Then("已有密码时先提示输入当前密码，然后两次输入新密码；成功后显示 `passwd：已成功更新密码`。输入过程中屏幕不显示任何字符。"),
            Step.Root(
                "测试 sudo 是否可用",
                "sudo -v && echo 'sudo 可用'",
                "`-v` 只验证密码，不执行任何命令；验证后几分钟内再用 sudo 不必重新输入。").Then("提示 `[sudo] deck 的密码：`，输入正确后打印「sudo 可用」；密码错误显示「抱歉，请重试」；从未设置过密码时会一直失败。"),
            Step.Root(
                "查看自己被允许用 sudo 执行哪些命令",
                "sudo -l",
                "需要先输入密码。").Then("先显示默认设置（如 `secure_path=...`），最后一行 `(ALL) ALL` 表示可以以任何身份执行任何命令。"),
            Step.Root(
                "修复被 root 占用的家目录文件",
                "sudo chown -R \"$USER\":\"$USER\" <路径>",
                "当你曾用 sudo 在家目录里创建过文件时使用。只对自己家目录下的路径执行。").Then("没有输出。该路径及其下所有文件的所有者改回你自己，之前「权限不够」的程序可以正常读写了。"),
        ],

        Verify =
        [
            "1. `passwd -S` 的第二列为 `P`。",
            "2. `sudo -v` 输入密码后打印「sudo 可用」；`sudo whoami` 输出 `root`。",
        ],
    };
}
