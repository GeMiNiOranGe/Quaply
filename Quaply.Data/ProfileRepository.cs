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
        return _context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<Profile?> GetByIdIncludingDeletedAsync(int id)
    {
        return _context
            .Profiles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);
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
            .Where(p => p.DeletedAt != null)
            .AsAsyncEnumerable();
    }

    public void Add(Profile profile)
    {
        _context.Profiles.Add(profile);
    }

    public void Update(Profile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        _context.Profiles.Update(profile);
    }

    public void Remove(Profile profile)
    {
        profile.DeletedAt = DateTime.UtcNow;
        _context.Profiles.Update(profile);
    }

    public void Purge(Profile profile)
    {
        _context.Profiles.Remove(profile);
    }

    public void Restore(Profile profile)
    {
        profile.DeletedAt = null;
        profile.UpdatedAt = DateTime.UtcNow;
        _context.Profiles.Update(profile);
    }
}
