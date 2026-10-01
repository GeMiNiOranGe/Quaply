using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

/// <summary>
/// Maps a <see cref="double"/> input linearly from a source range
/// to a target range, clamping the input to the source range
/// before interpolating.
/// </summary>
/// <remarks>
/// <para>
/// Typical use case: deriving a control's size from an ancestor's <c>ActualWidth</c>
/// so it scales smoothly between two breakpoints instead of jumping abruptly.
/// </para>
/// <para>
/// <see cref="ConverterParameter"/> must be a semicolon-separated list of
/// <c>Key=Value</c> pairs, in any order, using invariant-culture numbers:
/// <c>"SourceMin=960;SourceMax=1280;TargetMin=160;TargetMax=320"</c>.
/// </para>
/// <para>
/// This converter is one-way only. <see cref="ConvertBack"/> always throws.
/// </para>
/// <example>
/// <code>
/// &lt;TextBox
///   Width="{Binding ActualWidth, ElementName=hostPage,
///   Converter={converters:LinearInterpolationConverter},
///   ConverterParameter='SourceMin=960;SourceMax=1280;TargetMin=160;TargetMax=320'}" /&gt;
/// </code>
/// </example>
/// </remarks>
public sealed class LinearInterpolationConverter
    : MarkupExtension,
        IValueConverter
{
    private const string SourceMinKey = "SourceMin";
    private const string SourceMaxKey = "SourceMax";
    private const string TargetMinKey = "TargetMin";
    private const string TargetMaxKey = "TargetMax";

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
        if (value is not double input)
        {
            return Binding.DoNothing;
        }

        if (
            parameter is not string paramString
            || !TryParseRanges(paramString, out var ranges)
        )
        {
            return Binding.DoNothing;
        }

        if (ranges.SourceMax <= ranges.SourceMin)
        {
            // Degenerate source range: nothing meaningful to interpolate against.
            return ranges.TargetMin;
        }

        var clampedInput = Math.Clamp(
            input,
            ranges.SourceMin,
            ranges.SourceMax
        );
        var ratio =
            (clampedInput - ranges.SourceMin)
            / (ranges.SourceMax - ranges.SourceMin);

        return ranges.TargetMin
            + (ratio * (ranges.TargetMax - ranges.TargetMin));
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException(
            $"{nameof(LinearInterpolationConverter)} does not support two-way binding."
        );
    }

    private static bool TryParseRanges(
        string paramString,
        out (
            double SourceMin,
            double SourceMax,
            double TargetMin,
            double TargetMax
        ) ranges
    )
    {
        ranges = default;

        double? sourceMin = null;
        double? sourceMax = null;
        double? targetMin = null;
        double? targetMax = null;

        foreach (
            string pair in paramString.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries
                    | StringSplitOptions.TrimEntries
            )
        )
        {
            var keyValue = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (
                keyValue.Length != 2
                || !double.TryParse(
                    keyValue[1],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var numericValue
                )
            )
            {
                return false;
            }

            switch (keyValue[0])
            {
                case SourceMinKey:
                    sourceMin = numericValue;
                    break;
                case SourceMaxKey:
                    sourceMax = numericValue;
                    break;
                case TargetMinKey:
                    targetMin = numericValue;
                    break;
                case TargetMaxKey:
                    targetMax = numericValue;
                    break;
                default:
                    // Unknown key: fail fast rather than silently ignoring a typo.
                    return false;
            }
        }

        if (
            sourceMin is null
            || sourceMax is null
            || targetMin is null
            || targetMax is null
        )
        {
            return false;
        }

        ranges = (
            sourceMin.Value,
            sourceMax.Value,
            targetMin.Value,
            targetMax.Value
        );
        return true;
    }
}
