using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LyuExtensions.Aspects;
using OpenCvSharp;
using Window = Avalonia.Controls.Window;

namespace FADemo.ViewModels;

[Transient]
public partial class BatchResultViewModel:ViewModelBase
{
    public AvaloniaList<Mat> Results { get; } = [];
    
    private Window? _window;
    
    public void SetWindow(Window window)
    {
        _window = window;
    }

    [ObservableProperty]
    public partial WindowTransparencyLevel CurrentBackgroundType { get; set; } = WindowTransparencyLevel.AcrylicBlur;

    partial void OnCurrentBackgroundTypeChanged(WindowTransparencyLevel value)
    {
        if (_window is not null)
        {
            _window.TransparencyLevelHint = value == WindowTransparencyLevel.None ? [] : [value];
        }
    }

    [RelayCommand]
    private void ShowResults(Mat mat)
    {
        Cv2.ImShow("Results", mat);
    }

    [RelayCommand]
    private async Task ExportAllAsync()
    {
        if (_window is null || Results.Count == 0)
            return;

        var folderPicker = await _window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择导出文件夹",
            AllowMultiple = false
        });

        if (folderPicker.Count == 0)
            return;

        var selectedFolder = folderPicker[0];
        var folderPath = selectedFolder.Path.LocalPath;

        try
        {
            for (int i = 0; i < Results.Count; i++)
            {
                var mat = Results[i];
                var fileName = $"result_{i + 1:D4}.png";
                var filePath = Path.Combine(folderPath, fileName);
                Cv2.ImWrite(filePath, mat);
            }

            // 可选：显示成功消息
            // await MessageBoxHelper.ShowAsync($"成功导出 {Results.Count} 张图片到 {folderPath}");
        }
        catch (Exception ex)
        {
            // 可选：显示错误消息
            // await MessageBoxHelper.ShowAsync($"导出失败: {ex.Message}");
        }
    }
}
