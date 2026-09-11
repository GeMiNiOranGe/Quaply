using System.ComponentModel.DataAnnotations;

namespace Quaply.Ui.Validations.Base;

[AttributeUsage(AttributeTargets.Property)]
public abstract class OptionalValidationAttribute : ValidationAttribute
{
    protected abstract ValidationAttribute Inner { get; }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    )
    {
        if (
            value is null
            || (value is string s && string.IsNullOrWhiteSpace(s))
        )
        {
            return ValidationResult.Success;
        }

        if (Inner.IsValid(value))
        {
            return ValidationResult.Success;
        }

        string[] memberNames = validationContext.MemberName is { } name
            ? [name]
            : [];

        return new ValidationResult(
            ErrorMessage ?? "Value is not valid.",
            memberNames
        );
    }
}
