using System.ComponentModel.DataAnnotations;

namespace Quaply.Data.Querying.WorkExperiences;

public enum DeletedWorkExperienceSortField
{
    [Display(Name = "Deleted date")]
    DeletedAt,

    [Display(Name = "Company name")]
    CompanyName,

    [Display(Name = "Position title")]
    PositionTitle,
}
