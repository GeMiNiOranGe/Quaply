using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IWorkExperienceRepository
{
    Task<WorkExperience?> GetByIdAsync(int id);

    Task<WorkExperience?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<WorkExperience> GetManyAsync();

    IAsyncEnumerable<WorkExperience> GetManyDeletedAsync();

    void Add(WorkExperience workExperience);

    void Update(WorkExperience workExperience);

    void Remove(WorkExperience workExperience);

    void Purge(WorkExperience workExperience);

    void Restore(WorkExperience workExperience);
}
