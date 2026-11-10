using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;

namespace Quaply.Data.Base;

/// <summary>
/// Common implementation for all repositories that support soft delete.
/// Derived classes only need to provide search/filter, sorting,
/// and (optionally) Include.
/// </summary>
internal abstract class Repository<TEntity, TSortField, TDeletedSortField>(
    QuaplyDbContext context
) : IRepository<TEntity, TSortField, TDeletedSortField>
    where TEntity : class, IEntity, ISoftDeletable
    where TSortField : struct, Enum
    where TDeletedSortField : struct, Enum
{
    protected QuaplyDbContext Context { get; } = context;

    private DbSet<TEntity> Set => Context.Set<TEntity>();

    // --- Extension points for derived classes ---

    /// <summary>
    /// Include configuration for paginated lists (default: no includes).
    /// </summary>
    protected virtual IQueryable<TEntity> IncludeForPaging(
        IQueryable<TEntity> source
    )
    {
        return source;
    }

    protected abstract IQueryable<TEntity> Filter(
        IQueryable<TEntity> source,
        ItemsQuery<TSortField> query
    );

    protected abstract IOrderedQueryable<TEntity> Sort(
        IQueryable<TEntity> source,
        ItemsQuery<TSortField> query
    );

    protected abstract IQueryable<TEntity> FilterDeleted(
        IQueryable<TEntity> source,
        DeletedItemsQuery<TDeletedSortField> query
    );

    protected abstract IOrderedQueryable<TEntity> SortDeleted(
        IQueryable<TEntity> source,
        DeletedItemsQuery<TDeletedSortField> query
    );

    // --- Read ---

    public Task<TEntity?> GetByIdAsync(int id)
    {
        return Set.FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public Task<TEntity?> GetByIdIncludingDeletedAsync(int id)
    {
        return Set.IgnoreQueryFilters()
            .FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public IAsyncEnumerable<TEntity> GetManyByIdsAsync(IEnumerable<int> ids)
    {
        return Set.Where(entity => ids.Contains(entity.Id)).AsAsyncEnumerable();
    }

    public IAsyncEnumerable<TEntity> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    )
    {
        return Set.IgnoreQueryFilters()
            .Where(entity => ids.Contains(entity.Id))
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<TEntity> GetManyDeletedAsync()
    {
        return Set.IgnoreQueryFilters()
            .Where(entity => entity.DeletedAt != null)
            .AsAsyncEnumerable();
    }

    public async Task<PagedResult<TEntity>> GetManyPagedAsync(
        ItemsQuery<TSortField> query
    )
    {
        IQueryable<TEntity> source = IncludeForPaging(Set.AsNoTracking());
        IQueryable<TEntity> filtered = Filter(source, query);

        // Sequential on purpose: a DbContext is not thread-safe.
        // Counting before sorting is more efficient.
        int total = await filtered.CountAsync();

        return await Sort(filtered, query)
            .ToPagedResultAsync(total, query.Paging);
    }

    public async Task<PagedResult<TEntity>> GetManyDeletedPagedAsync(
        DeletedItemsQuery<TDeletedSortField> query
    )
    {
        IQueryable<TEntity> source = Set.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(entity => entity.DeletedAt != null);

        IQueryable<TEntity> filtered = FilterDeleted(source, query);

        // Sequential on purpose: a DbContext is not thread-safe.
        // Counting before sorting is more efficient.
        int total = await filtered.CountAsync();

        return await SortDeleted(filtered, query)
            .ToPagedResultAsync(total, query.Paging);
    }

    public Task<int> CountAsync()
    {
        return Set.CountAsync();
    }

    public Task<int> CountDeletedAsync()
    {
        return Set.IgnoreQueryFilters()
            .CountAsync(entity => entity.DeletedAt != null);
    }

    // --- Write ---

    public void Add(TEntity entity)
    {
        Set.Add(entity);
    }

    public void Update(TEntity entity)
    {
        // Must be a tracked entity (loaded via GetByIdAsync). A detached entity
        // has Original == Current, so ApplyUpdatedAt would silently skip
        // UpdatedAt.
        Debug.Assert(
            Context.Entry(entity).State != EntityState.Detached,
            "Update() requires a tracked entity."
        );

        Set.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        Set.Update(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        // Avoid double enumeration
        TEntity[] items = [.. entities];
        DateTime now = DateTime.UtcNow;

        foreach (TEntity entity in items)
        {
            entity.DeletedAt = now;
        }

        Set.UpdateRange(items);
    }

    public void Restore(TEntity entity)
    {
        entity.DeletedAt = null;
        Set.Update(entity);
    }

    public void RestoreRange(IEnumerable<TEntity> entities)
    {
        // Avoid double enumeration
        TEntity[] items = [.. entities];

        foreach (TEntity entity in items)
        {
            entity.DeletedAt = null;
        }

        Set.UpdateRange(items);
    }

    public void Purge(TEntity entity)
    {
        Set.Remove(entity);
    }

    public void PurgeRange(IEnumerable<TEntity> entities)
    {
        Set.RemoveRange(entities);
    }
}
