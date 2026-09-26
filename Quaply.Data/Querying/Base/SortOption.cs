namespace Quaply.Data.Querying.Base;

public readonly record struct SortOption<TField>(TField Field, bool Descending)
    where TField : struct, Enum;
