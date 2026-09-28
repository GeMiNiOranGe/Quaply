using System.ComponentModel.DataAnnotations;

namespace Quaply.Data.Querying;

public enum RelativeDateRange
{
    [Display(Name = "All time")]
    All,

    [Display(Name = "Last 7 days")]
    Last7Days,

    [Display(Name = "Last 30 days")]
    Last30Days,
}
