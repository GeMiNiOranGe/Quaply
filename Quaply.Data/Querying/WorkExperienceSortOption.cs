namespace Quaply.Data.Querying;

public readonly record struct WorkExperienceSortOption(
    WorkExperienceSortField Field,
    bool Descending
);
