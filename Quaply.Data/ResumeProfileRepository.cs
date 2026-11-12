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
            .ResumeProfiles.Where(entity => entity.ProfileId == profileId)
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<ResumeProfile> GetManyByProfileIdsAsync(
        IEnumerable<int> profileIds
    )
    {
        return _context
            .ResumeProfiles.Where(entity =>
                profileIds.Contains(entity.ProfileId)
            )
            .AsAsyncEnumerable();
    }

    public void PurgeRange(IEnumerable<ResumeProfile> entities)
    {
        _context.ResumeProfiles.RemoveRange(entities);
    }
}
