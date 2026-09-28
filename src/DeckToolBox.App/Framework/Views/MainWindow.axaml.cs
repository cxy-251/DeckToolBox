using Avalonia.Controls;
using Avalonia.Interactivity;
using DeckToolBox.App.Framework.Models;
using DeckToolBox.App.Framework.ViewModels;

namespace DeckToolBox.App.Framework.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnScopeTabClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag && DataContext is MainWindowViewModel vm)
        {
            vm.SelectScopeByString(tag);
        }
    }

    private void OnModuleItemClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ModuleDefinition module)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SelectedModule = module;
            }
        }
    }
}
