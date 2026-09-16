using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IProjectRepository
{
    IAsyncEnumerable<Project> GetManyByWorkExperienceIdAsync(
        int workExperienceId
    );

    void Update(Project entity);

    void Remove(Project entity);
}
