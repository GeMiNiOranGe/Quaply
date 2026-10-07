using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.WorkExperiences;

public readonly record struct WorkExperienceDeletedQuery(
    string? SearchText,
    RelativeDateRange DeletedRange,
    WorkExperienceSortOption Sort,
    PageOption Paging
);
