using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;

namespace Quaply.Data;

internal class UnitOfWork(QuaplyDbContext context) : IUnitOfWork
{
    private readonly QuaplyDbContext _context = context;
    private IProfileRepository? _profiles;
    private IProjectRepository? _projects;
    private IResumeProfileRepository? _resumeProfiles;
    private IResumeWorkExperienceRepository? _resumeWorkExperiences;
    private IWorkExperienceRepository? _workExperiences;

    public IProfileRepository Profiles =>
        _profiles ??= new ProfileRepository(_context);

    public IProjectRepository Projects =>
        _projects ??= new ProjectRepository(_context);

    public IResumeProfileRepository ResumeProfiles =>
        _resumeProfiles ??= new ResumeProfileRepository(_context);

    public IResumeWorkExperienceRepository ResumeWorkExperiences =>
        _resumeWorkExperiences ??= new ResumeWorkExperienceRepository(_context);

    public IWorkExperienceRepository WorkExperiences =>
        _workExperiences ??= new WorkExperienceRepository(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
