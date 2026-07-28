using System.ComponentModel.DataAnnotations;

namespace Quaply.Ui.Validations.Base;

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

        return Inner.IsValid(value)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage ?? "Value is not valid.");
    }
}
