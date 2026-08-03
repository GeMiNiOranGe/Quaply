using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

public sealed class DateOnlyToDateTimeConverter
    : MarkupExtension,
        IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object? Convert(object? value, Type t, object? p, CultureInfo c)
    {
        return value is DateOnly d ? d.ToDateTime(TimeOnly.MinValue) : null;
    }

    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c)
    {
        return value is DateTime dt ? DateOnly.FromDateTime(dt) : null;
    }
}
