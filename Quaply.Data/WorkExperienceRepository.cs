using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

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

    public IAsyncEnumerable<WorkExperience> GetManyAsync()
    {
        return _context.WorkExperiences.AsNoTracking().AsAsyncEnumerable();
    }

    public IAsyncEnumerable<WorkExperience> GetManyDeletedAsync()
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(entity => entity.DeletedAt != null)
            .AsAsyncEnumerable();
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

    public void Restore(WorkExperience entity)
    {
        entity.DeletedAt = null;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
    }
}
