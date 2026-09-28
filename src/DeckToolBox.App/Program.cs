using System;
using Avalonia;
using DeckToolBox.App.Content;
using DeckToolBox.App.Framework;

namespace DeckToolBox.App;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args is ["--check"])
            return ContentValidator.RunCli(ContentRegistry.Scopes);

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
