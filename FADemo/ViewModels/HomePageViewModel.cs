using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FADemo.Extensions;
using FADemo.Views;
using KingGleeVision;
using LyuExtensions.Aspects;
using LyuOnnxCore.Extensions;
using LyuOnnxCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using ZLogger;

namespace FADemo.ViewModels;

[Singleton]
public partial class HomePageViewModel : ViewModelBase
{
    [Inject]
    private readonly ILogger<HomePageViewModel> _logger;
    
    [Inject]
    private readonly MainWindowViewModel _mainWindowViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CropperCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreProcessCommand))]
    [NotifyCanExecuteChangedFor(nameof(CheckCommand))]
    public partial Mat? CheckImage { get; set; }

    private bool CanCheck() => CheckImage is not null;

    [ObservableProperty]
    public partial Bitmap? ResultImage { get; set; }

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
            CheckImage = Cv2.ImRead(files[0].Path.LocalPath);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCheck))]
    private void PreProcess()
    {
        ResultImage = PcbCropper.PreProcess(CheckImage!)?.ToAvaloniaBitmap();
    }

    [RelayCommand(CanExecute = nameof(CanCheck))]
    private void Cropper()
    {
        ResultImage = PcbCropper.CropPcbArea(CheckImage!)?.ToAvaloniaBitmap();
    }

    [TryCatch]
    [RelayCommand(CanExecute = nameof(CanCheck))]
    private async Task Check()
    {
        var crop = PcbCropper.CropPcbArea(CheckImage!);
        ResultImage = crop is null ? new Mat().ToAvaloniaBitmap() : (await GetCheckResult(crop)).ToAvaloniaBitmap();
    }

    [TryCatch]
    [RelayCommand]
    private async Task BatchCheck()
    {
        var mainWindow = (
            Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
        )!.MainWindow!;

        var topLevel = TopLevel.GetTopLevel(mainWindow)!;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "选择检测图片(可多选)",
                AllowMultiple = true,
                FileTypeFilter = [FilePickerFileTypes.ImageAll],
            }
        );

        if (files.Count > 0)
        {
            var window = App.GetService<BatchResult>();
            var vm = window.DataContext as BatchResultViewModel;
            _mainWindowViewModel.IsLoading = true;
            foreach (var file in files)
            {
                var read = Cv2.ImRead(file.Path.LocalPath);
                vm!.Results.Add(await GetCheckResult(PcbCropper.CropPcbArea(read)!));
            }
            _mainWindowViewModel.IsLoading = false;
            window.Show();
        }
    }

    private async Task<Mat> GetCheckResult(Mat crop)
    {
        Mat cropClone3 = crop.Clone();
        if (crop.Channels() == 1)
        {
            Cv2.CvtColor(crop, cropClone3, ColorConversionCodes.GRAY2BGR);
        }
        
        return await Task.Run(() =>
        {
            using var session = new InferenceSession("./FindError.onnx");

            var detectionOptions = new DetectionOptions
            {
                ConfidenceThreshold = 0.02f,
                NmsThreshold = 0.45f,
                FilterLabels = ["error"],
            };

            var drawOptions = new DrawOptions
            {
                ShowConfidence = true,
                ShowLabel = true,
                BoxThickness = 5,
                FontScale = 2,
                BoxColor = (0, 0, 255),
                TextColor = (0, 0, 0),
                UseChineseFont = false,
            };

            var detections = session.Detect(cropClone3, ["error"], detectionOptions);
            return cropClone3.DrawDetections(detections, drawOptions);
        });
        
       
    }

    private async Task BatchProcessingExport()
    {
        var mainWindow = (
            Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
        )!.MainWindow!;

        var topLevel = TopLevel.GetTopLevel(mainWindow)!;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "选择检测图片",
                AllowMultiple = true,
                FileTypeFilter = [FilePickerFileTypes.ImageAll],
            }
        );

        if (files.Count > 0)
        {
            int num = 0;
            foreach (var f in files)
            {
                var read = Cv2.ImRead(f.Path.LocalPath);
                string dir = Path.GetDirectoryName(f.Path.LocalPath)!;
                string outDir = Path.Combine(dir, $"crop{num++}.png");
                Cv2.ImWrite(outDir, PcbCropper.CropPcbArea(read)!);
            }
        }
    }
}
