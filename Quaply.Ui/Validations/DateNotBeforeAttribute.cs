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

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    )
    {
        if (value is not DateOnly endValue)
        {
            return ValidationResult.Success;
        }

        PropertyInfo? comparisonProperty =
            validationContext.ObjectType.GetProperty(_comparisonPropertyName);

        if (comparisonProperty is null)
        {
            return new ValidationResult(
                $"Unknown property: {_comparisonPropertyName}"
            );
        }

        if (
            comparisonProperty.GetValue(validationContext.ObjectInstance)
            is not DateOnly startValue
        )
        {
            return ValidationResult.Success;
        }

        return endValue >= startValue
            ? ValidationResult.Success
            : new ValidationResult(
                ErrorMessage ?? "End date cannot be earlier than start date.",
                [validationContext.MemberName!]
            );
    }
}
