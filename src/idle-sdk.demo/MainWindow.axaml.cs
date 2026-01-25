using Avalonia.Controls;
using IdleSdk.Demo.Infrastructure;
using IdleSdk.Demo.ViewModels;

namespace IdleSdk.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DemoLogger.Info("ui", "main-window-init");
        InitializeComponent();
        DataContext = new DemoViewModel();
        DemoLogger.Info("ui", "main-window-ready");
    }
}
