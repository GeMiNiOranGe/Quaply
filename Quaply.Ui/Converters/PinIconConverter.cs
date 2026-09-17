using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Converters;

/// <summary>
/// Chevron flips direction based on the pin state:
/// unpinned points right (to expand), pinned points left (to unpin).
/// </summary>
public class PinIconConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return value is true ? SymbolRegular.PinOff24 : SymbolRegular.Pin24;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
