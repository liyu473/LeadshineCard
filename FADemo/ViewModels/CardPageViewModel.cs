using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using LyuExtensions.Aspects;
using Microsoft.Extensions.Logging;

namespace FADemo.ViewModels;

[Singleton]
public partial class CardPageViewModel : ViewModelBase
{
    [Inject]
    private readonly ILogger<CardPageViewModel> _logger;

    [TryCatch]
    [RelayCommand]
    private async Task ConnectCard()
    {

    }

    [TryCatch]
    [RelayCommand]
    private async Task DisconnectCard()
    {

    }

    [TryCatch]
    [RelayCommand]
    private async Task MoveAxis()
    {

    }

    [TryCatch]
    [RelayCommand]
    private async Task StopAxis()
    {

    }

    [TryCatch]
    [RelayCommand]
    private async Task HomeAxis()
    {

    }
}
