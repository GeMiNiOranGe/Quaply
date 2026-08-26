namespace Quaply.Data.Interfaces;

public interface IUnitOfWork
{
    IProfileRepository Profiles { get; }

    IResumeProfileRepository ResumeProfiles { get; }

    Task<int> SaveChangesAsync();
}
