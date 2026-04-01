using InkoreWpf.ViewModel;
using LyuExtensions.Aspects;
using System.Windows.Controls;

namespace InkoreWpf.View;

[Transient]
public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
