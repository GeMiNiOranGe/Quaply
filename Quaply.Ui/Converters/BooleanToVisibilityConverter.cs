using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Quaply.Ui.Converters;

/// <summary>
///     Represents the converter that converts Boolean values to and from
///     System.Windows.Visibility enumeration values.
/// </summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public sealed class BooleanToVisibilityConverter
    : MarkupExtension,
        IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <summary>
    ///     Converts a Boolean value to
    ///     a System.Windows.Visibility enumeration value.
    /// </summary>
    /// <param name="value">
    ///     The Boolean value to convert. This value can be
    ///     a standard Boolean value or a nullable Boolean value.
    /// </param>
    /// <param name="targetType">
    ///     This parameter is not used.
    /// </param>
    /// <param name="parameter">
    ///     This parameter is not used.
    /// </param>
    /// <param name="culture">
    ///     This parameter is not used.
    /// </param>
    /// <returns>
    ///     System.Windows.Visibility.Visible if value is true;
    ///     otherwise, System.Windows.Visibility.Collapsed.
    /// </returns>
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        bool flag = false;

        if (value is bool v)
        {
            flag = v;
        }

        return (!flag) ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    ///     Converts a System.Windows.Visibility enumeration value to
    ///     a Boolean value.
    /// </summary>
    /// <param name="value">
    ///     A System.Windows.Visibility enumeration value.
    /// </param>
    /// <param name="targetType">
    ///     This parameter is not used.
    /// </param>
    /// <param name="parameter">
    ///     This parameter is not used.
    /// </param>
    /// <param name="culture">
    ///     This parameter is not used.
    /// </param>
    /// <returns>
    ///     true if value is System.Windows.Visibility.Visible;
    ///     otherwise, false.
    /// </returns>
    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    )
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }

        return false;
    }
}
