using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IResumeProfileRepository
{
    IAsyncEnumerable<ResumeProfile> GetManyByProfileIdAsync(int profileId);

    void RemoveRange(IEnumerable<ResumeProfile> entities);
}
