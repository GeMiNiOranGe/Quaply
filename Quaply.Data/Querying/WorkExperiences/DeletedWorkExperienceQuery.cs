using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.WorkExperiences;

public readonly record struct DeletedWorkExperienceQuery(
    string? SearchText,
    RelativeDateRange DeletedRange,
    SortOption<DeletedWorkExperienceSortField> Sort,
    PageOption Paging
);
