using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;

namespace Quaply.Service.Interfaces;

public interface IWorkExperienceService
{
    Task<WorkExperience?> GetWorkExperienceByIdAsync(int id);

    Task<PagedResult<WorkExperience>> GetWorkExperiencesPagedAsync(
        WorkExperienceQuery query
    );

    Task<PagedResult<WorkExperience>> GetDeletedWorkExperiencesPagedAsync(
        DeletedWorkExperienceQuery query
    );

    Task<int> GetWorkExperienceCountAsync();

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
