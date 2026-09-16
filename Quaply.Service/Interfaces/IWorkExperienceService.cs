using Quaply.Data.Models;

namespace Quaply.Service.Interfaces;

public interface IWorkExperienceService
{
    Task<WorkExperience?> GetWorkExperienceByIdAsync(int id);

    Task<IEnumerable<WorkExperience>> GetWorkExperiencesAsync();

    Task<IEnumerable<WorkExperience>> GetDeletedWorkExperiencesAsync();

    Task CreateWorkExperienceAsync(WorkExperience workExperience);

    Task UpdateWorkExperienceAsync(WorkExperience workExperience);

    Task DeleteWorkExperienceAsync(int id);

    Task PurgeWorkExperienceAsync(int id);

    Task PurgeRangeWorkExperiencesAsync(IEnumerable<int> ids);

    Task RestoreWorkExperienceAsync(int id);

    Task RestoreRangeWorkExperiencesAsync(IEnumerable<int> ids);
}
