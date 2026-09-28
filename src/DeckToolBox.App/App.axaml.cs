using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DeckToolBox.App.Content;
using DeckToolBox.App.Framework.ViewModels;
using DeckToolBox.App.Framework.Views;

namespace DeckToolBox.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            window.DataContext = new MainWindowViewModel(
                ContentRegistry.Scopes,
                text => window.Clipboard?.SetTextAsync(text)
                        ?? throw new System.InvalidOperationException("当前会话没有可用的剪贴板"));
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
