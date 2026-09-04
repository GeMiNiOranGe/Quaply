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
        return _context.WorkExperiences.FirstOrDefaultAsync(we => we.Id == id);
    }

    public Task<WorkExperience?> GetByIdIncludingDeletedAsync(int id)
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .FirstOrDefaultAsync(we => we.Id == id);
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
            .Where(we => we.DeletedAt != null)
            .AsAsyncEnumerable();
    }

    public void Add(WorkExperience workExperience)
    {
        _context.WorkExperiences.Add(workExperience);
    }

    public void Update(WorkExperience workExperience)
    {
        workExperience.UpdatedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(workExperience);
    }

    public void Remove(WorkExperience workExperience)
    {
        workExperience.DeletedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(workExperience);
    }

    public void Purge(WorkExperience workExperience)
    {
        _context.WorkExperiences.Remove(workExperience);
    }

    public void Restore(WorkExperience workExperience)
    {
        workExperience.DeletedAt = null;
        workExperience.UpdatedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(workExperience);
    }
}
