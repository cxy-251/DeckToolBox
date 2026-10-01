using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Content.Modules.Manual.Data;

public static class GitData
{
    public static ModuleDefinition Create() => new()
    {
        Id = "man-git",
        Title = "git：给配置文件和笔记做版本管理",
        Keywords = "init · add · commit · diff · log · restore · 回退",
        Level = ModuleLevel.Intermediate,
        Overview = "git 不只是程序员的工具。把配置文件、脚本、笔记所在的目录交给 git 管理，每次修改前后提交一次，就能随时看到改了什么、回到任意一个历史版本。SteamOS 自带 git，不需要联网或注册任何账号即可在本地使用。",

        Why =
        [
            "1. 仓库：执行 `git init` 的目录成为一个仓库，历史记录保存在其中的隐藏目录 `.git/` 里。删除 `.git` 就等于删除全部历史，文件本身不受影响。",
            "2. 三个区域：",
            "- 工作区：你正在编辑的文件。",
            "- 暂存区：`git add` 把改动放进来，准备提交。",
            "- 历史：`git commit` 把暂存区的内容记录为一个版本（提交），附带说明文字和时间。",
            "3. 常用命令：",
            "- `git status`：哪些文件被修改、哪些还没被管理。",
            "- `git diff`：工作区与上次提交的差异（统一 diff 格式，见「diff 与 cmp」）。",
            "- `git log --oneline`：历史提交列表，每行一个版本的编号与说明。",
            "- `git restore 文件`：放弃工作区中对该文件的修改；`git restore --source=编号 文件` 把文件恢复成某个历史版本的样子。",
            "- `git show 编号`：查看某次提交改了什么。",
            "4. 首次使用前设置提交者信息：`git config --global user.name` 与 `user.email`，只用于记录在提交中，不会发送到任何地方。",
            "5. `.gitignore`：列出不需要管理的文件（缓存、日志、含密码的文件）。",
        ],

        Consequences =
        [
            "• `git add`、`commit`、`log`、`diff` 不会改动你的文件内容。",
            "• **`git restore 文件` 会丢弃该文件尚未提交的修改**，无法撤销；不确定时先 `git diff` 看看会丢掉什么。",
            "• 不要把密码、密钥提交进仓库；以后上传到网上时它们会随历史一起公开。",
        ],

        Steps =
        [
            Step.Read(
                "查看是否已设置提交者信息",
                "git config --global user.name; git config --global user.email",
                "没有输出表示尚未设置。").Then("输出你设置过的名字与邮箱；为空时用 `git config --global user.name '<名字>'` 与 `user.email` 设置一次。"),
            Step.Change(
                "把一个目录变成仓库并提交第一个版本",
                "mkdir -p /tmp/git-demo && cd /tmp/git-demo && git init -q -b main && echo 'volume=50' > app.conf && git add app.conf && git commit -qm '初始配置' && git log --oneline",
                "`-b main` 指定默认分支名，`-q` 减少输出。").Then("输出一行 `e278c8d 初始配置`：第一个版本已记录（编号每次不同）。").Practice(),
            Step.Change(
                "修改文件并查看差异",
                "sed -i 's/50/80/' app.conf && git status --short && git diff",
                "`--short` 用两个字符表示状态，` M` 为已修改。").Then("先输出 ` M app.conf`，然后是统一 diff：`-volume=50` 与 `+volume=80`。").Practice(),
            Step.Change(
                "提交修改",
                "git commit -qam '音量改为 80' && git log --oneline",
                "`-a` 自动加入所有已被管理且修改过的文件，`-m` 写说明。").Then("两行历史，最新的 `音量改为 80` 在最上面。").Practice(),
            Step.Change(
                "把文件恢复成上一个版本的样子",
                "git restore --source=HEAD~1 app.conf && cat app.conf",
                "`HEAD~1` 表示当前提交的上一个；也可以写 `git log` 中的编号。").Then("输出 `volume=50`：文件内容回到了第一个版本，历史记录本身不变。").Practice(),
            Step.Change(
                "放弃恢复，回到最新版本",
                "git restore app.conf && cat app.conf",
                "丢弃工作区中尚未提交的修改。").Then("输出 `volume=80`。").Practice(),
            Step.Read(
                "查看某次提交的详情",
                "git log --format='%h %an %ar %s'; git show --stat HEAD",
                "`%h` 短编号、`%an` 作者、`%ar` 相对时间、`%s` 说明。").Then("每行显示一个版本的编号、作者、多久以前、说明；`git show --stat` 列出最近一次提交修改了哪些文件、增删了几行。"),
        ],

        Verify =
        [
            "1. `git log --oneline | wc -l` 输出 2，说明有两个版本。",
            "2. 练习结束后执行 `cd ~ && rm -rf /tmp/git-demo` 清理（`-f` 用于删除 `.git` 中的只读文件）。",
        ],
    };
}
