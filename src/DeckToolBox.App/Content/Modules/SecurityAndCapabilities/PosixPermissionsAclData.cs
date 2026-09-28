using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.SecurityAndCapabilities;

public static class PosixPermissionsAclData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "permissions-acl",
        Title = "文件权限与 ACL",
        Keywords = "chmod · umask · setfacl",
        Level = ModuleLevel.Intermediate,
        Overview = "Linux 用「所有者 / 所属组 / 其他人」三组读写执行权限控制文件访问，另有 SUID、SGID、粘滞位三个特殊位，以及可为任意用户单独授权的 ACL。理解目录权限与文件权限的区别，是排查「权限不够」问题的基础。",

        Why =
        [
            "1. 权限位：每个文件有 12 个权限位，`ls -l` 显示为 `-rwxr-xr-x` 这样的形式：",
            "- 所有者 (u)、所属组 (g)、其他人 (o) 各有 `r`（读，4）、`w`（写，2）、`x`（执行，1）三位，常用八进制表示，如 `755`、`644`。",
            "- 另有三个特殊位：SUID (4000)、SGID (2000)、粘滞位 (1000)。",
            "2. 对目录而言，r/w/x 的含义与文件不同：",
            "- `r`：可以列出目录中的文件名。",
            "- `w`：可以在目录中新建、删除、重命名文件。**即使某个文件本身只读，只要对其所在目录有写权限，就能删除它。**",
            "- `x`：可以进入目录、访问其中文件的属性和内容。没有 `x` 时，即使文件本身权限为 777 也无法访问。",
            "3. 特殊权限位：",
            "- SUID：运行该程序时以文件所有者的身份执行，例如 `passwd` 以 root 身份修改密码文件。",
            "- SGID（用于目录）：在其中新建的文件自动归属该目录的组，便于多人共享。",
            "- 粘滞位（用于目录）：所有人都能写入，但只能删除自己的文件，如 `/tmp`（`drwxrwxrwt`）。",
            "4. ACL：在三组权限之外，为指定的用户或组单独授权，用 `getfacl`/`setfacl` 管理。带 ACL 的文件在 `ls -l` 的权限末尾显示 `+`。",
            "5. umask：新建文件时要去掉的权限。默认 `022`，于是新目录为 755、新文件为 644。",
        ],

        Consequences =
        [
            "• 查看权限的命令只读。",
            "• 不要用 `chmod 777` 解决权限问题：任何程序都能修改其中的文件。先弄清是哪个用户、需要什么权限，再精确授权。",
            "• 对 `/etc`、`~/.ssh` 等目录递归 `chmod -R` 很容易出问题：例如 `~/.ssh` 权限过宽时，ssh 会拒绝使用其中的密钥。",
            "• `setfacl -b <文件>` 可以删除全部 ACL 条目，恢复为普通权限。",
        ],

        Steps =
        [
            Step.Read(
                "查看新建文件时的默认权限 (umask)",
                "umask -S && umask",
                "umask 表示新建文件时要去掉的权限位。").Then("输出两行：`u=rwx,g=rx,o=rx` 与 `0022`。意味着你新建的目录权限为 755、文件为 644：别人可以读，不能写。"),
            Step.Read(
                "查看 /tmp 的粘滞位",
                "ls -ld /tmp /var/tmp",
                "`-d` 显示目录本身而不是其内容。").Then("两行都以 `drwxrwxrwt` 开头：所有人可写，但末尾的 `t`（粘滞位）让每个人只能删除自己的文件。"),
            Step.Read(
                "列出带 SUID / SGID 位的程序",
                "find /usr/bin -type f -perm /6000 2>/dev/null | head -n 20",
                "`-perm /6000` 匹配带有 SUID (4000) 或 SGID (2000) 任一位的文件。").Then("每行一个程序路径，如 `/usr/bin/sudo`、`/usr/bin/passwd`、`/usr/bin/newgrp`。"),
            Step.Read(
                "查看一个文件的 ACL",
                "getfacl -p /dev/uinput",
                "`user:deck:rw-` 是 udev 为当前登录用户动态添加的 ACL（见「手柄输入链与 Steam Input」）。").Then("输出 `# file:`、`# owner: root`、`# group: root` 以及 `user::rw-`、`user:deck:rw-`、`group::rw-`、`mask::rw-`、`other::---` 等条目。"),
            Step.Change(
                "实验：为指定用户单独授权，再清除 ACL",
                "mkdir -p /tmp/acl-demo && setfacl -m u:nobody:rx /tmp/acl-demo && ls -ld /tmp/acl-demo && getfacl -p /tmp/acl-demo && setfacl -b /tmp/acl-demo && rmdir /tmp/acl-demo",
                "`-m u:nobody:rx` 只给 nobody 用户加读和进入权限；`-b` 删除全部扩展条目。").Then("`ls -ld` 显示 `drwxr-xr-x+`（末尾的 `+` 表示带 ACL），`getfacl` 中多出 `user:nobody:r-x`；随后 ACL 被清除、目录被删除。"),
        ],

        Verify =
        [
            "1. `stat -c '%a %n' ~/.bashrc` 输出 `644 /home/deck/.bashrc`。",
            "2. 创建一个目录并执行 `chmod 644 <目录>`（有读无执行），再 `cd` 进入会提示「权限不够」，`ls -l <目录>` 能列出文件名但属性全是 `?`，这就是目录 `x` 位的作用。",
        ],
    };
}
