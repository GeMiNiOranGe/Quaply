using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

/// <summary>
/// Converts a UTC DateTime to a full local timestamp string for tooltips,
/// e.g. "Tuesday, August 18, 2026 at 6:43 PM GMT+7".
/// </summary>
public class FullLocalDateTimeConverter : MarkupExtension, IValueConverter
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
        if (value is not DateTime utcDate)
        {
            return string.Empty;
        }

        DateTime localDate = utcDate.ToLocalTime();
        TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(utcDate);
        string offsetText = FormatOffset(offset);

        // Force English (invariant culture) regardless of OS/user locale.
        CultureInfo enCulture = CultureInfo.InvariantCulture;

        return string.Format(
            enCulture,
            "{0:dddd}, {0:MMMM} {0:d}, {0:yyyy} at {0:h:mm tt} GMT{1}",
            localDate,
            offsetText
        );
    }

    private static string FormatOffset(TimeSpan offset)
    {
        string sign = offset < TimeSpan.Zero ? "-" : "+";
        TimeSpan abs = offset.Duration();

        // Show minutes only when non-zero (e.g. GMT+7 vs GMT+5:30).
        return abs.Minutes == 0
            ? $"{sign}{abs.Hours}"
            : $"{sign}{abs.Hours}:{abs.Minutes:D2}";
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
