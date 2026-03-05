using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using LyuExtensions.Aspects;
using OpenCvSharp;

namespace FADemo.ViewModels;

[Transient]
public partial class BatchResultViewModel:ViewModelBase
{
    public AvaloniaList<Mat> Results { get; } = [];

    [RelayCommand]
    private void ShowResults(Mat mat)
    {
        Cv2.ImShow("Results", mat);
    }
}
