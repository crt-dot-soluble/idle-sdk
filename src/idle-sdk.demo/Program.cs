using Avalonia;
using IdleSdk.Demo.Infrastructure;
using System;
using System.Threading.Tasks;

namespace IdleSdk.Demo;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        DemoLogger.Initialize("idle-sdk-demo");
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            if (eventArgs.ExceptionObject is Exception ex)
            {
                DemoLogger.Error("runtime", "unhandled-exception", ex);
            }
        };
        TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
        {
            DemoLogger.Error("runtime", "unobserved-task-exception", eventArgs.Exception);
            eventArgs.SetObserved();
        };

        DemoLogger.Info("runtime", "startup", new { args = args.Length });
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        DemoLogger.Info("runtime", "shutdown");
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
