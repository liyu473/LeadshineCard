using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LyuExtensions.Aspects;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using ZLogger;

namespace FADemo.ViewModels;

[Singleton]
public partial class HomePageViewModel : ViewModelBase
{
    [Inject]
    private readonly ILogger<HomePageViewModel> _logger;
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
    public partial Mat? CheckImage {get; set; }
    
    private bool CanCheck() => CheckImage is not null;
    
    [ObservableProperty]
    public partial Bitmap? ResultImage {get; set; }

    [TryCatch]
    [RelayCommand]
    private async Task ImportImages()
    {
        var mainWindow = (
            Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
        )!.MainWindow!;

        var topLevel = TopLevel.GetTopLevel(mainWindow)!;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "选择检测图片",
                AllowMultiple = false,
                FileTypeFilter = [FilePickerFileTypes.ImageAll],
            }
        );

        if (files.Count > 0)
        {
            _logger.ZLogTrace($"导入图片地址为{files[0].Path.LocalPath}");
            CheckImage =  Cv2.ImRead(files[0].Path.LocalPath);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCheck))]
    private void Check()
    {
        
    }
}
