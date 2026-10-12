using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.WorkExperiences;

public readonly record struct WorkExperienceQuery(
    string? SearchText,
    RelativeDateRange UpdatedRange,
    WorkExperienceSortOption Sort,
    PageOption Paging
);
