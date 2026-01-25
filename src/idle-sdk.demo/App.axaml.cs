using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IdleSdk.Demo.Infrastructure;

namespace IdleSdk.Demo;

public partial class App : Application
{
    public override void Initialize()
    {
        DemoLogger.Info("app", "initialize");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DemoLogger.Info("app", "framework-initialized");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
