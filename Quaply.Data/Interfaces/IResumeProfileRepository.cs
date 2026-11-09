using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IResumeProfileRepository
{
    IAsyncEnumerable<ResumeProfile> GetManyByProfileIdAsync(int profileId);

    IAsyncEnumerable<ResumeProfile> GetManyByProfileIdsAsync(
        IEnumerable<int> ids
    );

    void RemoveRange(IEnumerable<ResumeProfile> entities);
}
