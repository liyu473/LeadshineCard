using System.Windows;
using System.Windows.Controls;

namespace InkoreWpf.Controls;

public partial class SectionHeader : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SectionHeader),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty BarHeightProperty =
        DependencyProperty.Register(
            nameof(BarHeight),
            typeof(double),
            typeof(SectionHeader),
            new PropertyMetadata(26.0, OnBarHeightChanged));

    public static readonly DependencyProperty TitleFontSizeProperty =
        DependencyProperty.Register(
            nameof(TitleFontSize),
            typeof(double),
            typeof(SectionHeader),
            new PropertyMetadata(24.0, OnFontSizeChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public SectionHeader()
    {
        InitializeComponent();
    }

    private static void OnBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SectionHeader header)
        {
            header.AccentBar.Height = (double)e.NewValue;
        }
    }

    private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SectionHeader header)
        {
            header.TitleText.FontSize = (double)e.NewValue;
        }
    }
}
