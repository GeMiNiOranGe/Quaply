using Microsoft.EntityFrameworkCore;
using Quaply.Data.Models;

namespace Quaply.Data.Querying.Extensions;

internal static class WorkExperienceQueryableExtensions
{
    public static IQueryable<WorkExperience> ApplySearch(
        this IQueryable<WorkExperience> query,
        string? searchText
    )
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        string term = searchText.Trim().Replace("%", "\\%").Replace("_", "\\_");

        return query.Where(entity =>
            EF.Functions.Like(entity.CompanyName, $"%{term}%", "\\")
            || EF.Functions.Like(entity.PositionTitle, $"%{term}%", "\\")
        );
    }

    public static IQueryable<WorkExperience> ApplyDeletedRange(
        this IQueryable<WorkExperience> query,
        RelativeDateRange range
    )
    {
        DateTime? cutoff = range switch
        {
            RelativeDateRange.Last7Days => DateTime.UtcNow.AddDays(-7),
            RelativeDateRange.Last30Days => DateTime.UtcNow.AddDays(-30),
            _ => null, // All time - unfiltered
        };

        return cutoff is null
            ? query
            : query.Where(entity => entity.DeletedAt >= cutoff);
    }

    public static IQueryable<WorkExperience> ApplySort(
        this IQueryable<WorkExperience> query,
        WorkExperienceSortOption sort
    )
    {
        return sort.Field switch
        {
            WorkExperienceSortField.CompanyName => sort.Descending
                ? query.OrderByDescending(e => e.CompanyName)
                : query.OrderBy(e => e.CompanyName),
            WorkExperienceSortField.PositionTitle => sort.Descending
                ? query.OrderByDescending(e => e.PositionTitle)
                : query.OrderBy(e => e.PositionTitle),
            _ => sort.Descending
                ? query.OrderByDescending(e => e.DeletedAt)
                : query.OrderBy(e => e.DeletedAt),
        };
    }
}
