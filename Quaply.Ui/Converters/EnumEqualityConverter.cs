using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

public sealed class EnumEqualityConverter
    : MarkupExtension,
        IValueConverter,
        IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    // Single: value == ConverterParameter (static value, x:Static)
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        return value?.Equals(parameter) ?? false;
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

    // Multi: values[0] == values[1] (Both are binding)
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        if (
            values.Length != 2
            || values[0] == DependencyProperty.UnsetValue
            || values[1] == DependencyProperty.UnsetValue
        )
        {
            return false;
        }

        return Equals(values[0], values[1]);
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
