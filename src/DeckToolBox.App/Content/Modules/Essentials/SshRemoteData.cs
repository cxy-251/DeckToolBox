using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Essentials;

public static class SshRemoteData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "ssh-remote",
        Title = "SSH 远程登录与传文件",
        Keywords = "sshd · ssh-keygen · scp · rsync · sshfs",
        Level = ModuleLevel.Basic,
        Overview = "开启 SSH 后，就能在电脑上登录 Deck 的终端：用真正的键盘打命令、复制粘贴，传文件也比 U 盘方便。反过来，Deck 也可以登录其他 Linux 机器。",

        Why =
        [
            "1. 服务端与客户端：`sshd` 是在被登录机器上运行的服务，监听 22 端口；`ssh` 是发起连接的客户端。Windows 10/11、macOS、各 Linux 发行版都自带 ssh 客户端。",
            "2. 两种登录方式：",
            "- 密码登录：输入对方账户的密码。Deck 必须先用 `passwd` 设置过密码（见「sudo、密码与用户」）。",
            "- 密钥登录：客户端生成一对密钥，把**公钥**放到服务端的 `~/.ssh/authorized_keys` 中，之后免密码登录，也更安全。私钥永远只留在自己的电脑上。",
            "3. 首次连接时会提示确认对方的「主机指纹」，确认后记录在 `~/.ssh/known_hosts`。以后指纹变化时 ssh 会发出警告，防止连到冒充的机器。",
            "4. 传文件的几种方式：",
            "- `scp`：简单复制，语法类似 `cp`。",
            "- `rsync -e ssh`：增量同步，适合大量文件或断点续传。",
            "- `sshfs`：把远程目录挂载成本地目录，在 Dolphin 里直接浏览。Dolphin 地址栏输入 `sftp://deck@<IP>/` 也能直接访问。",
        ],

        Consequences =
        [
            "• 开启 sshd 后，同一局域网内知道密码的人都能登录你的 Deck。使用强密码，或者只用密钥登录；不用时可以关闭服务。",
            "• `systemctl enable` 会让 sshd 开机自动启动；只想临时使用，用 `start` 代替 `enable --now`。",
            "• 不要把私钥（`~/.ssh/id_ed25519`，没有 `.pub` 后缀的那个）发给任何人或放进网盘。",
        ],

        Steps =
        [
            Step.Read(
                "查看 Deck 的 IP 地址",
                "ip -4 -br addr show wlan0",
                "其他设备用这个地址连接 Deck。").Then("输出一行，如 `wlan0  UP  192.168.0.21/24`，斜杠前是 IP。"),
            Step.Read(
                "查看 sshd 服务状态",
                "systemctl status sshd --no-pager",
                "`--no-pager` 直接输出。").Then("显示服务状态：`Active: active (running)` 表示正在运行；`inactive (dead)` 表示未启动。下方有最近几行日志。"),
            Step.Root(
                "启动 sshd 并设为开机自启",
                "sudo systemctl enable --now sshd",
                "需要已设置 deck 用户的密码。关闭用 `sudo systemctl disable --now sshd`。").Then("输入 sudo 密码后可能输出 `Created symlink ...`，sshd 立即启动，并且以后开机自动启动。"),
            Step.Read(
                "从另一台电脑登录 Deck",
                "ssh deck@<Deck的IP>",
                "`<Deck的IP>` 由 Deck 终端执行 `ip -br addr show wlan0` 查得。在你的电脑上执行（Windows 可用 PowerShell）。").On("另一台电脑").Then("首次连接显示主机指纹并询问 `Are you sure you want to continue connecting`，输入 `yes`；再输入 deck 用户的密码，提示符变为 `(deck@steamdeck ~)$`，之后输入的命令都在 Deck 上执行。`exit` 断开。"),
            Step.Change(
                "在电脑上生成密钥并把公钥发给 Deck",
                "ssh-keygen -t ed25519 && ssh-copy-id deck@<Deck的IP>",
                "`<Deck的IP>` 由 Deck 上执行 `ip -br addr` 查得。Windows 若无 `ssh-copy-id`，可手动把 `.pub` 内容追加到 Deck 的 `~/.ssh/authorized_keys`。").On("另一台电脑").Then("`ssh-keygen` 询问保存位置和口令（可直接回车），生成 `~/.ssh/id_ed25519` 与 `.pub` 两个文件；`ssh-copy-id` 要求输入一次 Deck 密码，提示 `Number of key(s) added: 1`。此后登录不再询问密码。"),
            Step.Change(
                "从电脑复制文件到 Deck",
                "scp <本地文件> deck@<Deck的IP>:~/Downloads/",
                "`<本地文件>` 为电脑端待发送文件的本地路径；`<Deck的IP>` 由 Deck 终端 `ip -br addr` 查得。反向传输：`scp deck@<Deck的IP>:~/<文件> .`。").On("另一台电脑").Then("显示文件名、进度百分比、大小、速度和剩余时间，完成后文件出现在 Deck 的 `~/Downloads/` 中。"),
            Step.Change(
                "把远程目录挂载到本地",
                "mkdir -p ~/remote && sshfs <用户名>@<对方IP>:/home/<用户名> ~/remote",
                "`<用户名>` 与 `<对方IP>` 分别替换为目标机器的系统登录用户名和 IP 地址（可由远端执行 `whoami` 与 `ip addr` 查得）。卸载：`fusermount3 -u ~/remote`。").Then("要求输入对方账户的密码（或使用密钥），没有其他输出。之后 `~/remote` 里显示的就是对方家目录的内容，可以用 Dolphin 直接浏览和拷贝。"),
        ],

        Verify =
        [
            "1. 在电脑上 `ssh deck@<Deck的IP>` 后，提示符变为 `(deck@steamdeck ~)$`。",
            "2. 配置密钥后再次登录，不再询问密码。",
            "3. 在 Deck 上执行 `journalctl -u sshd -n 20 --no-pager`，能看到对应的 `Accepted password` 或 `Accepted publickey` 登录记录。",
        ],
    };
}
