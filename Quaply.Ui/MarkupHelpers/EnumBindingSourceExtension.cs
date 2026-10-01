using System.Windows.Markup;

namespace Quaply.Ui.MarkupHelpers;

/// <summary>
/// Markup extension that returns all values of an enum type, used as
/// <c>ItemsSource</c> for a <see cref="ComboBox"/>/<see cref="ItemsControl"/>
/// without exposing a dedicated property on the ViewModel.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// ItemsSource="{markup:EnumBindingSource {x:Type querying:RelativeDateRange}}"
/// </code>
/// </remarks>
public class EnumBindingSourceExtension : MarkupExtension
{
    public Type? EnumType
    {
        get;
        set
        {
            if (value != field)
            {
                if (value != null)
                {
                    // Allow Nullable<enum> as well as a plain enum type
                    Type underlyingType =
                        Nullable.GetUnderlyingType(value) ?? value;

                    if (!underlyingType.IsEnum)
                    {
                        throw new ArgumentException(
                            "EnumType must be an enum type (or a Nullable<enum>)."
                        );
                    }
                }

                field = value;
            }
        }
    }

    public EnumBindingSourceExtension() { }

    public EnumBindingSourceExtension(Type enumType)
    {
        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (EnumType == null)
        {
            throw new InvalidOperationException("EnumType has not been set.");
        }

        Type actualEnumType = Nullable.GetUnderlyingType(EnumType) ?? EnumType;
        Array enumValues = Enum.GetValues(actualEnumType);

        // If EnumType is Nullable<enum>, prepend a null entry
        // (useful for a ComboBox with an "All" / unset option)
        if (actualEnumType == EnumType)
        {
            return enumValues;
        }

        Array resultArray = Array.CreateInstance(
            actualEnumType,
            enumValues.Length + 1
        );
        enumValues.CopyTo(resultArray, 1);
        return resultArray;
    }
}
