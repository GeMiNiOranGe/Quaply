using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Quaply.Ui.Extensions;

public static class EnumExtensions
{
    public static string ToDisplayName(this Enum value)
    {
        FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());

        DisplayAttribute? attribute =
            fieldInfo?.GetCustomAttribute<DisplayAttribute>();

        return attribute?.Name ?? value.ToString();
    }
}
