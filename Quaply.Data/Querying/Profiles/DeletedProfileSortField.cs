using System.ComponentModel;

namespace Quaply.Data.Querying.Profiles;

public enum DeletedProfileSortField
{
    [Description("Full name")]
    FullName,

    [Description("Email")]
    Email,

    [Description("Deleted date")]
    DeletedAt,
}
