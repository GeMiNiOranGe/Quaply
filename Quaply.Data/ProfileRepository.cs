using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

namespace Quaply.Data;

public class ProfileRepository(QuaplyDbContext context) : IProfileRepository
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

    public async Task AddAsync(Profile profile)
    {
        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int id)
    {
        Profile? profile = await _context.Profiles.FindAsync(id);
        if (profile is null)
        {
            return;
        }

        _context.Profiles.Remove(profile);
        await _context.SaveChangesAsync();
    }
}
