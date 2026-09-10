using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Quaply.Ui.Views.Molecules;

/// <summary>
/// Interaction logic for ValidatedDatePickerField.xaml
/// </summary>
public partial class ValidatedDatePickerField : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(ValidatedDatePickerField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(ValidatedDatePickerField),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            )
        );

    public static readonly DependencyProperty ErrorTextProperty =
        DependencyProperty.Register(
            nameof(ErrorText),
            typeof(string),
            typeof(ValidatedDatePickerField),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty IsValueEditableProperty =
        DependencyProperty.Register(
            nameof(IsValueEditable),
            typeof(bool),
            typeof(ValidatedDatePickerField),
            new PropertyMetadata(true)
        );

    public ValidatedDatePickerField()
    {
        InitializeComponent();
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>
    /// Cho phép disable DatePicker bên trong (ví dụ: EndDate khi IsCurrentlyWorking = true)
    /// mà không đụng vào UserControl.IsEnabled (vốn ảnh hưởng cả Label lẫn ErrorText).
    /// </summary>
    public bool IsValueEditable
    {
        get => (bool)GetValue(IsValueEditableProperty);
        set => SetValue(IsValueEditableProperty, value);
    }

    private void ValidatedDatePickerField_OnValidationError(
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
