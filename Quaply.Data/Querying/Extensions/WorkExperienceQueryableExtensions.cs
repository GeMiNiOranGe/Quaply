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
            _ => sort.Descending
                ? query.OrderByDescending(e => e.DeletedAt)
                : query.OrderBy(e => e.DeletedAt),
        };
    }
}
