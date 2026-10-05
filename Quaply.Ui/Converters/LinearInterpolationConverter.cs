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
    /// <summary>
    /// The four interpolation bounds plus the optional rounding flag,
    /// parsed from a <see cref="LinearInterpolationConverter"/>'s
    /// <c>ConverterParameter</c>.
    /// </summary>
    /// <param name="SourceMin">Lower bound of the input range.</param>
    /// <param name="SourceMax">Upper bound of the input range.</param>
    /// <param name="TargetMin">Lower bound of the output range.</param>
    /// <param name="TargetMax">Upper bound of the output range.</param>
    /// <param name="Round">
    /// Whether the interpolated result should be rounded to the nearest integer
    /// (away from zero) before being returned.
    /// </param>
    private readonly record struct InterpolationRange(
        double SourceMin,
        double SourceMax,
        double TargetMin,
        double TargetMax,
        bool Round
    );

    /// <summary>
    /// Mutable accumulator used only while parsing a <c>ConverterParameter</c> string.
    /// Exists so the static setter delegates below have something to write into
    /// without capturing per-call local variables.
    /// </summary>
    private struct ParsedFields
    {
        public double? SourceMin;
        public double? SourceMax;
        public double? TargetMin;
        public double? TargetMax;
        public bool Round;
    }

    private const string SourceMinKey = "SourceMin";
    private const string SourceMaxKey = "SourceMax";
    private const string TargetMinKey = "TargetMin";
    private const string TargetMaxKey = "TargetMax";
    private const string RoundKey = "Round";

    private delegate void DoubleSetter(ref ParsedFields fields, double value);
    private delegate void BoolSetter(ref ParsedFields fields, bool value);

    private static readonly Dictionary<string, DoubleSetter> DoubleSetters =
        new()
        {
            [SourceMinKey] = (ref fields, value) => fields.SourceMin = value,
            [SourceMaxKey] = (ref fields, value) => fields.SourceMax = value,
            [TargetMinKey] = (ref fields, value) => fields.TargetMin = value,
            [TargetMaxKey] = (ref fields, value) => fields.TargetMax = value,
        };

    private static readonly Dictionary<string, BoolSetter> BoolSetters = new()
    {
        [RoundKey] = (ref fields, value) => fields.Round = value,
    };

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
            || !TryParseRanges(paramString, out InterpolationRange range)
        )
        {
            return Binding.DoNothing;
        }

        // deconstruct the 'range' into local variables for clarity
        (
            double sourceMin,
            double sourceMax,
            double targetMin,
            double targetMax,
            bool round
        ) = range;

        if (sourceMax <= sourceMin)
        {
            // Degenerate source range: nothing meaningful to interpolate against.
            return targetMin;
        }

        double clampedInput = Math.Clamp(input, sourceMin, sourceMax);
        double ratio = (clampedInput - sourceMin) / (sourceMax - sourceMin);
        double result = targetMin + (ratio * (targetMax - targetMin));

        return round
            ? Math.Round(result, MidpointRounding.AwayFromZero)
            : result;
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
        out InterpolationRange range
    )
    {
        range = default;
        ParsedFields fields = new();

        foreach (
            string pair in paramString.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries
                    | StringSplitOptions.TrimEntries
            )
        )
        {
            string[] keyValue = pair.Split(
                '=',
                2,
                StringSplitOptions.TrimEntries
            );
            if (keyValue.Length != 2)
            {
                return false;
            }

            string key = keyValue[0];
            string rawValue = keyValue[1];

            if (DoubleSetters.TryGetValue(key, out DoubleSetter? setDouble))
            {
                if (
                    !double.TryParse(
                        rawValue,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double numericValue
                    )
                )
                {
                    return false;
                }
                setDouble(ref fields, numericValue);
            }
            else if (BoolSetters.TryGetValue(key, out BoolSetter? setBool))
            {
                if (!bool.TryParse(rawValue, out bool booleanValue))
                {
                    return false;
                }
                setBool(ref fields, booleanValue);
            }
            else
            {
                // Unknown key: fail fast rather than silently ignoring a typo.
                return false;
            }
        }

        if (
            fields.SourceMin is null
            || fields.SourceMax is null
            || fields.TargetMin is null
            || fields.TargetMax is null
        )
        {
            return false;
        }

        range = new InterpolationRange(
            fields.SourceMin.Value,
            fields.SourceMax.Value,
            fields.TargetMin.Value,
            fields.TargetMax.Value,
            fields.Round
        );
        return true;
    }
}
