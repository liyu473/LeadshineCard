using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FADemo.Services;
using FADemo.ViewModels;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using LyuExtensions.Aspects;
using Microsoft.Extensions.Logging;

namespace FADemo.Views;

[Transient]
public partial class MainView : UserControl
{
    [Inject]
    private readonly ILogger<MainView> _logger;

    [Inject]
    private readonly MainViewModel _vm;

    [Inject]
    private readonly INavigationPageFactory _navigationPageFactory;

    [Inject]
    private readonly SettingsPageViewModel _settingsVm;

    public MainView()
    {
        InitializeComponent();
        DataContext = _vm;

        FrameView.Navigated += OnFrameViewNavigated;
        BackButton.Click += (sender, args) => FrameView.GoBack();

        _navigationPageFactory?.RegistersPages();
        FrameView.NavigationPageFactory = _navigationPageFactory;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(
                aw.TitleBar.RightInset,
                GridUnitType.Pixel
            );
            BackButton.IsVisible = false;
        }

        NavigateTo("HomePage");
    }


    private void OnFrameViewNavigated(object? sender, NavigationEventArgs e)
    {
        if (FrameView.BackStackDepth > 0 && !BackButton.IsVisible)
        {
            _ = AnimateTitleForBackButton(show: true);
        }
        else if (FrameView.BackStackDepth == 0 && BackButton.IsVisible)
        {
            _ = AnimateTitleForBackButton(show: false);
        }

        SyncSelectedItemWithCurrentPage();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);

        // Frame handles X1 -> BackRequested automatically, we can handle X2
        // here to enable forward navigation
        if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton2Released)
        {
            if (FrameView.CanGoForward)
            {
                FrameView.GoForward();
                e.Handled = true;
            }
        }

        base.OnPointerReleased(e);
    }

    /// <summary>
    /// 为返回按钮显示提供动画
    /// </summary>
    /// <param name="show"></param>
    /// <returns></returns>
    private async Task AnimateTitleForBackButton(bool show)
    {
        var from = show ? new Thickness(12, 8, 12, 8) : new Thickness(48, 8, 12, 8);
        var to = show ? new Thickness(48, 8, 12, 8) : new Thickness(12, 8, 12, 8);

        if (!show)
            BackButton.IsVisible = false;

        var ani = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(220),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(MarginProperty, from) } },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    KeySpline = new KeySpline(0, 0, 0, 1),
                    Setters = { new Setter(MarginProperty, to) },
                },
            },
        };

        await ani.RunAsync(WindowIcon);

        if (show)
            BackButton.IsVisible = true;
    }

    private void OnNavViewItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.IsSettingsInvoked)
        {
            NavigateTo("SettingsPage");
            return;
        }

        if (e.InvokedItemContainer is NavigationViewItem nvi && nvi.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string tag)
    {
        var assembly = typeof(MainView).Assembly;
        var pageType = assembly.GetType($"{assembly.GetName().Name}.Views.{tag}");

        if (pageType != null)
        {
            if (App.GetService(pageType) is Control page)
            {
                var transition = GetNavigationTransition();
                FrameView.NavigateFromObject(
                    page,
                    new FrameNavigationOptions
                    {
                        TransitionInfoOverride = transition,
                        IsNavigationStackEnabled = true,
                    }
                );
            }
        }
    }

    /// <summary>
    /// 根据用户设置获取导航动画
    /// </summary>
    private NavigationTransitionInfo? GetNavigationTransition()
    {
        var transitionName = _settingsVm?.CurrentNavigationTransition ?? "Default";

        return transitionName switch
        {
            "Slide" => new SlideNavigationTransitionInfo(),
            "DrillIn" => new DrillInNavigationTransitionInfo(),
            "None" => new SuppressNavigationTransitionInfo(),
            _ => null,
        };
    }

    private void SyncSelectedItemWithCurrentPage()
    {
        if (FrameView.Content == null)
            return;

        var currentPageType = FrameView.Content.GetType();
        var currentPageName = currentPageType.Name;

        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem nvi && nvi.Tag is string tag)
            {
                bool isSelected = tag == currentPageName;

                if (isSelected)
                {
                    NavView.SelectedItem = nvi;
                }

                SetNavigationItemIcon(nvi, isSelected);
            }
        }

        if (currentPageName == "SettingsPage")
        {
            NavView.SelectedItem = NavView.SettingsItem;
            SetNavigationItemIcon(NavView.SettingsItem, true);
        }
        else
        {
            SetNavigationItemIcon(NavView.SettingsItem, false);
        }
    }

    private void SetNavigationItemIcon(NavigationViewItem item, bool isSelected)
    {
        if (item == null)
            return;

        if (item == NavView.SettingsItem || item.Tag as string == "设置")
        {
            var settingsIconKey = isSelected ? "SettingsIconFilled" : "SettingsIcon";
            if (
                this.TryFindResource(settingsIconKey, out var settingsIcon)
                || Application.Current?.TryFindResource(settingsIconKey, out settingsIcon) == true
            )
            {
                item.IconSource = settingsIcon as IconSource;
            }
            return;
        }

        if (item.Tag is not string tag)
            return;

        var iconKey = tag switch
        {
            "HomePage" => isSelected ? "HomeIconFilled" : "HomeIcon",
            "CardPage" => isSelected ? "CardIconFilled" : "CardIcon",
            _ => null,
        };

        if (iconKey != null)
        {
            if (
                this.TryFindResource(iconKey, out var icon)
                || Application.Current?.TryFindResource(iconKey, out icon) == true
            )
            {
                item.IconSource = icon as IconSource;
            }
        }
    }
}
