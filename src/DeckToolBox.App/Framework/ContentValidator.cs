using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Framework;

/// <summary>
/// 内容静态检查：dotnet run -- --check
/// 保证每条指令的风险标记与实际行为一致，文本标记成对闭合，模块结构完整。
/// </summary>
public static partial class ContentValidator
{
    [GeneratedRegex(@"(^|[\s;&|(])sudo\s")]
    private static partial Regex SudoPattern();

    // 整条指令只是 echo '一段说明文字'，复制出去什么也做不了
    [GeneratedRegex(@"^\s*echo '[^']*'\s*$")]
    private static partial Regex PseudoEchoPattern();

    [GeneratedRegex(@"\brm\s+(-\w*[rf]\w*\s+)+")]
    private static partial Regex RecursiveRemovePattern();

    // 只读步骤中不应出现的修改操作（先去掉引号内的内容再匹配，避免 awk、SQL 中的 > 等误报）
    // 命令位置：行首，或 ; & | ( 之后，或 $( 之后
    private const string Cmd = @"(?:^|[;&|(\n]\s*|\$\(\s*)";
    [GeneratedRegex(Cmd + @"(touch|mkdir|rmdir|rm|mv|cp|ln|chmod|chown|chgrp|truncate|tee|install|dd|shred|reboot|poweroff)\s"
        + "|" + Cmd + @"(kill|pkill|killall)\s+(?!-[lL]\b)"
        + "|" + Cmd + @"sed\s+(-\w+\s+)*-i"
        + @"|systemctl\s+(--user\s+)?(start|stop|restart|reload|enable|disable|mask|unmask|edit|daemon-reload|reboot|poweroff|suspend)\b"
        + @"|steamosctl\s+(set-|switch-|trim-|update-|factory)"
        + @"|nmcli\b[^;|&]*\b(connect|disconnect|modify|add|delete|up|down|hotspot)\b"
        + @"|git\s+(init|add|commit|reset|checkout|restore|rm|mv|push|pull|clone)\b"
        + @"|(?<=\s)>>?(?!&|\s*/dev/null)\s*[~/$\w.]")]
    private static partial Regex ModifyingPattern();

    [GeneratedRegex(@"'[^']*'|""(?:\\.|[^""\\])*""")]
    private static partial Regex QuotedPattern();

    public static IReadOnlyList<string> Validate(IReadOnlyList<KnowledgeScope> scopes)
    {
        var issues = new List<string>();
        var ids = new HashSet<string>();
        var allIds = scopes.SelectMany(s => s.Categories).SelectMany(c => c.Modules).Select(m => m.Id).ToHashSet();

        foreach (var scope in scopes)
        foreach (var category in scope.Categories)
        {
            if (category.Modules.Count == 0) issues.Add($"[{category.Name}] 分类下没有模块");

            foreach (var m in category.Modules)
            {
                var where = $"[{m.Id}]";
                if (!ids.Add(m.Id)) issues.Add($"{where} 模块 Id 重复");
                if (DisplayWidth(m.Title) > 36) issues.Add($"{where} 标题过长（显示宽度 {DisplayWidth(m.Title)}），导航栏会被截断");
                if (m.Why.Count == 0) issues.Add($"{where} 缺少「原理」段");
                if (m.Consequences.Count == 0) issues.Add($"{where} 缺少「后果与边界」段");
                if (m.Steps.Count == 0) issues.Add($"{where} 缺少操作步骤");
                if (m.Verify.Count == 0) issues.Add($"{where} 缺少「验证」段");
                foreach (var r in m.Related)
                    if (r == m.Id || !allIds.Contains(r)) issues.Add($"{where} 相关模块「{r}」不存在或指向自身");

                foreach (var text in m.Why.Concat(m.Consequences).Concat(m.Verify).Append(m.Overview))
                    CheckMarkup(text, where, issues);

                foreach (var s in m.Steps)
                {
                    var at = $"{where} 步骤「{s.Title}」";
                    CheckMarkup(s.Note, at, issues);
                    CheckMarkup(s.Outcome, at, issues);
                    if (string.IsNullOrWhiteSpace(s.Outcome))
                        issues.Add($"{at} 缺少「执行后」说明（.Then(...)）");

                    if (PseudoEchoPattern().IsMatch(s.Command))
                        issues.Add($"{at} 是 echo 包装的伪指令，请改为真实可执行的命令");
                    if (SudoPattern().IsMatch(s.Command) && s.Risk < Risk.Root)
                        issues.Add($"{at} 含 sudo，但风险标记为 {s.Risk}");
                    if (s.Risk == Risk.Read && ModifyingPattern().Match(QuotedPattern().Replace(s.Command, "''")) is { Success: true } mod)
                        issues.Add($"{at} 标记为只读，但含有修改操作「{mod.Value.Trim()}」；请改为 Change，或确认后调整写法");
                    if (RecursiveRemovePattern().IsMatch(s.Command) && s.Risk < Risk.Change)
                        issues.Add($"{at} 含 rm -r/-f，不能标记为只读");
                    if (s.IsPractice && (s.Risk is Risk.Read or Risk.Root || s.Host is not null))
                        issues.Add($"{at} 标记为 Practice()，但只有在本机执行的 Change / Danger 步骤可以标记为练习步骤");
                    if (s.IsPractice && SudoPattern().IsMatch(s.Command))
                        issues.Add($"{at} 标记为 Practice()，但包含 sudo");
                    if (s.Command.Contains("%command%") && s.Host != Hosts.SteamLaunchOptions)
                        issues.Add($"{at} 含 %command%，Host 应为 Hosts.SteamLaunchOptions");
                }
            }
        }

        return issues;
    }

    private static void CheckMarkup(string text, string where, List<string> issues)
    {
        var withoutBold = text.Replace("**", "");
        if ((text.Length - withoutBold.Length) / 2 % 2 != 0)
            issues.Add($"{where} 加粗标记 ** 未成对：{Excerpt(text)}");
        if (withoutBold.Count(c => c == '`') % 2 != 0)
            issues.Add($"{where} 代码标记 ` 未成对：{Excerpt(text)}");
    }

    // 中日韩字符按 2 个英文字符宽度计
    private static int DisplayWidth(string text) => text.Sum(c => c > 0x2E80 ? 2 : 1);

    private static string Excerpt(string text) => text.Length <= 40 ? text : text[..40] + "…";

    /// <summary>按转义后的真实内容打印指令，用于在终端里逐条核对。</summary>
    public static int PrintCommands(IReadOnlyList<KnowledgeScope> scopes, string? moduleId)
    {
        var modules = scopes.SelectMany(s => s.Categories).SelectMany(c => c.Modules)
            .Where(m => moduleId is null || m.Id == moduleId).ToList();
        foreach (var m in modules)
        {
            Console.WriteLine($"## {m.Id}");
            foreach (var s in m.Steps)
                Console.WriteLine($"[{s.Risk}{(s.Host is null ? "" : " @ " + s.Host)}] {s.Title}\n{s.Command}\n  → {s.Outcome}\n");
        }
        return modules.Count == 0 ? 1 : 0;
    }

    public static int RunCli(IReadOnlyList<KnowledgeScope> scopes)
    {
        var issues = Validate(scopes);
        var modules = scopes.SelectMany(s => s.Categories).SelectMany(c => c.Modules).ToList();
        var steps = modules.SelectMany(m => m.Steps).ToList();

        Console.WriteLine($"专区 {scopes.Count} · 分类 {scopes.Sum(s => s.Categories.Count)} · 模块 {modules.Count} · 指令 {steps.Count}");
        foreach (var risk in Enum.GetValues<Risk>())
            Console.WriteLine($"  {risk,-7} {steps.Count(s => s.Risk == risk)}");

        foreach (var issue in issues) Console.WriteLine($"✗ {issue}");
        Console.WriteLine(issues.Count == 0 ? "✓ 内容检查通过" : $"共 {issues.Count} 个问题");
        return issues.Count == 0 ? 0 : 1;
    }
}
