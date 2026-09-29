using System.Windows;

namespace Quaply.Ui.MarkupHelpers;

public class BindingProxy : Freezable
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null)
        );

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    // Mandatory override - Freezable requires this method to clone itself.
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}
