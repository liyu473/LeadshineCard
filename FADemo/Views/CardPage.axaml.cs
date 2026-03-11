using Avalonia.Controls;
using FADemo.ViewModels;
using LyuExtensions.Aspects;

namespace FADemo.Views;

[Transient]
public partial class CardPage : UserControl
{
    [Inject]
    private readonly CardPageViewModel _vm;
    
    public CardPage()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}
