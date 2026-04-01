using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Media.Animation;
using InkoreWpf.View;
using LyuExtensions.Aspects;
using LyuWpfHelper.Helpers;
using LyuWpfHelper.ViewModels;

namespace InkoreWpf.ViewModel;

[Singleton]
public partial class SettingsViewModel : ViewModelBase
{
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    [Inject]
    private readonly MainWindow _mainWindow;

    public ObservableCollection<NavigationTransitionItem> NavigationTransitions { get; } =
        [
            new("Entrance", new EntranceNavigationTransitionInfo()),
            new("DrillIn", new DrillInNavigationTransitionInfo()),
            new("Slide", new SlideNavigationTransitionInfo()),
            new("Suppress", new SuppressNavigationTransitionInfo()),
        ];

    public SettingsViewModel()
    {
        SelectedTheme = WindowThemeMode.FollowSystem;
        SelectedBackdrop = WindowBackdropType.Mica;
        SelectedNavTransition = NavigationTransitions[0];
    }

    [ObservableProperty]
    public partial NavigationViewPaneDisplayMode SelectedNavMode { get; set; } =
        NavigationViewPaneDisplayMode.Auto;

    [ObservableProperty]
    public partial WindowBackdropType SelectedBackdrop { get; set; } 

    partial void OnSelectedBackdropChanged(WindowBackdropType value)
    {
        WindowBackdropHelper.SetBackdrop(_mainWindow, value);
    }

    [ObservableProperty]
    public partial WindowThemeMode SelectedTheme { get; set; }

    partial void OnSelectedThemeChanged(WindowThemeMode value)
    {
        WindowThemeHelper.SetTheme(_mainWindow, value);
    }

    [ObservableProperty]
    public partial NavigationTransitionItem SelectedNavTransition { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string AppName => _assembly.GetName().Name ?? string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version => _assembly.GetName().Version?.ToString() ?? string.Empty;
}

public record NavigationTransitionItem(string Name, NavigationTransitionInfo Value);
