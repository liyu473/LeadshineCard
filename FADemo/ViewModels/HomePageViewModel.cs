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
using KingGleeVision.Models;
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

    [Timing]
    [RelayCommand(CanExecute = nameof(CanCheck))]
    private void Cropper()
    {
        var crop = PcbCropper.CropPcbArea1(CheckImage!);
        ResultImage = crop?.CroppedMat.ToAvaloniaBitmap();
    }

    [TryCatch]
    [Timing]
    [RelayCommand(CanExecute = nameof(CanCheck))]
    private async Task Check()
    {
        var cropResult = PcbCropper.CropPcbArea1(CheckImage!);
        if (cropResult is null)
        {
            ResultImage = new Mat().ToAvaloniaBitmap();
            return;
        }

        ResultImage = (await GetCheckResult(cropResult)).ToAvaloniaBitmap();
    }

    [TryCatch]
    [RelayCommand]
    private async Task BatchCrop()
    {
        await BatchCropExport();
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
                vm!.Results.Add(await GetCheckResult(PcbCropper.CropPcbArea1(read)!));
            }
            _mainWindowViewModel.IsLoading = false;
            window.Show();
        }
    }

    [Timing]
    private async Task<Mat> GetCheckResult(CropResult crop)
    {
        Mat cropClone3 = crop.CroppedMat.Clone();
        if (crop.CroppedMat.Channels() == 1)
        {
            Cv2.CvtColor(crop.CroppedMat, cropClone3, ColorConversionCodes.GRAY2BGR);
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

            /* 检测检测区域在原图的坐标是否准确
             * 
            if (detections.Count > 0)
            {
                var box = detections[0].BoundingBox;
                if (box is BoundingBox boxvalue)
                {
                    var rect = crop.ToOriginalCoordinates(
                        boxvalue.ToRect()
                    );

                    var check = CheckImage!.DrawRectEx(rect);
                }
            } 

            */

            return cropClone3.DrawDetections(detections, drawOptions);
        });
    }

    private async Task BatchCropExport()
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
                Cv2.ImWrite(outDir, PcbCropper.CropPcbArea1(read)!.CroppedMat);
            }
        }
    }
}
