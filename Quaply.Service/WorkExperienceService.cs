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
        return _unitOfWork.WorkExperiences.GetByIdAsync(id);
    }

    public async Task<IEnumerable<WorkExperience>> GetWorkExperiencesAsync()
    {
        return await _unitOfWork.WorkExperiences.GetManyAsync().ToListAsync();
    }

    public async Task<
        IEnumerable<WorkExperience>
    > GetDeletedWorkExperiencesAsync()
    {
        return await _unitOfWork
            .WorkExperiences.GetManyDeletedAsync()
            .ToListAsync();
    }

    public async Task CreateWorkExperienceAsync(WorkExperience workExperience)
    {
        _unitOfWork.WorkExperiences.Add(workExperience);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateWorkExperienceAsync(WorkExperience workExperience)
    {
        _unitOfWork.WorkExperiences.Update(workExperience);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteWorkExperienceAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task PurgeWorkExperienceAsync(int id)
    {
        WorkExperience? workExperience =
            await _unitOfWork.WorkExperiences.GetByIdIncludingDeletedAsync(id);
        if (workExperience is null)
        {
            return;
        }

        if (workExperience.DeletedAt is null)
        {
            throw new InvalidOperationException(
                "The work experience must be soft-deleted before it can be permanently hard-deleted."
            );
        }

        _unitOfWork.WorkExperiences.Purge(workExperience);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreWorkExperienceAsync(int id)
    {
        WorkExperience? workExperience =
            await _unitOfWork.WorkExperiences.GetByIdIncludingDeletedAsync(id);
        if (workExperience is null)
        {
            return;
        }

        if (workExperience.DeletedAt is null)
        {
            return;
        }

        _unitOfWork.WorkExperiences.Restore(workExperience);
        await _unitOfWork.SaveChangesAsync();
    }
}
