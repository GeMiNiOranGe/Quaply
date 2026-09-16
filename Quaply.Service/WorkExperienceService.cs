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
        WorkExperience? workExperience =
            await _unitOfWork.WorkExperiences.GetByIdAsync(id);
        if (workExperience == null)
        {
            return;
        }

        // Remove links to ResumeWorkExperience
        IEnumerable<ResumeWorkExperience> links = await _unitOfWork
            .ResumeWorkExperiences.GetManyByWorkExperienceIdAsync(id)
            .ToListAsync();
        _unitOfWork.ResumeWorkExperiences.RemoveRange(links);

        // and set WorkExperienceId to null for Projects
        IEnumerable<Project> projects = await _unitOfWork
            .Projects.GetManyByWorkExperienceIdAsync(id)
            .ToListAsync();
        foreach (Project project in projects)
        {
            project.WorkExperienceId = null;
            _unitOfWork.Projects.Update(project);
            // TODO: Consider using the Remove (aka Delete) method via ProjectService.
            _unitOfWork.Projects.Remove(project);
        }

        _unitOfWork.WorkExperiences.Remove(workExperience);
        await _unitOfWork.SaveChangesAsync();
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

    public async Task PurgeRangeWorkExperiencesAsync(IEnumerable<int> ids)
    {
        if (!ids.Any())
        {
            return;
        }

        List<WorkExperience> workExperiences = await _unitOfWork
            .WorkExperiences.GetManyByIdsIncludingDeletedAsync(ids)
            .ToListAsync();

        if (workExperiences.Any(entity => entity.DeletedAt is null))
        {
            throw new InvalidOperationException(
                "All work experiences must be soft-deleted before they can be permanently hard-deleted."
            );
        }

        _unitOfWork.WorkExperiences.PurgeRange(workExperiences);
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

    public async Task RestoreRangeWorkExperiencesAsync(IEnumerable<int> ids)
    {
        if (!ids.Any())
        {
            return;
        }

        List<WorkExperience> workExperiences = await _unitOfWork
            .WorkExperiences.GetManyByIdsIncludingDeletedAsync(ids)
            .ToListAsync();

        List<WorkExperience> toRestore =
        [
            .. workExperiences.Where(entity => entity.DeletedAt is not null),
        ];

        if (toRestore.Count == 0)
        {
            return;
        }

        _unitOfWork.WorkExperiences.RestoreRange(toRestore);
        await _unitOfWork.SaveChangesAsync();
    }
}
