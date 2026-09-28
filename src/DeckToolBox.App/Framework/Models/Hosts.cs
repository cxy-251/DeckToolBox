namespace DeckToolBox.App.Framework.Models;

/// <summary>
/// 指令执行环境的标准名称。发行版指令都可以在 SteamOS 上通过 Distrobox 容器实操
/// （各发行版模块的第一步会给出创建对应容器的命令）。
/// </summary>
public static class Hosts
{
    public const string SteamLaunchOptions = "Steam 启动选项";
    public const string SteamClient = "Steam 客户端界面";

    public const string Debian = "Debian / Ubuntu";
    public const string Fedora = "Fedora / RHEL";
    public const string Arch = "Arch Linux";
    public const string OpenSuse = "openSUSE";
    public const string Alpine = "Alpine";
    public const string Nix = "装有 Nix 的系统";
    public const string Silverblue = "Fedora Atomic (Silverblue)";
    public const string Distrobox = "Distrobox 容器内";
}
