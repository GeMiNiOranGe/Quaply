using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.WorkExperiences;

public readonly record struct WorkExperienceSortOption(
    WorkExperienceSortField Field,
    bool Descending
)
{
    public static implicit operator SortOption<WorkExperienceSortField>(
        WorkExperienceSortOption option
    )
    {
        return new(option.Field, option.Descending);
    }

    public static implicit operator WorkExperienceSortOption(
        SortOption<WorkExperienceSortField> option
    )
    {
        return new(option.Field, option.Descending);
    }
}
