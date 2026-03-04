using CommunityToolkit.Mvvm.ComponentModel;
using LyuExtensions.Aspects;

namespace FADemo.ViewModels;


[Singleton]
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
}