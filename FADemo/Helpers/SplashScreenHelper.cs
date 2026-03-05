using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Windowing;

namespace FADemo.Helpers;

public class SplashScreenHelper
{
    /// <summary>
    /// 简单SplashScreen，仅包含文字
    /// </summary>
    /// <returns></returns>
    public static DemoSplashScreen GetSplashScreenWithTitleOnly(string title, int minimumShowTime = 1200)
    {
        var splashScreen = new DemoSplashScreen
        {
            AppName = title,
            MinimumShowTime = minimumShowTime
        };
        return splashScreen;
    }

    /// <summary>
    /// 自定义
    /// </summary>
    /// <returns></returns>
    public static DemoSplashScreen GetSplashScreenWithBothTitleAndLogo(object content, int minimumShowTime = 1200)
    {
        var splashScreen = new DemoSplashScreen
        {
            SplashScreenContent = content,
            MinimumShowTime = minimumShowTime
        };
        return splashScreen;
    }
}

public class DemoSplashScreen : IApplicationSplashScreen
{
    public string? AppName { get; init; }
    public IImage? AppIcon { get; init; }
    public object? SplashScreenContent { get; init; }

    // To avoid too quickly transitioning away from the splash screen, you can set a minimum
    // time to hold before loading the content, value is in Milliseconds
    public int MinimumShowTime { get; set; }

    // Place your loading tasks here. NOTE, this is already called on a background thread, so
    // if any UI thread work needs to be done, use Dispatcher.UIThread.Post or .InvokeAsync
    public Task RunTasks(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}
