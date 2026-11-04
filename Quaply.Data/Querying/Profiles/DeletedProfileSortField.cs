using System.ComponentModel.DataAnnotations;

namespace Quaply.Data.Querying.Profiles;

public enum DeletedProfileSortField
{
    [Display(Name = "Full name")]
    FullName,

    [Display(Name = "Email")]
    Email,

    [Display(Name = "Deleted date")]
    DeletedAt,
}
