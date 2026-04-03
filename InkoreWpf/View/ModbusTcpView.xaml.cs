using System.Windows.Controls;
using InkoreWpf.ViewModel;
using LyuExtensions.Aspects;

namespace InkoreWpf.View;

/// <summary>
/// CommunicationView.xaml 的交互逻辑
/// </summary>
[Transient]
public partial class ModbusTcpView : UserControl
{
    [Inject]
    private readonly ModbusTcpViewModel _vm;
    public ModbusTcpView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}
