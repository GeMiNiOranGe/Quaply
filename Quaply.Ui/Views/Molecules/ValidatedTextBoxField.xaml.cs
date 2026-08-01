using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Interaction logic for ValidatedTextBoxField.xaml
/// </summary>
public partial class ValidatedTextBoxField : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(ValidatedTextBoxField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(ValidatedTextBoxField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(ValidatedTextBoxField),
            new PropertyMetadata(null)
        );

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ValidatedTextBoxField),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

    public static readonly DependencyProperty ErrorTextProperty =
        DependencyProperty.Register(
            nameof(ErrorText),
            typeof(string),
            typeof(ValidatedTextBoxField),
            new PropertyMetadata(string.Empty)
        );

    public ValidatedTextBoxField()
    {
        InitializeComponent();
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    private void ValidatedTextBoxField_OnValidationError(
        object sender,
        ValidationErrorEventArgs e
    )
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        ReadOnlyObservableCollection<ValidationError> errors =
            Validation.GetErrors(source);

        ErrorText =
            errors.Count > 0
                ? errors[0].ErrorContent?.ToString() ?? string.Empty
                : string.Empty;
    }
}
