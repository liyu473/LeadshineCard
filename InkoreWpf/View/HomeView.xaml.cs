using InkoreWpf.ViewModel;
using LyuExtensions.Aspects;
using System.Windows.Controls;

namespace InkoreWpf.View;

[Transient]
public partial class HomeView : UserControl
{
    [Inject]
    private readonly HomeViewModel _vm;

    public HomeView()
    {
        InitializeComponent();
    }
}
