using System.ComponentModel.DataAnnotations;
using Quaply.Ui.Validations.Base;

namespace Quaply.Ui.Validations;

public sealed class OptionalUrlAttribute : OptionalValidationAttribute
{
    private readonly ValidationAttribute _inner = new UrlAttribute();

    protected override ValidationAttribute Inner => _inner;
}
