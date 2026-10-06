using Microsoft.EntityFrameworkCore;

namespace Quaply.Data.Querying.Base;

public static class PagingQueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> sortedQuery,
        int totalCount,
        PageOption paging
    )
    {
        int pageSize = Math.Max(1, paging.PageSize);
        int lastPage = Math.Max(
            1,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
        int page = Math.Clamp(paging.Page, 1, lastPage);

        // ToArrayAsync avoids the unused capacity List<T> allocates internally.
        IReadOnlyList<T> items = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }
}
