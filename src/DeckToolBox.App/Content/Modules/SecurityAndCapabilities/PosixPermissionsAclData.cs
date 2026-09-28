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
        Overview = "深入 Linux 自主访问控制 (DAC) 底层体系，透视 Inode 12 位权限结构、目录与文件读写执行的本质差异、SUID/SGID/Sticky 特殊位与 POSIX 访问控制列表 (ACL)。",

        Why =
        [
            "1. Inode 中的 12 位权限位图 (mode_t)：",
            "• Linux 文件系统中的每个文件与目录由 Inode 唯一标识，其元数据结构中的 `i_mode` 字段包含 12 个关键权限位：",
            "  - **高 3 位：特殊权限位**（4 = SUID, 2 = SGID, 1 = Sticky Bit）。",
            "  - **中 3 位：属主权限 (User)**（4 = r, 2 = w, 1 = x）。",
            "  - **次 3 位：属组权限 (Group)**（4 = r, 2 = w, 1 = x）。",
            "  - **低 3 位：其他用户权限 (Other)**（4 = r, 2 = w, 1 = x）。",
            "2. 目录 vs 文件的 r/w/x 语义本质差异（常见踩坑点）：",
            "• **对普通文件**：`r` 允许读取内容；`w` 允许写入或截断文件；`x` 允许发起 `execve` 作为二进制或脚本执行。",
            "• **对目录（核心考点）**：",
            "  - `r` (Read)：仅允许列出目录包含的文件名列表 (`ls` / `readdir` 系统调用)。若没有 `x`，无法获取文件的详细属性 (stat)。",
            "  - `w` (Write)：**具有在目录内创建、重命名、删除任意文件的绝对权力**！即使目录下某个文件是只读的，只要上级目录有 `w` 权限，该文件就能被直接删除！",
            "  - `x` (Execute)：允许进入该目录 (`cd` / `chdir`) 以及读取其下文件/子目录的 Inode 元数据。没有目录的 `x` 权限，即便子文件设置了 777 也无法访问！",
            "3. 三大特殊权限位的工程价值：",
            "• **SUID (SetUID, 4000)**：执行该二进制时，进程临时获得文件属主的有效 UID (如 passwd 命令)。",
            "• **SGID (SetGID, 2000)**：在目录上设置 SGID 时，任何人在该目录下新建的文件或子目录，其所属组将自动继承该父目录的组，实现团队无缝共享协作。",
            "• **Sticky Bit (粘滞位, 1000)**：常见于公共临时目录 `/tmp`（权限 `drwxrwxrwt`）。所有人都有写入权限，但**只有文件所有者本人或 root 才能删除或重命名该文件**，彻底防止恶意用户删掉他人的临时套接字或缓存。",
            "4. 突破三段式局限的 POSIX ACL (Access Control Lists)：",
            "• 传统 `u:g:o` 无法满足“只让 user_a 读写、user_b 只读、其余人拒绝”的复杂需求。POSIX ACL 扩展属性允许直接向文件绑定任意多个用户和组的权限条目，利用 `getfacl` 和 `setfacl` 实现细粒度赋权。",
        ],

        Consequences =
        [
            "• 严禁滥用 `chmod 777`：向目录赋予 777 会允许任何非可信进程任意修改、覆盖或清空其中的配置文件或动态库，且会在目录中引入恶意脚本执行漏洞。",
            "• 谨慎向全盘递归 `chmod -R`：递归修改系统目录（如 /etc、/usr 或 ~）会导致 SSH 密钥权限过宽被拒绝加载 (`Permissions 0777 are too open`)，或者导致 SUID 标志丢失而使 sudo 失效。",
            "• ACL 清理方法：通过 `setfacl -b <文件>` 可一键剥离所有扩展 ACL 规则，完全复原为标准 POSIX 基础权限。",
        ],

        Steps =
        [
            Step.Read(
                "查看当前 Shell 默认创建文件与目录的权限掩码 (umask)",
                "umask -S && umask",
                "umask 表示新建文件时要去掉的权限位。").Then("输出两行：`u=rwx,g=rx,o=rx` 与 `0022`。意味着你新建的目录权限为 755、文件为 644：别人可以读，不能写。"),
            Step.Read(
                "查看系统中关键特殊权限位目录（如 /tmp 粘滞位）",
                "ls -ld /tmp /var/tmp",
                "`-d` 显示目录本身而不是其内容。").Then("两行都以 `drwxrwxrwt` 开头：所有人可写，但末尾的 `t`（粘滞位）让每个人只能删除自己的文件。"),
            Step.Read(
                "查看系统中的 SUID / SGID 特权可执行程序清单",
                "find /usr/bin -type f -perm /6000 2>/dev/null | head -n 20",
                "`-perm /6000` 匹配带有 SUID (4000) 或 SGID (2000) 任一位的文件。").Then("每行一个程序路径，如 `/usr/bin/sudo`、`/usr/bin/passwd`、`/usr/bin/newgrp`。"),
            Step.Read(
                "使用 getfacl 深入查看文件的扩展 ACL 访问控制列表",
                "getfacl -p /dev/uinput",
                "`user:deck:rw-` 是 udev 为当前登录用户动态添加的 ACL（见「手柄输入链与 Steam Input」）。").Then("输出 `# file:`、`# owner: root`、`# group: root` 以及 `user::rw-`、`user:deck:rw-`、`group::rw-`、`mask::rw-`、`other::---` 等条目。"),
            Step.Change(
                "实验：为指定用户单独授权，再清除 ACL",
                "mkdir -p /tmp/acl-demo && setfacl -m u:nobody:rx /tmp/acl-demo && ls -ld /tmp/acl-demo && getfacl -p /tmp/acl-demo && setfacl -b /tmp/acl-demo && rmdir /tmp/acl-demo",
                "`-m u:nobody:rx` 只给 nobody 用户加读和进入权限；`-b` 删除全部扩展条目。").Then("`ls -ld` 显示 `drwxr-xr-x+`（末尾的 `+` 表示带 ACL），`getfacl` 中多出 `user:nobody:r-x`；随后 ACL 被清除、目录被删除。"),
        ],

        Verify =
        [
            "1. 八进制权限精确核验：执行 `stat -c \"%a %n\" ~/.bashrc`，预期输出为 `644 /home/deck/.bashrc`，验证属主读写、他人只读的健康状态。",
            "2. 目录遍历测试：若对目录设置 `chmod 644 dir`（有 r 无 x），执行 `cd dir` 会立即报错 `Permission denied`，执行 `ls -l dir` 会列出文件名但属性全为 `?`，亲手验证目录 x 位的实质。",
        ],
    };
}
