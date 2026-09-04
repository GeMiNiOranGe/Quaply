using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IWorkExperienceRepository
{
    Task<WorkExperience?> GetByIdAsync(int id);

    Task<WorkExperience?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<WorkExperience> GetManyAsync();

    IAsyncEnumerable<WorkExperience> GetManyDeletedAsync();

    void Add(WorkExperience entity);

    void Update(WorkExperience entity);

    void Remove(WorkExperience entity);

    void Purge(WorkExperience entity);

    void Restore(WorkExperience entity);
}
