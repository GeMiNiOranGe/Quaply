namespace Quaply.Data.Querying.Base;

/// <summary>
/// Entity-neutral description of "what the user is currently looking at".
/// The base ViewModel builds it; each derived ViewModel maps it to its own
/// Deleted*Query type.
/// </summary>
/// <typeparam name="TSortField">
/// The enum type used to describe the sort field.
/// </typeparam>
// NOTE: Record (class) chosen over struct because of its size and need
// for inheritance. Re-evaluate if fields change.
public record DeletedItemsQuery<TSortField>(
    string? SearchText,
    RelativeDateRange DeletedRange,
    SortOption<TSortField> Sorting,
    PageOption Paging
)
    where TSortField : struct, Enum;
