using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

/// <summary>
/// Converts a selection count into a Thickness, adding extra bottom inset
/// to a base padding when the count is greater than zero.
/// </summary>
/// <remarks>
/// Used to reserve space under content only while a floating overlay
/// (e.g. a bulk-action bar) is visible.
/// </remarks>
/// <param name="value">
/// The count (int). Extra inset applies when &gt; 0.
/// </param>
/// <param name="parameter">
/// Base Thickness as "left,top,right,bottom", optionally with extra amount
/// appended: "0,0,0,0|72". Defaults to "0,0,0,0" and extra 72.
/// </param>
/// <returns>
/// The base Thickness, with extra bottom inset added when count &gt; 0.
/// </returns>
public class ConditionalBottomInsetConverter : MarkupExtension, IValueConverter
{
    private const double DefaultExtra = 72;

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
        int count = value is int c ? c : 0;

        (Thickness baseThickness, double extra) = ParseParameter(
            parameter as string
        );

        return count > 0
            ? new Thickness(
                baseThickness.Left,
                baseThickness.Top,
                baseThickness.Right,
                baseThickness.Bottom + extra
            )
            : baseThickness;
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

    private static (Thickness Base, double Extra) ParseParameter(
        string? parameter
    )
    {
        if (string.IsNullOrWhiteSpace(parameter))
        {
            return (new Thickness(0), DefaultExtra);
        }

        string[] parts = parameter.Split('|');
        Thickness thickness = (Thickness)
            ThicknessConverterHelper.Parse(parts[0]);
        double extra =
            parts.Length > 1
            && double.TryParse(
                parts[1],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var e
            )
                ? e
                : DefaultExtra;

        return (thickness, extra);
    }
}

internal static class ThicknessConverterHelper
{
    private static readonly ThicknessConverter Converter = new();

    public static object Parse(string value)
    {
        return Converter.ConvertFromString(value)!;
    }
}
