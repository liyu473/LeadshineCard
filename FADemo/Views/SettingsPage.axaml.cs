using Avalonia.Controls;
using FADemo.ViewModels;
using LyuExtensions.Aspects;

namespace FADemo.Views;

[Transient]
public partial class SettingsPage : UserControl
{
    [Inject] 
    private readonly SettingsPageViewModel _vm;
    
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}
