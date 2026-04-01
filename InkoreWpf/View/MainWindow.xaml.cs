using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using InkoreWpf.Service;
using InkoreWpf.ViewModel;
using LyuExtensions.Aspects;
using LyuExtensions.Extensions;
using LyuWpfHelper.Controls;
using LyuWpfHelper.Extensions;
using LyuWpfHelper.Helpers;
using LyuWpfHelper.Services;

namespace InkoreWpf.View;

[Singleton]
public partial class MainWindow : LyuWindow
{
    [Inject]
    private readonly MainWindowViewModel _vm;

    [Inject]
    private readonly NavigateServer _nav;

    [Inject]
    private readonly INotificationService notificationService;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    private void NavView_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args
    )
    {
        if (args.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            var page = _nav.GetPageByTag(tag);

            if (page is not null)
            {
                ContentFrame.Navigate(
                    page,
                    null,
                    App.GetService<SettingsViewModel>().SelectedNavTransition.Value
                );
            }
        }
    }

    private void NvSample_BackRequested(
        NavigationView sender,
        NavigationViewBackRequestedEventArgs args
    )
    {
        if (ContentFrame.CanGoBack)
            ContentFrame.GoBack();
    }

    protected override void OnThemeChanged(LyuWindowThemeChangedEventArgs e)
    {
        base.OnThemeChanged(e);
        var elementTheme =
            e.EffectiveTheme == WindowThemeMode.Dark ? ElementTheme.Dark : ElementTheme.Light;

        ThemeManager.SetRequestedTheme(this, elementTheme);
        notificationService.ShowSuccess(
            $"切换至{elementTheme.GetEnumDescription()}",
            position: NotificationPosition.BottomRight
        );
    }

    private void About_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        AboutDrawer.IsOpen = !AboutDrawer.IsOpen;
    }
}
