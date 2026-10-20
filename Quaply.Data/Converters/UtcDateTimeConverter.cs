using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Quaply.Data.Converters;

public class UtcDateTimeConverter : ValueConverter<DateTime, string>
{
    public UtcDateTimeConverter()
        : base(v => ToUtcString(v), v => ParseUtc(v)) { }

    private static string ToUtcString(DateTime v)
    {
        if (v.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException(
                $"Expected DateTimeKind.Utc but got {v.Kind}. Always use DateTime.UtcNow."
            );
        }

        return v.ToString(
            Constants.SqliteDateTimeFormat,
            CultureInfo.InvariantCulture
        );
    }

    private static DateTime ParseUtc(string v)
    {
        return DateTime.SpecifyKind(
            DateTime.ParseExact(
                v,
                Constants.SqliteDateTimeFormat,
                CultureInfo.InvariantCulture
            ),
            DateTimeKind.Utc
        );
    }
}
