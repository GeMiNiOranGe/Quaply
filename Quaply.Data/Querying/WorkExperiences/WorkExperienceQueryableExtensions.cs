using Microsoft.EntityFrameworkCore;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Utilities;

namespace Quaply.Data.Querying.WorkExperiences;

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

    public static IQueryable<WorkExperience> ApplyUpdatedAtRange(
        this IQueryable<WorkExperience> query,
        RelativeDateRange range
    )
    {
        DateTime? cutoff = ResolveCutoff(range);
        return cutoff is null
            ? query
            : query.Where(entity => entity.UpdatedAt >= cutoff);
    }

    public static IQueryable<WorkExperience> ApplyDeletedAtRange(
        this IQueryable<WorkExperience> query,
        RelativeDateRange range
    )
    {
        DateTime? cutoff = ResolveCutoff(range);
        return cutoff is null
            ? query
            : query.Where(entity => entity.DeletedAt >= cutoff);
    }

    public static IOrderedQueryable<WorkExperience> ApplySort(
        this IQueryable<WorkExperience> query,
        WorkExperienceSortOption sort
    )
    {
        IOrderedQueryable<WorkExperience> ordered = sort.Field switch
        {
            WorkExperienceSortField.CompanyName => sort.Descending
                ? query.OrderByDescending(e => e.CompanyName)
                : query.OrderBy(e => e.CompanyName),

            WorkExperienceSortField.PositionTitle => sort.Descending
                ? query.OrderByDescending(e => e.PositionTitle)
                : query.OrderBy(e => e.PositionTitle),

            WorkExperienceSortField.StartDate => sort.Descending
                ? query.OrderByDescending(e => e.StartDate)
                : query.OrderBy(e => e.StartDate),

            WorkExperienceSortField.EndDate => sort.Descending
                ? query.OrderByDescending(e => e.EndDate ?? DateOnly.MaxValue)
                : query.OrderBy(e => e.EndDate ?? DateOnly.MaxValue),

            WorkExperienceSortField.UpdatedAt => sort.Descending
                ? query.OrderByDescending(e => e.UpdatedAt)
                : query.OrderBy(e => e.UpdatedAt),

            _ => throw ThrowHelper.InvalidEnumValue(sort.Field, nameof(sort)),
        };

        // Tiebreaker for stable paging when the primary sort key has duplicates.
        return ordered.ThenBy(e => e.Id);
    }

    public static IOrderedQueryable<WorkExperience> ApplySort(
        this IQueryable<WorkExperience> query,
        DeletedWorkExperienceSortOption sort
    )
    {
        IOrderedQueryable<WorkExperience> ordered = sort.Field switch
        {
            DeletedWorkExperienceSortField.CompanyName => sort.Descending
                ? query.OrderByDescending(e => e.CompanyName)
                : query.OrderBy(e => e.CompanyName),

            DeletedWorkExperienceSortField.PositionTitle => sort.Descending
                ? query.OrderByDescending(e => e.PositionTitle)
                : query.OrderBy(e => e.PositionTitle),

            DeletedWorkExperienceSortField.DeletedAt => sort.Descending
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
