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

    public static IReadOnlyList<string> Validate(IReadOnlyList<KnowledgeScope> scopes)
    {
        var issues = new List<string>();
        var ids = new HashSet<string>();

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

                foreach (var text in m.Why.Concat(m.Consequences).Concat(m.Verify).Append(m.Overview))
                    CheckMarkup(text, where, issues);

                foreach (var s in m.Steps)
                {
                    var at = $"{where} 步骤「{s.Title}」";
                    CheckMarkup(s.Note, at, issues);

                    if (PseudoEchoPattern().IsMatch(s.Command))
                        issues.Add($"{at} 是 echo 包装的伪指令，请改为真实可执行的命令");
                    if (SudoPattern().IsMatch(s.Command) && s.Risk < Risk.Root)
                        issues.Add($"{at} 含 sudo，但风险标记为 {s.Risk}");
                    if (RecursiveRemovePattern().IsMatch(s.Command) && s.Risk != Risk.Danger)
                        issues.Add($"{at} 含 rm -r/-f，风险必须标记为 Danger");
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
