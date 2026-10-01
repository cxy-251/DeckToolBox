using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Framework.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private const string IdleStatus = "点击【复制】后，到 Konsole 终端粘贴，看清楚再回车执行";

    private readonly Func<string, Task> _setClipboard;
    private readonly List<NavModuleViewModel> _allModules = new();

    private ScopeTabViewModel _activeTab;
    private IReadOnlyList<NavCategoryViewModel> _navCategories = [];
    private NavModuleViewModel? _selected;
    private ModuleViewModel? _currentModule;
    private string _searchText = string.Empty;
    private IReadOnlyList<NavModuleViewModel> _searchResults = [];
    private string _statusMessage = IdleStatus;
    private bool _isStatusSuccess;

    public MainWindowViewModel(IReadOnlyList<KnowledgeScope> scopes, Func<string, Task> setClipboard)
    {
        _setClipboard = setClipboard;

        ScopeTabs = scopes.Select(s => new ScopeTabViewModel(s, ActivateTab)).ToList();

        foreach (var scope in scopes)
            foreach (var category in scope.Categories)
                foreach (var module in category.Modules)
                    _allModules.Add(new NavModuleViewModel(module, scope, category, Select));

        ModuleCount = _allModules.Count;

        _activeTab = ScopeTabs[0];
        ActivateTab(_activeTab);
    }

    public IReadOnlyList<ScopeTabViewModel> ScopeTabs { get; }
    public int ModuleCount { get; }

    public string AppVersion { get; } = typeof(MainWindowViewModel).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";

    public KnowledgeScope ActiveScope => _activeTab.Scope;

    public IReadOnlyList<NavCategoryViewModel> NavCategories
    {
        get => _navCategories;
        private set => SetProperty(ref _navCategories, value);
    }

    public ModuleViewModel? CurrentModule
    {
        get => _currentModule;
        private set => SetProperty(ref _currentModule, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!SetProperty(ref _searchText, value)) return;
            RunSearch();
            OnPropertyChanged(nameof(IsSearching));
        }
    }

    public bool IsSearching => !string.IsNullOrWhiteSpace(_searchText);

    public IReadOnlyList<NavModuleViewModel> SearchResults
    {
        get => _searchResults;
        private set
        {
            if (SetProperty(ref _searchResults, value)) OnPropertyChanged(nameof(SearchSummary));
        }
    }

    public string SearchSummary => _searchResults.Count == 0
        ? "没有匹配的模块，换个关键词试试（支持命令名，如 journalctl）"
        : $"在全部专区中找到 {_searchResults.Count} 个模块";

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsStatusSuccess
    {
        get => _isStatusSuccess;
        private set => SetProperty(ref _isStatusSuccess, value);
    }

    private void ActivateTab(ScopeTabViewModel tab)
    {
        _activeTab.IsActive = false;
        _activeTab = tab;
        tab.IsActive = true;
        OnPropertyChanged(nameof(ActiveScope));

        NavCategories = tab.Scope.Categories
            .Select(c => new NavCategoryViewModel(c, _allModules.Where(m => m.Category == c).ToList()))
            .ToList();

        // 切换专区时，若当前模块不在该专区，默认打开该专区第一个模块
        if (_selected?.Scope != tab.Scope)
        {
            var first = NavCategories.SelectMany(c => c.Modules).FirstOrDefault();
            if (first is not null) Select(first);
        }
    }

    private void Select(NavModuleViewModel item)
    {
        if (_selected is not null) _selected.IsSelected = false;
        _selected = item;
        item.IsSelected = true;

        var related = item.Module.Related
            .Select(id => _allModules.FirstOrDefault(m => m.Module.Id == id))
            .OfType<NavModuleViewModel>()
            .ToList();
        CurrentModule = new ModuleViewModel(item.Module, item.Scope, item.Category, CopyToClipboard, related);

        if (item.Scope != _activeTab.Scope)
            ActivateTab(ScopeTabs.First(t => t.Scope == item.Scope));
    }

    private void RunSearch()
    {
        var terms = _searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        SearchResults = terms.Length == 0
            ? []
            : _allModules
                .Select(m => (Module: m, Score: m.Match(terms)))
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Module)
                .ToList();
    }

    private async void CopyToClipboard(string text)
    {
        try
        {
            await _setClipboard(text);
            StatusMessage = "✅ 已复制到剪贴板。粘贴到终端后先读一遍，确认无误再回车。";
            IsStatusSuccess = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"⚠️ 复制失败：{ex.Message}";
            IsStatusSuccess = false;
        }
    }
}
