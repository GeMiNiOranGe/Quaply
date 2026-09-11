using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Quaply.Ui.Validations;

/// <summary>
/// Validates that the decorated <see cref="DateOnly"/>? property is not
/// earlier than another DateOnly? property on the same object.
/// Passes when either value is null (use [Required] separately if needed).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DateNotBeforeAttribute(string comparisonPropertyName)
    : ValidationAttribute
{
    private readonly string _comparisonPropertyName = comparisonPropertyName;
    private static readonly ConcurrentDictionary<
        (Type, string),
        PropertyInfo?
    > PropertyCache = new();

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    )
    {
        if (value is not DateOnly endValue)
        {
            return ValidationResult.Success;
        }

        // A developer configuration error, not a user data error.
        PropertyInfo? property =
            PropertyCache.GetOrAdd(
                (validationContext.ObjectType, _comparisonPropertyName),
                key => key.Item1.GetProperty(key.Item2)
            )
            ?? throw new InvalidOperationException(
                $"{nameof(DateNotBeforeAttribute)}: property '{_comparisonPropertyName}' not found on {validationContext.ObjectType.Name}."
            );

        object? rawValue = property.GetValue(validationContext.ObjectInstance);

        if (rawValue is null)
        {
            return ValidationResult.Success;
        }

        if (rawValue is not DateOnly startValue)
        {
            throw new InvalidOperationException(
                $"{nameof(DateNotBeforeAttribute)}: property '{_comparisonPropertyName}' must be of type DateOnly or DateOnly?, but was {rawValue.GetType().Name}."
            );
        }

        if (endValue >= startValue)
        {
            return ValidationResult.Success;
        }

        string[] memberNames = validationContext.MemberName is { } name
            ? [name]
            : [];

        return new ValidationResult(
            ErrorMessage ?? "End date cannot be earlier than start date.",
            memberNames
        );
    }
}
