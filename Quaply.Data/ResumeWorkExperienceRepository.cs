using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;

namespace Quaply.Data;

internal class ResumeWorkExperienceRepository(QuaplyDbContext context)
    : IResumeWorkExperienceRepository
{
    private readonly QuaplyDbContext _context = context;

    public IAsyncEnumerable<ResumeWorkExperience> GetManyByWorkExperienceIdAsync(
        int workExperienceId
    )
    {
        return _context
            .ResumeWorkExperiences.Where(entity =>
                entity.WorkExperienceId == workExperienceId
            )
            .AsAsyncEnumerable();
    }

    public void RemoveRange(IEnumerable<ResumeWorkExperience> entities)
    {
        _context.ResumeWorkExperiences.RemoveRange(entities);
    }
}
