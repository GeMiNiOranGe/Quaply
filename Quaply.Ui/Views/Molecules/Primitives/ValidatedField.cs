using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules.Primitives;

public class ValidatedField : ContentControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(ValidatedField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty ErrorTextProperty =
        DependencyProperty.Register(
            nameof(ErrorText),
            typeof(string),
            typeof(ValidatedField),
            new PropertyMetadata(string.Empty)
        );

    static ValidatedField()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ValidatedField),
            new FrameworkPropertyMetadata(typeof(ValidatedField))
        );
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }
}
