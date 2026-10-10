using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.WorkExperiences;

public readonly record struct DeletedWorkExperienceSortOption(
    DeletedWorkExperienceSortField Field,
    bool Descending
)
{
    public static implicit operator SortOption<DeletedWorkExperienceSortField>(
        DeletedWorkExperienceSortOption option
    )
    {
        return new(option.Field, option.Descending);
    }

    public static implicit operator DeletedWorkExperienceSortOption(
        SortOption<DeletedWorkExperienceSortField> option
    )
    {
        return new(option.Field, option.Descending);
    }
}
