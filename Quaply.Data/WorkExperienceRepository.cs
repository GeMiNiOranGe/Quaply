using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;

namespace Quaply.Data;

internal class WorkExperienceRepository(QuaplyDbContext context)
    : IWorkExperienceRepository
{
    private readonly QuaplyDbContext _context = context;

    public Task<WorkExperience?> GetByIdAsync(int id)
    {
        return _context.WorkExperiences.FirstOrDefaultAsync(entity =>
            entity.Id == id
        );
    }

    public Task<WorkExperience?> GetByIdIncludingDeletedAsync(int id)
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public IAsyncEnumerable<WorkExperience> GetManyByIdsAsync(
        IEnumerable<int> ids
    )
    {
        return _context
            .WorkExperiences.Where(entity => ids.Contains(entity.Id))
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<WorkExperience> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    )
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .Where(entity => ids.Contains(entity.Id))
            .AsAsyncEnumerable();
    }

    /*
    public IAsyncEnumerable<WorkExperience> GetManyAsync()
    {
        return _context.WorkExperiences.AsAsyncEnumerable();
    }
    */

    public IAsyncEnumerable<WorkExperience> GetManyDeletedAsync()
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .Where(entity => entity.DeletedAt != null)
            .AsAsyncEnumerable();
    }

    public async Task<PagedResult<WorkExperience>> GetManyPagedAsync(
        WorkExperienceQuery query
    )
    {
        IQueryable<WorkExperience> filtered = _context
            .WorkExperiences.AsNoTracking()
            .Include(entity => entity.Projects)
            .ApplySearch(query.SearchText)
            .ApplyUpdatedAtRange(query.UpdatedRange);

        // Sequential on purpose: a DbContext is not thread-safe.
        // Counting before sorting is more efficient.
        int total = await filtered.CountAsync();

        return await filtered
            .ApplySort(query.Sort)
            .ToPagedResultAsync(total, query.Paging);
    }

    public async Task<PagedResult<WorkExperience>> GetManyDeletedPagedAsync(
        DeletedWorkExperienceQuery query
    )
    {
        IQueryable<WorkExperience> filtered = _context
            .WorkExperiences.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(entity => entity.DeletedAt != null)
            .ApplySearch(query.SearchText)
            .ApplyDeletedAtRange(query.DeletedRange);

        // Sequential on purpose: a DbContext is not thread-safe.
        // Counting before sorting is more efficient.
        int total = await filtered.CountAsync();

        return await filtered
            .ApplySort(query.Sort)
            .ToPagedResultAsync(total, query.Paging);
    }

    public Task<int> CountAsync()
    {
        return _context.WorkExperiences.CountAsync();
    }

    public Task<int> CountDeletedAsync()
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .CountAsync(entity => entity.DeletedAt != null);
    }

    public void Add(WorkExperience entity)
    {
        _context.WorkExperiences.Add(entity);
    }

    public void Update(WorkExperience entity)
    {
        // Must be a tracked entity (loaded via GetByIdAsync). A detached entity
        // has Original == Current, so ApplyUpdatedAt would silently skip
        // UpdatedAt.
        Debug.Assert(
            _context.Entry(entity).State != EntityState.Detached,
            "Update() requires a tracked entity."
        );

        _context.WorkExperiences.Update(entity);
    }

    public void Remove(WorkExperience entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
    }

    public void RemoveRange(IEnumerable<WorkExperience> entities)
    {
        foreach (WorkExperience entity in entities)
        {
            entity.DeletedAt = DateTime.UtcNow;
        }

        _context.WorkExperiences.UpdateRange(entities);
    }

    public void Purge(WorkExperience entity)
    {
        _context.WorkExperiences.Remove(entity);
    }

    public void PurgeRange(IEnumerable<WorkExperience> entities)
    {
        _context.WorkExperiences.RemoveRange(entities);
    }

    public void Restore(WorkExperience entity)
    {
        entity.DeletedAt = null;
        _context.WorkExperiences.Update(entity);
    }

    public void RestoreRange(IEnumerable<WorkExperience> entities)
    {
        foreach (WorkExperience entity in entities)
        {
            entity.DeletedAt = null;
        }

        _context.WorkExperiences.UpdateRange(entities);
    }
}
