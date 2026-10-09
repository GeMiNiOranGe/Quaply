using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.WorkExperiences;

public readonly record struct WorkExperienceSortOption(
    DeletedWorkExperienceSortField Field,
    bool Descending
)
{
    public static implicit operator SortOption<DeletedWorkExperienceSortField>(
        WorkExperienceSortOption option
    )
    {
        return new(option.Field, option.Descending);
    }

    public static implicit operator WorkExperienceSortOption(
        SortOption<DeletedWorkExperienceSortField> option
    )
    {
        return new(option.Field, option.Descending);
    }
}
