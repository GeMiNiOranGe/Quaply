using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying;

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

    public IAsyncEnumerable<WorkExperience> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    )
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .Where(entity => ids.Contains(entity.Id))
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<WorkExperience> GetManyAsync()
    {
        return _context
            .WorkExperiences.AsNoTracking()
            .Include(entity => entity.Projects)
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<WorkExperience> GetManyDeletedAsync(
        WorkExperienceSortOption sort
    )
    {
        IQueryable<WorkExperience> query = _context
            .WorkExperiences.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(entity => entity.DeletedAt != null);

        query = sort.Field switch
        {
            WorkExperienceSortField.CompanyName => sort.Descending
                ? query.OrderByDescending(e => e.CompanyName)
                : query.OrderBy(e => e.CompanyName),
            _ => sort.Descending
                ? query.OrderByDescending(e => e.DeletedAt)
                : query.OrderBy(e => e.DeletedAt),
        };

        return query.AsAsyncEnumerable();
    }

    public void Add(WorkExperience entity)
    {
        _context.WorkExperiences.Add(entity);
    }

    public void Update(WorkExperience entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
    }

    public void Remove(WorkExperience entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
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
        entity.UpdatedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
    }

    public void RestoreRange(IEnumerable<WorkExperience> entities)
    {
        foreach (WorkExperience entity in entities)
        {
            entity.DeletedAt = null;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        _context.WorkExperiences.UpdateRange(entities);
    }
}
