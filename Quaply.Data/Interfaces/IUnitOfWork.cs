namespace Quaply.Data.Interfaces;

public interface IUnitOfWork
{
    IProfileRepository Profiles { get; }

    IProjectRepository Projects { get; }

    IResumeProfileRepository ResumeProfiles { get; }

    IResumeWorkExperienceRepository ResumeWorkExperiences { get; }

    IWorkExperienceRepository WorkExperiences { get; }

    Task<int> SaveChangesAsync();
}
