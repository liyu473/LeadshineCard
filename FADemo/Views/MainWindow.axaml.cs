using Avalonia.Controls;
using FADemo.Helpers;
using FADemo.ViewModels;
using FluentAvalonia.UI.Windowing;
using LyuExtensions.Aspects;

namespace FADemo.Views;

[Transient]
public partial class MainWindow : AppWindow
{
    [Inject]
    private readonly MainWindowViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        TransparencyLevelHint = [WindowTransparencyLevel.Mica];
        SplashScreen = SplashScreenHelper.GetSplashScreenWithTitleOnly("Demo");
        RootHost.Content = App.GetService<MainView>();
    }
}
