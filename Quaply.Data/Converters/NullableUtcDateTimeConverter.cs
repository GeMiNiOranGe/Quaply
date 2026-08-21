using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Quaply.Data.Converters;

public class NullableUtcDateTimeConverter()
    : ValueConverter<DateTime?, string?>(v => ToUtcString(v), v => ParseUtc(v))
{
    private static string? ToUtcString(DateTime? v)
    {
        if (!v.HasValue)
        {
            return null;
        }

        if (v.Value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException(
                $"Expected DateTimeKind.Utc but got {v.Value.Kind}. Always use DateTime.UtcNow."
            );
        }

        return v.Value.ToString(
            Constants.Iso8601Format,
            CultureInfo.InvariantCulture
        );
    }

    private static DateTime? ParseUtc(string? v)
    {
        if (v is null)
        {
            return null;
        }

        return DateTime.SpecifyKind(
            DateTime.ParseExact(
                v,
                Constants.Iso8601Format,
                CultureInfo.InvariantCulture
            ),
            DateTimeKind.Utc
        );
    }
}
