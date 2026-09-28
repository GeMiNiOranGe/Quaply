using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Quaply.Ui.Converters;

public sealed class EnumToButtonAppearanceConverter
    : MarkupExtension,
        IValueConverter
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
        return value?.Equals(parameter) == true
            ? ControlAppearance.Secondary
            : ControlAppearance.Transparent;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
