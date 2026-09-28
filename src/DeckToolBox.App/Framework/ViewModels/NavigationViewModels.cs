using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Framework.ViewModels;

public sealed class ScopeTabViewModel : ViewModelBase
{
    private bool _isActive;

    public ScopeTabViewModel(KnowledgeScope scope, Action<ScopeTabViewModel> select)
    {
        Scope = scope;
        SelectCommand = new RelayCommand(() => select(this));
    }

    public KnowledgeScope Scope { get; }
    public string Icon => Scope.Icon;
    public string Label => Scope.TabLabel;
    public ICommand SelectCommand { get; }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}

public sealed class NavCategoryViewModel(CategoryDefinition category, IReadOnlyList<NavModuleViewModel> modules)
{
    public string Name => category.Name;
    public string Icon => category.Icon;
    public string Description => category.Description;
    public IReadOnlyList<NavModuleViewModel> Modules { get; } = modules;
}

/// <summary>导航栏与搜索结果共用的模块条目（同一模块只有一个实例，选中态因此天然同步）。</summary>
public sealed class NavModuleViewModel : ViewModelBase
{
    private bool _isSelected;
    private readonly string _searchText;

    public NavModuleViewModel(ModuleDefinition module, KnowledgeScope scope, CategoryDefinition category, Action<NavModuleViewModel> select)
    {
        Module = module;
        Scope = scope;
        Category = category;
        SelectCommand = new RelayCommand(() => select(this));
        _searchText = string.Join('\n', new[] { module.Title, module.Keywords, module.Overview, category.Name }
            .Concat(module.Why).Concat(module.Consequences).Concat(module.Verify)
            .Concat(module.Steps.SelectMany(s => new[] { s.Title, s.Command, s.Note, s.Outcome })));
    }

    public ModuleDefinition Module { get; }
    public KnowledgeScope Scope { get; }
    public CategoryDefinition Category { get; }
    public ICommand SelectCommand { get; }

    public string Title => Module.Title;
    public string Keywords => Module.Keywords;
    public string LevelLabel => Labels.Level(Module.Level);
    public string Location => $"{Scope.Icon} {Category.Name}";

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>空格分隔的多个关键词须全部命中；标题命中的得分更高。返回 0 表示不匹配。</summary>
    public int Match(string[] terms)
    {
        var score = 0;
        foreach (var term in terms)
        {
            if (Module.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) score += 10;
            else if (Module.Keywords.Contains(term, StringComparison.OrdinalIgnoreCase)) score += 5;
            else if (_searchText.Contains(term, StringComparison.OrdinalIgnoreCase)) score += 1;
            else return 0;
        }
        return score;
    }
}
