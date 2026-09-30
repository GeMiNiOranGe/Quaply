using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

public sealed class PanelColumnWidthConverter
    : MarkupExtension,
        IMultiValueConverter
{
    private const double DefaultMinPanelWidth = 320;
    private const double DefaultMaxPanelWidth = 480;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        bool isOpen = values[0] is true;
        double width = values[1] is double d ? d : DefaultMinPanelWidth;

        if (!isOpen)
        {
            return new GridLength(0);
        }

        var (minWidth, maxWidth) = GetWidthRange(parameter, culture);

        double clamped = Math.Clamp(width, minWidth, maxWidth);

        return new GridLength(clamped, GridUnitType.Pixel);
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture
    )
    {
        double width = value is GridLength gl
            ? gl.Value
            : GetWidthRange(parameter, culture).MinWidth;

        return [Binding.DoNothing, width];
    }

    private static (double MinWidth, double MaxWidth) GetWidthRange(
        object parameter,
        CultureInfo culture
    )
    {
        if (parameter is string text)
        {
            string[] parts = text.Split(';');

            if (
                parts.Length == 2
                && double.TryParse(
                    parts[0].Trim(),
                    NumberStyles.Float,
                    culture,
                    out double minWidth
                )
                && double.TryParse(
                    parts[1].Trim(),
                    NumberStyles.Float,
                    culture,
                    out double maxWidth
                )
                && minWidth <= maxWidth
            )
            {
                return (minWidth, maxWidth);
            }
        }

        return (DefaultMinPanelWidth, DefaultMaxPanelWidth);
    }
}
