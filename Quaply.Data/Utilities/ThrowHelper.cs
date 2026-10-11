namespace Quaply.Data.Utilities;

internal static class ThrowHelper
{
    public static ArgumentOutOfRangeException InvalidEnumValue<TEnum>(
        TEnum value,
        string paramName
    )
        where TEnum : struct, Enum
    {
        return new ArgumentOutOfRangeException(
            paramName,
            value,
            $"Invalid {typeof(TEnum).Name}: {value}"
        );
    }
}
