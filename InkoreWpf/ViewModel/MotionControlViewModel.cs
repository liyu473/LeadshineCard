using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LeadshineCard.Core.Interfaces;
using LeadshineCard.Extensions;
using LyuExtensions.Aspects;
using LyuExtensions.Extensions;
using LyuWpfHelper.ViewModels;

namespace InkoreWpf.ViewModel;

[Singleton]
public partial class MotionControlViewModel : ViewModelBase
{
    [Inject]
    private readonly IMotionCardManager _manager;

    public ObservableCollection<IMotionCard> MotionCards { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AxisSource))]
    public partial IMotionCard? SelectedCard { get; set; }
    partial void OnSelectedCardChanged(IMotionCard? value)
    {
        SelectedAxis = null;
    }

    public ObservableCollection<IAxisController> AxisSource =>
        SelectedCard is null ? [] : new(SelectedCard.GetAxises());

    [ObservableProperty]
    public partial IAxisController? SelectedAxis { get; set; }

    public MotionControlViewModel()
    {
        _manager!.GetInitializedCards().ForEach(card => MotionCards.Add(card));
    }
}
