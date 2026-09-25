using System.ComponentModel.DataAnnotations;

namespace Quaply.Data.Querying;

public enum WorkExperienceSortField
{
    [Display(Name = "Deleted date")]
    DeletedAt,

    [Display(Name = "Company name")]
    CompanyName,
}
