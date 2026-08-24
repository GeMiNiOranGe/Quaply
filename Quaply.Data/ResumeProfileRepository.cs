using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

namespace Quaply.Data;

internal class ResumeProfileRepository(QuaplyDbContext context)
    : IResumeProfileRepository
{
    private readonly QuaplyDbContext _context = context;

    public IEnumerable<ResumeProfile> GetByProfileId(int profileId)
    {
        return _context.ResumeProfiles.Where(rp => rp.ProfileId == profileId);
    }

    public void RemoveRange(IEnumerable<ResumeProfile> links)
    {
        _context.ResumeProfiles.RemoveRange(links);
    }
}
