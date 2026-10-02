using System.Windows;
using UiButton = Wpf.Ui.Controls.Button;

namespace Quaply.Ui.Views.Molecules;

public sealed class SegmentedButtonItem : UiButton
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(SegmentedButtonItem)
        );

    public SegmentedButtonItem()
    {
        // Same style as a plain ui:Button, resolved at runtime
        // (no merge-order dependency).
        SetResourceReference(StyleProperty, typeof(UiButton));
        SetCurrentValue(PaddingProperty, new Thickness(16, 6, 16, 6));
    }

    /// <summary>
    /// Value this segment represents (used when declared directly in XAML).
    /// </summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}
