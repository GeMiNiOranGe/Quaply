using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

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

    public IAsyncEnumerable<Profile> GetManyAsync()
    {
        return _context.Profiles.AsNoTracking().AsAsyncEnumerable();
    }

    public IAsyncEnumerable<Profile> GetManyDeletedAsync()
    {
        return _context
            .Profiles.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(entity => entity.DeletedAt != null)
            .AsAsyncEnumerable();
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

    public void Purge(Profile entity)
    {
        _context.Profiles.Remove(entity);
    }

    public void Restore(Profile entity)
    {
        entity.DeletedAt = null;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Profiles.Update(entity);
    }
}
