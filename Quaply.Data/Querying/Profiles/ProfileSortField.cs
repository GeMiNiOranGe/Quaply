using System.ComponentModel.DataAnnotations;

namespace Quaply.Data.Querying.Profiles;

/// <summary>
/// Sortable fields of the (active) Profile list.
/// Sibling of <c>DeletedProfileSortField</c>, which sorts the trash.
/// </summary>
public enum ProfileSortField
{
    [Display(Name = "Full name")]
    FullName,

    [Display(Name = "Email")]
    Email,

    [Display(Name = "Last updated")]
    UpdatedAt,
}
