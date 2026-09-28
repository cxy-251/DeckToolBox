using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Disk;

public static class GocryptfsData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-gocryptfs",
        Title = "gocryptfs：加密文件夹",
        Keywords = "gocryptfs -init · 挂载 · fusermount3 -u · 主密钥 · Plasma Vault",
        Level = ModuleLevel.Intermediate,
        Overview = "gocryptfs 把一个普通目录变成加密保险箱：磁盘上只存加密后的文件，输入密码挂载后才能在另一个目录中看到原文。它不需要 root，加密后的目录可以放进网盘或 SD 卡同步。KDE 的「Plasma 保险库」图形功能也可以使用它作为后端。",

        Why =
        [
            "1. 两个目录：",
            "- 密文目录 (cipher)：实际保存在磁盘上的内容，文件名和内容都被加密，还包含配置文件 `gocryptfs.conf`。",
            "- 明文目录 (plain)：挂载点，挂载后在这里读写，gocryptfs 在后台实时加密、解密。未挂载时它是空的。",
            "2. 按文件加密：每个文件单独加密，修改一个文件只改变对应的密文文件，适合配合网盘、rsync 做增量同步。",
            "3. 主密钥 (master key)：初始化时生成。在终端中运行 `-init` 会显示一次主密钥，**务必抄下来保存**；忘记密码时可以凭主密钥恢复数据。",
            "4. 常用命令：",
            "- `gocryptfs -init 密文目录`：初始化，设置密码。",
            "- `gocryptfs 密文目录 明文目录`：挂载。",
            "- `fusermount3 -u 明文目录`：卸载。",
            "- `gocryptfs -passwd 密文目录`：修改密码。",
            "5. 需要加密整个分区或 U 盘时，使用 LUKS（`cryptsetup`，需要 root），SteamOS 同样自带。",
        ],

        Consequences =
        [
            "• **忘记密码且没有保存主密钥时，数据无法恢复。**",
            "• 挂载期间，任何以你身份运行的程序都能读取明文目录；离开时及时卸载。",
            "• 不要直接修改或删除密文目录中的文件（尤其是 `gocryptfs.conf` 和 `gocryptfs.diriv`），否则对应数据会损坏。",
            "• 重启或注销后挂载自动消失，需要重新输入密码挂载。",
        ],

        Steps =
        [
            Step.Change(
                "创建两个目录并初始化加密",
                "mkdir -p ~/Vault/cipher ~/Vault/plain && gocryptfs -init ~/Vault/cipher",
                "按提示输入两次密码。").Then("提示 `Choose a password for protecting your files.`，输入密码后显示一段 `Your master key is:` 与一串 64 位十六进制主密钥（请抄下保存），最后 `The gocryptfs filesystem has been created successfully.`。"),
            Step.Change(
                "挂载保险箱",
                "gocryptfs ~/Vault/cipher ~/Vault/plain",
                "输入密码。").Then("输出 `Decrypting master key` 和 `Filesystem mounted and ready.`；`~/Vault/plain` 现在可以正常使用。"),
            Step.Change(
                "放入文件，对比明文与密文",
                "echo 私密内容 > ~/Vault/plain/note.txt && ls ~/Vault/plain; ls ~/Vault/cipher",
                "在明文目录中操作。").Then("明文目录中是 `note.txt`；密文目录中是 `gocryptfs.conf`、`gocryptfs.diriv` 和一个形如 `-O7QAQd22tdT_t_xoqsy3w` 的加密文件名。"),
            Step.Change(
                "卸载",
                "fusermount3 -u ~/Vault/plain && ls ~/Vault/plain",
                "卸载后明文目录变为空目录。").Then("`ls` 没有输出：没有密码就只能看到密文目录中的乱码文件。"),
            Step.Change(
                "修改密码",
                "gocryptfs -passwd ~/Vault/cipher",
                "需在未挂载状态下执行，依次输入旧密码和两次新密码。").Then("提示 `Password changed.`，数据无需重新加密。"),
        ],

        Verify =
        [
            "1. 再次挂载后 `cat ~/Vault/plain/note.txt` 输出「私密内容」。",
            "2. `mount | grep Vault` 在挂载时能看到一条 `fuse.gocryptfs` 记录，卸载后消失。",
        ],
    };
}
