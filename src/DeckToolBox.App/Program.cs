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
        if (args is ["--commands", ..])
            return ContentValidator.PrintCommands(ContentRegistry.Scopes, args.Length > 1 ? args[1] : null);
        if (args is ["--try-read", ..])
            return ReadStepRunner.RunCli(ContentRegistry.Scopes, args[1..]);

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
