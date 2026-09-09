using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

public class InverseBooleanToVisibilityConverter
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
        bool flag = false;

        if (value is bool b)
        {
            flag = b;
        }
        else if (value is bool?)
        {
            flag = (bool?)value == true;
        }

        // Invert the original logic
        return flag ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        return false;
    }
}
