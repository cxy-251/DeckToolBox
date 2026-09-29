using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DeckToolBox.App.Framework.ViewModels;

namespace DeckToolBox.App.Framework.Views;

public partial class MainWindow : Window
{
    // 滚动后选中项上下各留出的距离，避免它紧贴列表边缘
    private const double ScrollMargin = 48;

    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        Opened += (_, _) => RevealSelected();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel is not null) _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel is not null) _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        base.OnDataContextChanged(e);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 选中模块变化（含从搜索结果跳到其他专区）、退出搜索回到目录时，让选中项滚到可见区域
        if (e.PropertyName is nameof(MainWindowViewModel.CurrentModule) or nameof(MainWindowViewModel.IsSearching))
            RevealSelected();
    }

    /// <summary>等列表完成布局后，把当前可见列表中的选中项滚动到视野内；已可见时不移动。</summary>
    private void RevealSelected() => Dispatcher.UIThread.Post(() =>
    {
        var list = SearchScroll.IsVisible ? SearchScroll : NavScroll;
        var item = list.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.DataContext is NavModuleViewModel { IsSelected: true });

        item?.BringIntoView(new Rect(0, -ScrollMargin, item.Bounds.Width, item.Bounds.Height + 2 * ScrollMargin));
    }, DispatcherPriority.Loaded);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && SearchBox.IsFocused)
        {
            SearchBox.Text = string.Empty;
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }
}
