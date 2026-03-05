using Avalonia.Collections;
using Avalonia.Controls;
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
}
