using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;

namespace Quaply.Service;

public class WorkExperienceService(IUnitOfWork unitOfWork)
    : IWorkExperienceService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public Task<WorkExperience?> GetWorkExperienceByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<WorkExperience>> GetWorkExperiencesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<
        IEnumerable<WorkExperience>
    > GetDeletedWorkExperiencesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task CreateWorkExperienceAsync(WorkExperience workExperience)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateWorkExperienceAsync(WorkExperience workExperience)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteWorkExperienceAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task PurgeWorkExperienceAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task RestoreWorkExperienceAsync(int id)
    {
        throw new NotImplementedException();
    }
}
