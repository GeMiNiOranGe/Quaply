using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IProjectRepository
{
    IAsyncEnumerable<Project> GetManyByWorkExperienceIdAsync(
        int workExperienceId
    );

    IAsyncEnumerable<Project> GetManyByWorkExperienceIdsAsync(
        IEnumerable<int> ids
    );

    void Update(Project entity);

    void Remove(Project entity);
}
