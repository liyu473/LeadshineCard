using Avalonia.Controls;
using FADemo.Helpers;
using FADemo.ViewModels;
using FluentAvalonia.UI.Windowing;
using LyuExtensions.Aspects;
using LyuExtensions.Extensions;

namespace FADemo.Views;

[Transient]
public partial class BatchResult : AppWindow
{
    [Inject]
    private readonly BatchResultViewModel _vm;

    public BatchResult()
    {
        InitializeComponent();
        DataContext = _vm;

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];

        SplashScreen = SplashScreenHelper.GetSplashScreenWithTitleOnly("请等待。。。");
        
        _vm!.SetWindow(this);

        Closed += (sender, args) =>
        {
            _vm!.Results.ForEach(m => m.Dispose());
        };
    }
}
