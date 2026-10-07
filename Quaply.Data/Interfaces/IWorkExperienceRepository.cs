using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;

namespace Quaply.Data.Interfaces;

public interface IWorkExperienceRepository
{
    Task<WorkExperience?> GetByIdAsync(int id);

    Task<WorkExperience?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<WorkExperience> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    );

    IAsyncEnumerable<WorkExperience> GetManyAsync();

    IAsyncEnumerable<WorkExperience> GetManyDeletedAsync(
        WorkExperienceDeletedQuery query
    );

    Task<PagedResult<WorkExperience>> GetManyDeletedPagedAsync(
        WorkExperienceDeletedQuery query
    );

    Task<int> CountDeletedAsync();

    void Add(WorkExperience entity);

    void Update(WorkExperience entity);

    void Remove(WorkExperience entity);

    void Purge(WorkExperience entity);

    void PurgeRange(IEnumerable<WorkExperience> entities);

    void Restore(WorkExperience entity);

    void RestoreRange(IEnumerable<WorkExperience> entities);
}
