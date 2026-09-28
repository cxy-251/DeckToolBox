using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Files;

public static class ChmodChownData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-chmod-chown",
        Title = "chmod 与 chown：修改权限与所有者",
        Keywords = "chmod +x · 755 · 644 · u=rw · chown · -R",
        Level = ModuleLevel.Basic,
        Overview = "chmod 修改文件的读、写、执行权限，chown 修改文件的所有者和组。最常见的场景是：下载的脚本或程序无法运行（缺少执行权限），以及用 sudo 创建的文件自己无法修改（所有者变成了 root）。",

        Why =
        [
            "1. chmod 的两种写法：",
            "• 符号写法：`谁` + `操作` + `权限`。谁：`u` 所有者、`g` 组、`o` 其他人、`a` 所有人；操作：`+` 加、`-` 去、`=` 设为；权限：`r`、`w`、`x`。例如 `chmod +x 脚本`、`chmod go-w 文件`、`chmod u=rw,go=r 文件`。",
            "• 数字写法：r=4、w=2、x=1，三组分别相加。常用值：",
            "- `755`：所有者可读写执行，其他人可读可执行（程序、脚本、目录）。",
            "- `644`：所有者可读写，其他人只读（普通文件）。",
            "- `700` / `600`：只有所有者能访问（私密目录 / 文件，如 `~/.ssh`）。",
            "2. `-R`：递归修改目录及其全部内容。",
            "3. chown：`chown 用户:组 文件`；`chown 用户: 文件` 表示组设为该用户的默认组。修改为其他用户需要 sudo。",
            "4. 权限含义的细节（目录的 r/w/x、特殊权限位、ACL）见「文件权限与 ACL」模块。",
        ],

        Consequences =
        [
            "• 修改立即生效，可以随时改回，不会改变文件内容。",
            "• 不要用 `chmod -R 777` 解决问题：这会让任何程序都能修改这些文件，而且会给所有文件加上执行权限。",
            "• 对 `~/.ssh` 放宽权限后，ssh 会拒绝使用其中的私钥（报 `Permissions ... are too open`）。",
            "• 修改系统目录（`/etc`、`/usr`）的权限或所有者可能导致系统功能异常；SteamOS 的 `/usr` 本身只读。",
        ],

        Steps =
        [
            Step.Change(
                "准备一个脚本",
                "mkdir -p /tmp/perm-demo && cd /tmp/perm-demo && printf '#!/bin/bash\\necho 脚本运行了\\n' > hello.sh && ls -l hello.sh",
                "新建的文件默认没有执行权限。").Then("显示 `-rw-r--r-- ... hello.sh`：没有 `x`。"),
            Step.Read(
                "尝试运行没有执行权限的脚本",
                "./hello.sh",
                "`./` 表示当前目录下的文件。").Then("报错 `bash: ./hello.sh: 权限不够`。"),
            Step.Change(
                "加上执行权限后运行",
                "chmod +x hello.sh && ls -l hello.sh && ./hello.sh",
                "`+x` 为所有人加上执行权限（受 umask 限制）。").Then("权限变为 `-rwxr-xr-x`，然后输出「脚本运行了」。"),
            Step.Change(
                "用数字设为只有自己可读写",
                "chmod 600 hello.sh && ls -l hello.sh",
                "`600` = 所有者 rw、组与其他人无权限。").Then("权限变为 `-rw-------`。"),
            Step.Change(
                "用符号写法精确设置",
                "chmod u=rwx,go=rx hello.sh && ls -l hello.sh",
                "等同于 `chmod 755`。").Then("权限变为 `-rwxr-xr-x`。"),
            Step.Change(
                "递归去掉组和其他人的写权限",
                "chmod -R go-w /tmp/perm-demo && ls -ld /tmp/perm-demo",
                "`-R` 作用于目录及其所有内容。").Then("没有报错；目录与其中所有文件的组、其他人都不再有 `w`。"),
            Step.Root(
                "把被 root 占用的文件改回自己所有",
                "sudo chown -R \"$USER\": <路径>",
                "`\"$USER\":` 表示所有者和组都改为当前用户。只对自己家目录下的路径执行。").Then("输入 sudo 密码后没有输出，`ls -l` 中所有者变为 `deck deck`，之后不用 sudo 也能修改这些文件。"),
        ],

        Verify =
        [
            "1. `stat -c '%a %n' /tmp/perm-demo/hello.sh` 输出 `755 /tmp/perm-demo/hello.sh`。",
            "2. 练习结束后执行 `cd ~ && rm -r /tmp/perm-demo` 清理。",
        ],
    };
}
