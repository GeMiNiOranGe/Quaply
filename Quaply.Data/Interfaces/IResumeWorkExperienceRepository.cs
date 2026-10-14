using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IResumeWorkExperienceRepository
{
    IAsyncEnumerable<ResumeWorkExperience> GetManyByWorkExperienceIdAsync(
        int workExperienceId
    );

    IAsyncEnumerable<ResumeWorkExperience> GetManyByWorkExperienceIdsAsync(
        IEnumerable<int> ids
    );

    void RemoveRange(IEnumerable<ResumeWorkExperience> entities);
}
