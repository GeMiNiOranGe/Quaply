using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

namespace Quaply.Data;

internal class ProjectRepository(QuaplyDbContext context) : IProjectRepository
{
    private readonly QuaplyDbContext _context = context;

    public IAsyncEnumerable<Project> GetManyByWorkExperienceIdAsync(
        int workExperienceId
    )
    {
        return _context
            .Projects.Where(entity =>
                entity.WorkExperienceId == workExperienceId
            )
            .AsAsyncEnumerable();
    }

    public void Update(Project entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(entity);
    }

    public void Remove(Project entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        _context.Projects.Update(entity);
    }
}
