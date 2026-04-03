using System.Windows.Controls;
using InkoreWpf.ViewModel;
using LyuExtensions.Aspects;

namespace InkoreWpf.View;

[Transient]
public partial class MotionControlView : UserControl
{
    [Inject]
    private readonly MotionControlViewModel _vm;
    public MotionControlView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}
