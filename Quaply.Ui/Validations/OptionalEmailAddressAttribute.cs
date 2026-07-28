using System.ComponentModel.DataAnnotations;
using Quaply.Ui.Validations.Base;

namespace Quaply.Ui.Validations;

public sealed class OptionalEmailAddressAttribute : OptionalValidationAttribute
{
    private readonly ValidationAttribute _inner = new EmailAddressAttribute();

    protected override ValidationAttribute Inner => _inner;
}
