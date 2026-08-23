using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

namespace Quaply.Data;

internal class ProfileRepository(QuaplyDbContext context) : IProfileRepository
{
    private readonly QuaplyDbContext _context = context;

    public async Task<Profile?> GetByIdAsync(int id)
    {
        return await _context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Profile>> GetManyAsync()
    {
        return await _context.Profiles.AsNoTracking().ToListAsync();
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
}
