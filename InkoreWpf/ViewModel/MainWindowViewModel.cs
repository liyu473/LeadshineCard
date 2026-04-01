using CommunityToolkit.Mvvm.ComponentModel;
using InkoreWpf.Service;
using LyuExtensions.Aspects;
using LyuWpfHelper.ViewModels;
using System.Reflection;

namespace InkoreWpf.ViewModel;

[Singleton]
public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    [Inject]
    private readonly NavigateServer _nav;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    public partial object? Page { get; set; }

    public string? Header => Page is { } page ? _nav.GetHeaderByType(page.GetType()) : null;

    public SettingsViewModel SettingsViewModel => App.GetService<SettingsViewModel>();

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string AppName => _assembly.GetName().Name ?? string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version => _assembly.GetName().Version?.ToString() ?? string.Empty;

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title => $"{AppName} v{Version}";
}
