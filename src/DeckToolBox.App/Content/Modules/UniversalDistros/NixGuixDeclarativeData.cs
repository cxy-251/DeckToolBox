using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.UniversalDistros;

public static class NixGuixDeclarativeData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "nix-atomic",
        Title = "Nix 与原子化发行版",
        Keywords = "nix · NixOS · rpm-ostree · 回滚",
        Level = ModuleLevel.Advanced,
        Overview = "SteamOS 的「只读系统 + 整体更新」并非孤例。NixOS 用声明式配置和内容寻址存储走得更远，Fedora Atomic（Silverblue 等）用 OSTree 实现类似 Git 的系统版本管理。了解它们，就能理解不可变系统的共同思路。",

        Why =
        [
            "1. 传统发行版的问题：所有软件共享 `/usr/lib`。当两个程序需要同一个库的不同版本时，只能二选一；升级过程中途失败，系统可能处于半新半旧状态。",
            "2. Nix 的做法：",
            "• 每个包安装到独立目录 `/nix/store/<哈希>-<名称>-<版本>/`，哈希由它的全部构建输入（源码、依赖、编译参数）计算得出。",
            "• 同一个库的不同版本、不同编译参数都有各自的路径，可以同时存在；程序直接引用自己依赖的精确路径。",
            "• 用户环境、整个系统配置都是指向 store 的一组符号链接，称为「代」(generation)。升级就是生成新的一代并切换链接，回滚就是切回上一代。",
            "3. NixOS：整个系统（内核、服务、用户、软件）写在 `configuration.nix` 中，`nixos-rebuild switch` 构建并切换到新的一代；启动菜单可以选择任意历史代启动。",
            "4. Fedora Atomic（Silverblue / Kinoite / Bazzite 等）：",
            "• 系统由 OSTree 管理，像 Git 提交一样整体部署。`rpm-ostree install` 会在当前系统之上叠加一层包，生成新的部署，**重启后生效**。",
            "• 与 SteamOS 的相同点：只读系统、整体更新、可回滚到上一次部署；图形应用都推荐用 Flatpak。",
        ],

        Consequences =
        [
            "• Nix 从不覆盖旧文件，历史代会一直占用空间，需要定期垃圾回收。",
            "• NixOS 不遵循传统目录结构：没有 `/usr/lib`，只有 `/bin/sh`。从别处下载的预编译程序往往无法直接运行，需要 `nix-ld`、`steam-run` 或打补丁。",
            "• rpm-ostree 叠加的包越多，更新越慢、越容易冲突，官方建议尽量少用，优先 Flatpak 和容器（Toolbx / Distrobox）。",
            "• 本模块的命令需要在装有 Nix 或 Fedora Atomic 的系统上执行，SteamOS 默认不含这些工具，这里作为对照学习。",
        ],

        Steps =
        [
            Step.Read(
                "查看 store 中的包目录形态",
                "ls /nix/store | head -n 20",
                "每个目录名以 32 位哈希开头。").On(Hosts.Nix).Then("列出 20 个目录名，形如 `0a1b2c...-bash-5.2`：哈希、包名、版本。"),
            Step.Read(
                "临时进入一个带有指定工具的环境",
                "nix-shell -p cowsay jq",
                "工具只在这个 shell 中可用。").On(Hosts.Nix).Then("首次需要下载这些包，然后进入一个新 shell，其中 `cowsay`、`jq` 可用；`exit` 回到原 shell 后这两个命令不再可用。"),
            Step.Read(
                "查看用户环境的历史代",
                "nix-env --list-generations",
                "每次安装、删除都会生成新的一代。").On(Hosts.Nix).Then("每行一代：编号、创建时间，当前代后面标有 `(current)`。"),
            Step.Change(
                "回滚到上一代",
                "nix-env --rollback",
                "只切换符号链接，瞬间完成。").On(Hosts.Nix).Then("输出 `switching profile from version N to N-1`，用户环境立即变回上一代的软件集合。"),
            Step.Change(
                "删除旧的代并回收空间",
                "nix-collect-garbage -d",
                "`-d` 删除除当前代以外的所有旧代，之后将无法再回滚到它们。").On(Hosts.Nix).Then("列出被删除的旧代和 store 路径，最后显示释放了多少空间，如 `1234 store paths deleted, 2.1 GiB freed`。"),
            Step.Read(
                "查看 Fedora Atomic 的部署状态",
                "rpm-ostree status",
                "列出当前部署和上一个部署。").On(Hosts.Silverblue).Then("列出两个部署：带 `●` 的是当前启动的部署，另一个是回滚备用；每个部署显示版本号、提交哈希和叠加安装的包 (`LayeredPackages`)。"),
            Step.Root(
                "回滚到上一个部署",
                "sudo rpm-ostree rollback",
                "重启后生效。").On(Hosts.Silverblue).Then("提示已将默认启动的部署改为上一个，并建议重启；重启后系统回到上一次更新前的状态。"),
        ],

        Verify =
        [
            "1. 在 `nix-shell -p jq` 中执行 `command -v jq`，路径位于 `/nix/store/...`；退出后再执行，找不到 jq（或指向系统原有的版本）。",
            "2. `rpm-ostree status` 中带 `●` 的是当前启动的部署。",
            "3. 对照 SteamOS：`rauc status` 显示的 A/B 槽位，与 rpm-ostree 的「当前部署 / 上一个部署」是同一种思路。",
        ],
    };
}
