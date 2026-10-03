using System.ComponentModel;
using System.Globalization;

namespace Quaply.Ui.Foundations;

/// <summary>
/// A set of width breakpoints that maps a width to a <see cref="SizeClass"/>.
/// <para>
/// <see cref="Default"/> is the app-wide set used by every
/// <see cref="AdaptiveContent"/> that does not specify its own.
/// A control can override it in XAML with a string holding the lower bound of
/// each size class, in order <c>Compact;Medium;Expanded</c>:
/// <list type="bullet">
///   <item><c>Breakpoints="0;500;700"</c>: three size classes.</item>
///   <item>
///     <c>Breakpoints="0;500"</c>: two size classes. There is no Expanded,
///     so <see cref="Expanded"/> is <see cref="double.PositiveInfinity"/>
///     and that size class is never reached.
///   </item>
/// </list>
/// </para>
/// <para>
/// Ranges are <c>[min, max)</c>:
/// <list type="bullet">
///   <item>
///     <term>Compact</term>
///     <description>width &lt; <see cref="Medium"/>.</description>
///   </item>
///   <item>
///     <term>Medium</term>
///     <description>
///       <see cref="Medium"/> &lt;= width &lt; <see cref="Expanded"/>.
///     </description>
///   </item>
///   <item>
///     <term>Expanded</term>
///     <description><see cref="Expanded"/> &lt;= width.</description>
///   </item>
/// </list>
/// </para>
/// An inconsistent set (not strictly ascending) is rejected when it is created,
/// so it can never cause two layouts to overlap or leave a gap.
/// </summary>
[TypeConverter(typeof(BreakpointSetConverter))]
public sealed class BreakpointSet
{
    private const int MinValueCount = 2;
    private const int MaxValueCount = 3;

    /// <summary>
    /// App-wide default breakpoints.
    /// </summary>
    public static BreakpointSet Default { get; } =
        new(medium: 1120, expanded: 1440);

    /// <param name="medium">Lower bound of Medium.</param>
    /// <param name="expanded">
    /// Lower bound of Expanded. Pass <see cref="double.PositiveInfinity"/>
    /// for a set without an Expanded size class.
    /// </param>
    public BreakpointSet(double medium, double expanded)
    {
        // Written as a negated "&&" so that NaN is rejected too.
        if (!(0 < medium && medium < expanded))
        {
            throw new ArgumentException(
                $"Invalid breakpoints: require 0 < Medium ({medium}) < Expanded ({expanded})."
            );
        }

        Medium = medium;
        Expanded = expanded;
    }

    /// <summary>
    /// Minimum width (inclusive) for <see cref="SizeClass.Medium"/>.
    /// </summary>
    public double Medium { get; }

    /// <summary>
    /// Minimum width (inclusive) for <see cref="SizeClass.Expanded"/>.
    /// <see cref="double.PositiveInfinity"/> when the set has no Expanded.
    /// </summary>
    public double Expanded { get; }

    public SizeClass Resolve(double width)
    {
        return width >= Expanded ? SizeClass.Expanded
            : width >= Medium ? SizeClass.Medium
            : SizeClass.Compact;
    }

    /// <summary>
    /// Parses <c>"0;500;700"</c> or <c>"0;500"</c>: the lower bound of Compact
    /// (must be 0), Medium and, optionally, Expanded.
    /// Uses the invariant culture.
    /// </summary>
    public static BreakpointSet Parse(string text)
    {
        string[] parts = text.Split(';', StringSplitOptions.TrimEntries);

        if (parts.Length is < MinValueCount or > MaxValueCount)
        {
            throw new FormatException(
                $"Expected 2 or 3 values 'Compact;Medium[;Expanded]' (e.g. \"0;500;700\" or \"0;500\"), got {parts.Length}: \"{text}\"."
            );
        }

        double[] values = new double[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (
                !double.TryParse(
                    parts[i],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out values[i]
                )
            )
            {
                throw new FormatException(
                    $"'{parts[i]}' is not a valid number in breakpoints \"{text}\"."
                );
            }
        }

        if (values[0] != 0)
        {
            throw new FormatException(
                $"The first value (Compact) must be 0, got {values[0]}."
            );
        }

        double expanded =
            parts.Length == MaxValueCount ? values[2] : double.PositiveInfinity;

        return new BreakpointSet(values[1], expanded);
    }
}

/// <summary>
/// Lets XAML write <c>Breakpoints="0;500;700"</c> directly as an attribute.
/// </summary>
public sealed class BreakpointSetConverter : TypeConverter
{
    public override bool CanConvertFrom(
        ITypeDescriptorContext? context,
        Type sourceType
    )
    {
        return sourceType == typeof(string)
            || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value
    )
    {
        return value is string text
            ? BreakpointSet.Parse(text)
            : base.ConvertFrom(context, culture, value);
    }
}
