namespace Quaply.Data.Interfaces;

public interface IUnitOfWork
{
    IProfileRepository Profiles { get; }

    IResumeProfileRepository ResumeProfiles { get; }

    IWorkExperienceRepository WorkExperiences { get; }

    Task<int> SaveChangesAsync();
}
