using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;

namespace Quaply.Data;

internal class ProfileRepository(QuaplyDbContext context) : IProfileRepository
{
    private readonly QuaplyDbContext _context = context;

    public Task<Profile?> GetByIdAsync(int id)
    {
        return _context.Profiles.FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public Task<Profile?> GetByIdIncludingDeletedAsync(int id)
    {
        return _context
            .Profiles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public IAsyncEnumerable<Profile> GetManyByIdsAsync(IEnumerable<int> ids)
    {
        return _context
            .Profiles.Where(entity => ids.Contains(entity.Id))
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<Profile> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    )
    {
        return _context
            .Profiles.IgnoreQueryFilters()
            .Where(entity => ids.Contains(entity.Id))
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<Profile> GetManyDeletedAsync()
    {
        return _context
            .Profiles.IgnoreQueryFilters()
            .Where(entity => entity.DeletedAt != null)
            .AsAsyncEnumerable();
    }

    public async Task<PagedResult<Profile>> GetManyPagedAsync(
        ItemsQuery<ProfileSortField> query
    )
    {
        IQueryable<Profile> filtered = _context
            .Profiles.AsNoTracking()
            .Include(entity => entity.ResumeProfiles)
            .ApplySearch(query.SearchText)
            .ApplyUpdatedAtRange(query.UpdatedRange);

        // Sequential on purpose: a DbContext is not thread-safe.
        // Counting before sorting is more efficient.
        int total = await filtered.CountAsync();

        return await filtered
            .ApplySort(query.Sorting)
            .ToPagedResultAsync(total, query.Paging);
    }

    public async Task<PagedResult<Profile>> GetManyDeletedPagedAsync(
        DeletedItemsQuery<DeletedProfileSortField> query
    )
    {
        IQueryable<Profile> filtered = _context
            .Profiles.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(entity => entity.DeletedAt != null)
            .ApplySearch(query.SearchText)
            .ApplyDeletedAtRange(query.DeletedRange);

        // Sequential on purpose: a DbContext is not thread-safe.
        // Counting before sorting is more efficient.
        int total = await filtered.CountAsync();

        return await filtered
            .ApplySort(query.Sorting)
            .ToPagedResultAsync(total, query.Paging);
    }

    public Task<int> CountAsync()
    {
        return _context.Profiles.CountAsync();
    }

    public Task<int> CountDeletedAsync()
    {
        return _context
            .Profiles.IgnoreQueryFilters()
            .CountAsync(entity => entity.DeletedAt != null);
    }

    public void Add(Profile entity)
    {
        _context.Profiles.Add(entity);
    }

    public void Update(Profile entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Profiles.Update(entity);
    }

    public void Remove(Profile entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        _context.Profiles.Update(entity);
    }

    public void RemoveRange(IEnumerable<Profile> entities)
    {
        foreach (Profile entity in entities)
        {
            entity.DeletedAt = DateTime.UtcNow;
        }

        _context.Profiles.UpdateRange(entities);
    }

    public void Purge(Profile entity)
    {
        _context.Profiles.Remove(entity);
    }

    public void PurgeRange(IEnumerable<Profile> entities)
    {
        _context.Profiles.RemoveRange(entities);
    }

    public void Restore(Profile entity)
    {
        entity.DeletedAt = null;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Profiles.Update(entity);
    }

    public void RestoreRange(IEnumerable<Profile> entities)
    {
        foreach (Profile entity in entities)
        {
            entity.DeletedAt = null;
        }

        _context.Profiles.UpdateRange(entities);
    }
}
