using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

/// <summary>
/// Converts a UTC DateTime to a user-friendly relative time string (e.g. "5 minutes ago").
/// Falls back to an absolute local date for values older than 2 days.
/// </summary>
public sealed class RelativeDateTimeConverter : MarkupExtension, IValueConverter
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

        // Defensive check: EF model should already guarantee Utc kind,
        // but this converter may receive data from other sources too.
        if (utcDate.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException(
                $"{nameof(RelativeDateTimeConverter)} expects {nameof(DateTimeKind)}.{nameof(DateTimeKind.Utc)} but got {utcDate.Kind}."
            );
        }

        // Convert to local time before computing the diff and before display.
        DateTime localDate = utcDate.ToLocalTime();
        TimeSpan diff = DateTime.Now - localDate;

        return diff switch
        {
            { TotalSeconds: < 60 } => "Just now",
            { TotalMinutes: < 2 } => "1 minute ago",
            { TotalMinutes: < 60 } => $"{(int)diff.TotalMinutes} minutes ago",
            { TotalHours: < 2 } => "1 hour ago",
            { TotalHours: < 24 } => $"{(int)diff.TotalHours} hours ago",
            { TotalDays: < 2 } => $"Yesterday at {localDate:h:mm tt}",
            _ => FormatAbsoluteDate(localDate),
        };
    }

    /// <summary>
    /// Formats an absolute date in English convention, e.g. "Aug 15" for the
    /// current year, or "Aug 15, 2024" when the year differs from now.
    /// </summary>
    private static string FormatAbsoluteDate(DateTime localDate)
    {
        CultureInfo enCulture = CultureInfo.InvariantCulture;

        return localDate.Year == DateTime.Now.Year
            ? localDate.ToString("MMM d", enCulture)
            : localDate.ToString("MMM d, yyyy", enCulture);
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
