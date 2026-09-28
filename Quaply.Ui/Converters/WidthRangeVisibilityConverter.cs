using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

/// <summary>
/// Determines whether <see cref="ActualWidth"/> falls within the range
///  <c>[min, max)</c>.Empty bounds indicate no limit on that side.
/// <para>
/// The <see cref="ConverterParameter"/> format is <c>"min;max"</c>.
/// For example: <c>"0;900"</c> matches widths below 900,
/// <c>"900;1100"</c> matches widths from 900 (inclusive) to 1100 (exclusive),
/// and <c>"1100;"</c> matches widths of 1100 or greater.
/// </para>
/// </summary>
public sealed class WidthRangeVisibilityConverter
    : MarkupExtension,
        IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        double width = value is double d ? d : 0;

        (double min, double max) = ParseRange(parameter as string);

        bool isVisible = width >= min && width < max;

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }

    private static (double Min, double Max) ParseRange(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return (0, double.PositiveInfinity);
        }

        string[] parts = raw.Split(';');

        double min =
            parts.Length > 0 && double.TryParse(parts[0], out double minVal)
                ? minVal
                : 0;

        double max =
            parts.Length > 1 && double.TryParse(parts[1], out double maxVal)
                ? maxVal
                : double.PositiveInfinity;

        return (min, max);
    }
}
