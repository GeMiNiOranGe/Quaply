namespace Quaply.Data.Querying.Base;

/// <summary>
/// Entity-neutral description of "what the user is currently looking at"
/// in the list of active (non-deleted) items, filtered by last update time.
/// The base ViewModel builds it; each derived ViewModel maps it to its own
/// *Query type.
/// </summary>
/// <typeparam name="TSortField">
/// The enum type used to describe the sort field.
/// </typeparam>
// NOTE: Record (class) chosen over struct because of its size and need
// for inheritance. Re-evaluate if fields change.
public record ItemsQuery<TSortField>(
    string? SearchText,
    RelativeDateRange UpdatedRange,
    SortOption<TSortField> Sorting,
    PageOption Paging
)
    where TSortField : struct, Enum;
