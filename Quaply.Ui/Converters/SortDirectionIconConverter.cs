using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Converters;

/// <summary>
/// Reflects the current sort state (not the click action): descending shows a
/// down arrow, ascending shows an up arrow.
/// </summary>
public class SortDirectionIconConverter : MarkupExtension, IValueConverter
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
        return value is true
            ? SymbolRegular.ArrowSortDown24
            : SymbolRegular.ArrowSortUp24;
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
