using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.Styling;
using LyuExtensions.Aspects;

namespace FADemo.ViewModels;

[Singleton]
public partial class SettingsPageViewModel : ViewModelBase
{
    private readonly FluentAvaloniaTheme _faTheme;

    public SettingsPageViewModel()
    {
        _faTheme = (Application.Current?.Styles[0] as FluentAvaloniaTheme)!;
        CurrentAppTheme = AppThemes[0];

        if (_faTheme.TryGetResource("SystemAccentColor", null, out var currentColor))
        {
            CustomAccentColor = (Color)currentColor;
        }
    }

    public ThemeVariant[] AppThemes { get; } =
        [
            ThemeVariant.Default,
            ThemeVariant.Light,
            ThemeVariant.Dark,
        ];

    public List<WindowTransparencyLevel> BackgroundTypes { get; } =
        [
            WindowTransparencyLevel.Mica,
            WindowTransparencyLevel.AcrylicBlur,
        ];

    public List<string> NavigationTransitions { get; } =
        [
            "Default",
            "DrillIn",
            "Slide",
            "None",
        ];

    [ObservableProperty]
    public partial ThemeVariant? CurrentAppTheme { get; set; }

    partial void OnCurrentAppThemeChanged(ThemeVariant? value)
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = value;
            _faTheme.PreferSystemTheme = value == ThemeVariant.Default;
        }
    }

    [ObservableProperty]
    public partial Color CustomAccentColor { get; set; }

    partial void OnCustomAccentColorChanged(Color value)
    {
        _faTheme.CustomAccentColor = value;
    }

    [ObservableProperty]
    public partial WindowTransparencyLevel CurrentBackgroundType { get; set; } = WindowTransparencyLevel.Mica;

    partial void OnCurrentBackgroundTypeChanged(WindowTransparencyLevel value)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
        {
            desktop.MainWindow.TransparencyLevelHint = value == WindowTransparencyLevel.None ? [] : [value];
        }
    }

    [ObservableProperty]
    public partial string CurrentNavigationTransition { get; set; } = "Default";
}
