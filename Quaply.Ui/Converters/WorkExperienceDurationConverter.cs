using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Quaply.Data.Models;

namespace Quaply.Ui.Converters;

public class WorkExperienceDurationConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is not WorkExperience workExperience)
        {
            return string.Empty;
        }

        string start = workExperience.StartDate.ToString("MMM yyyy", culture);
        string end =
            workExperience.EndDate?.ToString("MMM yyyy", culture) ?? "Present";

        return $"{start} \u2013 {end}";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
