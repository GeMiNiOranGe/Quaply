using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

namespace Quaply.Data;

internal class ResumeProfileRepository(QuaplyDbContext context)
    : IResumeProfileRepository
{
    private readonly QuaplyDbContext _context = context;

    public IAsyncEnumerable<ResumeProfile> GetManyByProfileIdAsync(
        int profileId
    )
    {
        return _context
            .ResumeProfiles.Where(rp => rp.ProfileId == profileId)
            .AsAsyncEnumerable();
    }

    public void RemoveRange(IEnumerable<ResumeProfile> entities)
    {
        _context.ResumeProfiles.RemoveRange(entities);
    }
}
