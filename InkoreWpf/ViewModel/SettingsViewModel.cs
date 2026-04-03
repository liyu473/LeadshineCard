using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Media.Animation;
using InkoreWpf.Properties;
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
        SelectedTheme = (WindowThemeMode)Settings.Default.SelectedTheme;
        SelectedBackdrop = (WindowBackdropType)Settings.Default.SelectedBackdrop;
        SelectedNavMode = (NavigationViewPaneDisplayMode)Settings.Default.SelectedNavMode;
        SelectedColor = Settings.Default.SelectedColor;
        SelectedNavTransition = NavigationTransitions[0];
    }

    [ObservableProperty]
    public partial NavigationViewPaneDisplayMode SelectedNavMode { get; set; } 

    partial void OnSelectedNavModeChanged(NavigationViewPaneDisplayMode value)
    {
        Settings.Default.SelectedNavMode = (int)value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    public partial WindowBackdropType SelectedBackdrop { get; set; }

    partial void OnSelectedBackdropChanged(WindowBackdropType value)
    {
        WindowBackdropHelper.SetBackdrop(_mainWindow, value);
        Settings.Default.SelectedBackdrop = (int)value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    public partial WindowThemeMode SelectedTheme { get; set; }

    partial void OnSelectedThemeChanged(WindowThemeMode value)
    {
        WindowThemeHelper.SetTheme(_mainWindow, value);
        Settings.Default.SelectedTheme = (int)value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    public partial NavigationTransitionItem SelectedNavTransition { get; set; } 

    [ObservableProperty]
    public partial Color SelectedColor { get; set; }

    partial void OnSelectedColorChanged(Color value)
    {
        ThemeManager.Current.AccentColor = value;
        Settings.Default.SelectedColor = value;
        Settings.Default.Save();
    }

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string AppName => _assembly.GetName().Name ?? string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version => $"{_assembly.GetName().Version?.ToString() ?? string.Empty} © KingLee 2026" ;
}

public record NavigationTransitionItem(string Name, NavigationTransitionInfo Value);
