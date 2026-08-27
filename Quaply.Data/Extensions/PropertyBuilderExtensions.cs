using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quaply.Data.Converters;

namespace Quaply.Data.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<DateTime> HasUtcConversion(
        this PropertyBuilder<DateTime> entityBuilder
    )
    {
        return entityBuilder.HasConversion<UtcDateTimeConverter>();
    }

    public static PropertyBuilder<DateTime?> HasUtcConversion(
        this PropertyBuilder<DateTime?> entityBuilder
    )
    {
        return entityBuilder.HasConversion<NullableUtcDateTimeConverter>();
    }
}
