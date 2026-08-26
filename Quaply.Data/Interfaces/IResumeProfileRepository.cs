using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IResumeProfileRepository
{
    IEnumerable<ResumeProfile> GetByProfileId(int profileId);

    void RemoveRange(IEnumerable<ResumeProfile> links);
}
