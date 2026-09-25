using Quaply.Data.Models;
using Quaply.Data.Querying;

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
        WorkExperienceSortOption sort
    );

    void Add(WorkExperience entity);

    void Update(WorkExperience entity);

    void Remove(WorkExperience entity);

    void Purge(WorkExperience entity);

    void PurgeRange(IEnumerable<WorkExperience> entities);

    void Restore(WorkExperience entity);

    void RestoreRange(IEnumerable<WorkExperience> entities);
}
