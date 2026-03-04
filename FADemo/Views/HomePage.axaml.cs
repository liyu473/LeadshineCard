using Avalonia.Controls;
using FADemo.ViewModels;
using LyuExtensions.Aspects;

namespace FADemo.Views;

[Transient]
public partial class HomePage : UserControl
{
    [Inject]
    private readonly HomePageViewModel _vm;
    
    public HomePage()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}
