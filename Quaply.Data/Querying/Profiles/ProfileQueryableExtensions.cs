using Microsoft.EntityFrameworkCore;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Utilities;

namespace Quaply.Data.Querying.Profiles;

internal static class ProfileQueryableExtensions
{
    public static IQueryable<Profile> ApplySearch(
        this IQueryable<Profile> query,
        string? searchText
    )
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        string term = searchText.Trim().Replace("%", "\\%").Replace("_", "\\_");

        return query.Where(entity =>
            EF.Functions.Like(entity.FullName, $"%{term}%", "\\")
            || EF.Functions.Like(entity.Email, $"%{term}%", "\\")
        );
    }

    public static IQueryable<Profile> ApplyDeletedAtRange(
        this IQueryable<Profile> query,
        RelativeDateRange range
    )
    {
        DateTime? cutoff = ResolveCutoff(range);
        return cutoff is null
            ? query
            : query.Where(entity => entity.DeletedAt >= cutoff);
    }

    public static IOrderedQueryable<Profile> ApplySort(
        this IQueryable<Profile> query,
        DeletedProfileSortOption sort
    )
    {
        IOrderedQueryable<Profile> ordered = sort.Field switch
        {
            DeletedProfileSortField.FullName => sort.Descending
                ? query.OrderByDescending(e => e.FullName)
                : query.OrderBy(e => e.FullName),

            DeletedProfileSortField.Email => sort.Descending
                ? query.OrderByDescending(e => e.Email)
                : query.OrderBy(e => e.Email),

            DeletedProfileSortField.DeletedAt => sort.Descending
                ? query.OrderByDescending(e => e.DeletedAt)
                : query.OrderBy(e => e.DeletedAt),

            _ => throw ThrowHelper.InvalidEnumValue(sort.Field, nameof(sort)),
        };

        // Tiebreaker for stable paging when the primary sort key has duplicates.
        return ordered.ThenBy(e => e.Id);
    }

    private static DateTime? ResolveCutoff(RelativeDateRange range)
    {
        return range switch
        {
            RelativeDateRange.Last7Days => DateTime.UtcNow.AddDays(-7),
            RelativeDateRange.Last30Days => DateTime.UtcNow.AddDays(-30),
            _ => null,
        };
    }
}
