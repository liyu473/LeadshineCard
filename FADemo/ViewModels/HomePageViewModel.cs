using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FADemo.Helpers;
using LyuExtensions.Aspects;

namespace FADemo.ViewModels;

[Singleton]
public partial class HomePageViewModel : ViewModelBase
{
    [RelayCommand]
    public async Task ImportImages()
    {
        
    }
}
