using Quaply.Data.Querying.Base;

namespace Quaply.Data.Querying.Profiles;

public readonly record struct DeletedProfileSortOption(
    DeletedProfileSortField Field,
    bool Descending
)
{
    public static implicit operator SortOption<DeletedProfileSortField>(
        DeletedProfileSortOption option
    )
    {
        return new(option.Field, option.Descending);
    }

    public static implicit operator DeletedProfileSortOption(
        SortOption<DeletedProfileSortField> option
    )
    {
        return new(option.Field, option.Descending);
    }
}
