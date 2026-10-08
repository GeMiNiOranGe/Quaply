using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;

namespace Quaply.Service.Interfaces;

public interface IWorkExperienceService
{
    Task<WorkExperience?> GetWorkExperienceByIdAsync(int id);

    Task<IEnumerable<WorkExperience>> GetWorkExperiencesAsync();

    Task<PagedResult<WorkExperience>> GetDeletedWorkExperiencesPagedAsync(
        WorkExperienceDeletedQuery query
    );

    Task<int> GetDeletedWorkExperienceCountAsync();

    Task CreateWorkExperienceAsync(WorkExperience workExperience);

    Task UpdateWorkExperienceAsync(WorkExperience workExperience);

    Task DeleteWorkExperienceAsync(int id);

    Task PurgeWorkExperienceAsync(int id);

    Task PurgeRangeWorkExperiencesAsync(IEnumerable<int> ids);

    Task PurgeDeletedWorkExperiencesAsync();

    Task RestoreWorkExperienceAsync(int id);

    Task RestoreRangeWorkExperiencesAsync(IEnumerable<int> ids);
}
