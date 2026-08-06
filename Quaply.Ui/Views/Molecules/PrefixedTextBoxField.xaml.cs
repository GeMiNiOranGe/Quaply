using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Interaction logic for PrefixedTextBoxField.xaml
/// </summary>
public partial class PrefixedTextBoxField : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(PrefixedTextBoxField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.Register(
            nameof(Prefix),
            typeof(string),
            typeof(PrefixedTextBoxField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(PrefixedTextBoxField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(PrefixedTextBoxField),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

    public PrefixedTextBoxField()
    {
        InitializeComponent();
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
