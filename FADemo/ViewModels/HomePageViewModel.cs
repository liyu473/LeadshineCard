using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FADemo.Models;
using LyuExtensions.Aspects;

namespace FADemo.ViewModels;

[Singleton]
public partial class HomePageViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial AvaloniaList<VCheckItem> Images { get; set; } = [];

    [ObservableProperty]
    public partial VCheckItem? SelectedItem { get; set; }

    [TryCatch]
    [RelayCommand]
    public async Task ImportImages()
    {
        var mainWindow = (
            Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
        )!.MainWindow!;

        var topLevel = TopLevel.GetTopLevel(mainWindow)!;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "选择图片(可多选)",
                AllowMultiple = true,
                FileTypeFilter = [FilePickerFileTypes.ImageAll],
            }
        );

        if (files.Count > 0)
        {
            Images.Clear();
            AvaloniaList<VCheckItem> list = [];
            await Task.Run(async () =>
            {
                foreach (var file in files)
                {
                    await using var stream = await file.OpenReadAsync();
                    var bitmap = new Bitmap(stream);
                    list.Add(new VCheckItem { ImageSource = bitmap, Path = file.Path });
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Images = [.. list];
                    });
                }
            });
        }
    }

    [RelayCommand]
    private void Delete() { }
}
