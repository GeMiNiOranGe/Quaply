using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;

namespace Quaply.Data;

internal class UnitOfWork(QuaplyDbContext context) : IUnitOfWork
{
    private readonly QuaplyDbContext _context = context;
    private IProfileRepository? _profiles;
    private IResumeProfileRepository? _resumeProfiles;

    public IProfileRepository Profiles =>
        _profiles ??= new ProfileRepository(_context);

    public IResumeProfileRepository ResumeProfiles =>
        _resumeProfiles ??= new ResumeProfileRepository(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
