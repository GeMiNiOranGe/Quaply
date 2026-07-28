using System.ComponentModel.DataAnnotations;
using Quaply.Ui.Validations.Base;

namespace Quaply.Ui.Validations;

public sealed class OptionalPhoneAttribute : OptionalValidationAttribute
{
    private readonly ValidationAttribute _inner = new PhoneAttribute();

    protected override ValidationAttribute Inner => _inner;
}
