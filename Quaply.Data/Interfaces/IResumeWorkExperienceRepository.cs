using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IResumeWorkExperienceRepository
{
    IAsyncEnumerable<ResumeWorkExperience> GetManyByWorkExperienceIdAsync(
        int workExperienceId
    );

    void RemoveRange(IEnumerable<ResumeWorkExperience> entities);
}
