using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.Profiles;

public readonly record struct DeletedProfileQuery(
    string SearchText,
    RelativeDateRange DeletedRange,
    DeletedProfileSortOption Sort,
    PageOption Paging
);
