namespace Quaply.Data.Querying;

public readonly record struct WorkExperienceDeletedQuery(
    string? SearchText,
    WorkExperienceSortOption Sort
);
