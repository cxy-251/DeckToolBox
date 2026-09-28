using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Network;

public static class LanShareData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-lan-share",
        Title = "局域网设备发现与 SMB 共享",
        Keywords = "avahi-browse · .local · smbclient · smb:// · mount.cifs",
        Level = ModuleLevel.Intermediate,
        Overview = "家里的 Windows 电脑、Mac、NAS 通常用 SMB 协议共享文件夹。SteamOS 自带 avahi 用来发现局域网中的设备和服务，smbclient 访问 SMB 共享，Dolphin 可以直接打开 smb:// 地址，需要长期挂载时用 mount.cifs。",

        Why =
        [
            "1. 服务发现 (mDNS/DNS-SD)：局域网中的设备会广播自己的名字和提供的服务（SMB 共享、SSH、打印机、AirPlay 等）。",
            "- `avahi-browse -art`：列出发现的所有服务后退出（`-a` 全部、`-r` 解析地址、`-t` 不持续等待）。",
            "- 设备名加 `.local` 后缀可以直接当主机名使用，如 `ping 我的电脑.local`。",
            "2. SMB（Windows 文件共享，macOS 与 NAS 也支持）：",
            "- `smbclient -L //主机 -U 用户名`：列出主机上的共享文件夹。",
            "- `smbclient //主机/共享名 -U 用户名`：进入类似 FTP 的交互界面，`ls`、`get 文件`、`put 文件`、`exit`。",
            "- Dolphin 地址栏输入 `smb://主机/共享名`，可以像本地文件夹一样浏览、拖放，这是最方便的方式。",
            "- `sudo mount -t cifs //主机/共享名 挂载点 -o username=用户,uid=$(id -u)`：挂载成本地目录，所有程序都能访问。",
            "3. 其他系统访问 Deck：在 Deck 上开启 SSH 后，Windows 可以用 WinSCP、macOS 与 Linux 的文件管理器可以用 `sftp://deck@<IP>` 访问（见「SSH 远程登录与传文件」）。",
        ],

        Consequences =
        [
            "• 发现与列出共享是只读的。",
            "• `smbclient` 的 `put`、在挂载目录中写入，会修改对方电脑上的文件。",
            "• 访问需要密码的共享时，密码不要直接写在命令行中；smbclient 会提示输入。",
        ],

        Steps =
        [
            Step.Read(
                "发现局域网中的设备与服务",
                "avahi-browse -art 2>/dev/null | grep '^=' | cut -c1-90 | sort -u | head -n 20",
                "只保留已解析的条目（以 `=` 开头），并去掉重复。").Then("每行形如 `=  wlan0 IPv4 <设备名>  _smb._tcp  local`：倒数第二列是服务类型，`_smb._tcp` 提供 SMB 共享、`_ssh._tcp`/`_sftp-ssh._tcp` 可以 SSH、`_ipp._tcp` 是打印机。"),
            Step.Read(
                "查看某个服务的地址",
                "avahi-browse -rt _smb._tcp 2>/dev/null | grep -E 'hostname|address' | sort -u",
                "只查找 SMB 服务。").Then("列出提供 SMB 共享的主机名（如 `xxx.local`）和 IP 地址。"),
            Step.Read(
                "列出一台电脑上的共享文件夹",
                "smbclient -L //<主机名或IP> -U <用户名>",
                "按提示输入该用户在对方电脑上的密码。").Then("表格列出 `Sharename Type Comment`，`Disk` 类型的就是可以访问的共享文件夹。"),
            Step.Change(
                "从共享文件夹下载一个文件",
                "smbclient //<主机名或IP>/<共享名> -U <用户名> -c 'ls; get <文件名>'",
                "`-c` 执行一串命令后退出；文件下载到当前目录。").Then("先列出共享根目录的文件，然后显示 `getting file ... as ...` 与传输速度。"),
            Step.Read(
                "在 Dolphin 中打开共享",
                "kde-open 'smb://<主机名或IP>/<共享名>'",
                "效果与在 Dolphin 地址栏输入该地址相同。").Then("Dolphin 打开并提示输入用户名与密码（可选择记住），随后像本地文件夹一样显示共享内容。"),
            Step.Root(
                "把共享挂载为本地目录（示例）",
                "sudo mkdir -p /mnt/share && sudo mount -t cifs //<主机名或IP>/<共享名> /mnt/share -o username=<用户名>,uid=$(id -u),gid=$(id -g)",
                "会提示输入共享的密码。卸载：`sudo umount /mnt/share`。").Then("`/mnt/share` 中显示共享内容，文件属于你，任何程序都可以读写。"),
        ],

        Verify =
        [
            "1. `ping -c 2 <设备名>.local` 能通，说明 mDNS 名称解析正常。",
        ],
    };
}
