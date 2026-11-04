using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.WorkExperiences;

public readonly record struct WorkExperienceQuery(
    string? SearchText,
    RelativeDateRange UpdatedRange,
    SortOption<WorkExperienceSortField> Sort,
    PageOption Paging
);
