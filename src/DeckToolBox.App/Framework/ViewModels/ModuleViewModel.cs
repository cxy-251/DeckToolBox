using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Framework.ViewModels;

/// <summary>右侧内容区：一个模块的完整渲染数据。</summary>
public sealed class ModuleViewModel
{
    public ModuleViewModel(ModuleDefinition module, KnowledgeScope scope, CategoryDefinition category, Action<string> copy)
    {
        Module = module;
        Breadcrumb = $"{scope.Icon} {scope.TabLabel}  ›  {category.Icon} {category.Name}";
        Steps = module.Steps.Select((s, i) => new StepViewModel(s, i + 1, copy)).ToList();
    }

    public ModuleDefinition Module { get; }
    public string Breadcrumb { get; }
    public string Title => Module.Title;
    public string Keywords => Module.Keywords;
    public string LevelLabel => Labels.Level(Module.Level);
    public string Overview => Module.Overview;

    public IReadOnlyList<string> Why => Module.Why;
    public IReadOnlyList<string> Consequences => Module.Consequences;
    public IReadOnlyList<StepViewModel> Steps { get; }
    public IReadOnlyList<string> Verify => Module.Verify;

    public bool HasWhy => Why.Count > 0;
    public bool HasConsequences => Consequences.Count > 0;
    public bool HasSteps => Steps.Count > 0;
    public bool HasVerify => Verify.Count > 0;
}

public sealed partial class StepViewModel
{
    [GeneratedRegex(@"<[^<>\s'""|&;]+>")]
    private static partial Regex PlaceholderPattern();

    public StepViewModel(CommandStep step, int number, Action<string> copy)
    {
        Step = step;
        Number = number;
        CopyCommand = new RelayCommand(() => copy(step.Command));
    }

    public CommandStep Step { get; }
    public int Number { get; }
    public string Title => Step.Title;
    public string Command => Step.Command;
    public string Note => Step.Note;
    public bool HasNote => !string.IsNullOrWhiteSpace(Step.Note);
    public string Outcome => Step.Outcome;
    public bool HasOutcome => !string.IsNullOrWhiteSpace(Step.Outcome);

    public string RiskLabel => Labels.Risk(Step.Risk);
    public bool IsRead => Step.Risk == Risk.Read;
    public bool IsChange => Step.Risk == Risk.Change;
    public bool IsRoot => Step.Risk == Risk.Root;
    public bool IsDanger => Step.Risk == Risk.Danger;

    public bool HasHost => Step.Host is not null;
    public string HostLabel => Step.Host is null ? string.Empty : $"执行位置：{Step.Host}";

    /// <summary>指令里含有 &lt;AppID&gt; 这类占位符，复制后需要先替换。</summary>
    public bool HasPlaceholder => PlaceholderPattern().IsMatch(Step.Command);

    public ICommand CopyCommand { get; }
}

public static class Labels
{
    public static string Level(ModuleLevel level) => level switch
    {
        ModuleLevel.Basic => "入门",
        ModuleLevel.Intermediate => "进阶",
        _ => "深入"
    };

    public static string Risk(Risk risk) => risk switch
    {
        Models.Risk.Read => "只读",
        Models.Risk.Change => "修改用户数据",
        Models.Risk.Root => "需要 sudo",
        _ => "高危 · 难以撤销"
    };
}
