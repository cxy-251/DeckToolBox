using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using DeckToolBox.App.Content;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Framework.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Window? _window;
    private readonly List<CategoryDefinition> _allCategories = new();
    private KnowledgeScope _currentScope = KnowledgeScope.SteamDeckNative;
    private ModuleDefinition? _selectedModule;
    private DynamicModuleViewModel? _currentModuleViewModel;
    private string _statusMessage = "就绪：选择左侧知识模块，点击【复制指令】后在终端中手动执行";
    private bool _isStatusSuccess = false;

    public ObservableCollection<CategoryDefinition> Categories { get; } = new();

    public KnowledgeScope CurrentScope
    {
        get => _currentScope;
        set
        {
            if (SetProperty(ref _currentScope, value))
            {
                OnPropertyChanged(nameof(IsSteamDeckScope));
                OnPropertyChanged(nameof(IsUniversalDistrosScope));
                OnPropertyChanged(nameof(IsKernelInternalsScope));
                OnPropertyChanged(nameof(CurrentScopeTitle));
                OnPropertyChanged(nameof(CurrentScopeBadge));
                RefreshCategoriesForScope();
            }
        }
    }

    public bool IsSteamDeckScope => CurrentScope == KnowledgeScope.SteamDeckNative;
    public bool IsUniversalDistrosScope => CurrentScope == KnowledgeScope.UniversalDistros;
    public bool IsKernelInternalsScope => CurrentScope == KnowledgeScope.LinuxKernelInternals;

    public string CurrentScopeTitle => CurrentScope switch
    {
        KnowledgeScope.SteamDeckNative => "🎮 Steam Deck 本机专精 (SteamOS 3.x)",
        KnowledgeScope.UniversalDistros => "🐧 广域 Linux 宇宙与全景包管理",
        KnowledgeScope.LinuxKernelInternals => "🧠 Linux 通用内核底石与系统运行态",
        _ => "Linux 知识中心"
    };

    public string CurrentScopeBadge => CurrentScope switch
    {
        KnowledgeScope.SteamDeckNative => "本机专精",
        KnowledgeScope.UniversalDistros => "跨发行版宇宙",
        KnowledgeScope.LinuxKernelInternals => "内核底石",
        _ => "通用"
    };

    public ModuleDefinition? SelectedModule
    {
        get => _selectedModule;
        set
        {
            if (SetProperty(ref _selectedModule, value))
            {
                if (_selectedModule != null)
                {
                    CurrentModuleViewModel = new DynamicModuleViewModel(_selectedModule, CopyTextToClipboard);
                }
                OnPropertyChanged(nameof(CurrentCategoryTitle));
                OnPropertyChanged(nameof(CurrentModuleTitle));
            }
        }
    }

    public DynamicModuleViewModel? CurrentModuleViewModel
    {
        get => _currentModuleViewModel;
        private set => SetProperty(ref _currentModuleViewModel, value);
    }

    public string CurrentCategoryTitle => _selectedModule?.CategoryName ?? "请选择导航小节";
    public string CurrentModuleTitle => _selectedModule?.Title ?? "欢迎使用 DeckToolBox 知识中心";

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsStatusSuccess
    {
        get => _isStatusSuccess;
        set => SetProperty(ref _isStatusSuccess, value);
    }

    public MainWindowViewModel(Window? window = null)
    {
        _window = window;

        // 从应用层内容注册表加载所有类别与知识数据
        _allCategories.AddRange(ContentRegistry.LoadAllCategories());

        // 初始化当前视点下的分类列表
        RefreshCategoriesForScope();
    }

    public void SelectScope(KnowledgeScope scope)
    {
        CurrentScope = scope;
    }

    public void SelectScopeByString(string scopeName)
    {
        if (Enum.TryParse<KnowledgeScope>(scopeName, true, out var scope))
        {
            SelectScope(scope);
        }
    }

    private void RefreshCategoriesForScope()
    {
        Categories.Clear();
        var filtered = _allCategories.Where(c => c.Scope == _currentScope).ToList();
        foreach (var cat in filtered)
        {
            Categories.Add(cat);
        }

        // 切换视点时默认选中该视点下的第一个模块
        if (Categories.Count > 0 && Categories[0].Modules.Count > 0)
        {
            SelectedModule = Categories[0].Modules[0];
        }
    }

    public async void CopyTextToClipboard(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        try
        {
            if (_window?.Clipboard != null)
            {
                await _window.Clipboard.SetTextAsync(text);
                StatusMessage = $"✅ 已复制指令到剪贴板！请前往 Konsole 终端粘贴并执行。";
                IsStatusSuccess = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"⚠️ 复制到剪贴板失败: {ex.Message}";
            IsStatusSuccess = false;
        }
    }
}
