using System.ComponentModel.DataAnnotations;

namespace Quaply.Data.Querying.WorkExperiences;

public enum WorkExperienceSortField
{
    [Display(Name = "Company name")]
    CompanyName,

    [Display(Name = "Position title")]
    PositionTitle,

    [Display(Name = "Start date")]
    StartDate,

    [Display(Name = "End date")]
    EndDate,

    [Display(Name = "Updated date")]
    UpdatedAt,
}
