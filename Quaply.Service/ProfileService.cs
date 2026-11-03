using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;
using Quaply.Service.Interfaces;

namespace Quaply.Service;

public class ProfileService(IUnitOfWork unitOfWork) : IProfileService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public Task<Profile?> GetProfileByIdAsync(int id)
    {
        return _unitOfWork.Profiles.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        return await _unitOfWork.Profiles.GetManyAsync().ToListAsync();
    }

    public async Task<PagedResult<Profile>> GetDeletedProfilesPagedAsync(
        DeletedProfileQuery query
    )
    {
        return await _unitOfWork.Profiles.GetManyDeletedPagedAsync(query);
    }

    public Task<int> GetDeletedProfileCountAsync()
    {
        return _unitOfWork.Profiles.CountDeletedAsync();
    }

    public async Task CreateProfileAsync(Profile profile)
    {
        _unitOfWork.Profiles.Add(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateProfileAsync(Profile profile)
    {
        _unitOfWork.Profiles.Update(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteProfileAsync(int id)
    {
        Profile? profile = await _unitOfWork.Profiles.GetByIdAsync(id);
        if (profile is null)
        {
            return;
        }

        IEnumerable<ResumeProfile> links = await _unitOfWork
            .ResumeProfiles.GetManyByProfileIdAsync(id)
            .ToListAsync();
        _unitOfWork.ResumeProfiles.RemoveRange(links);

        _unitOfWork.Profiles.Remove(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PurgeProfileAsync(int id)
    {
        Profile? profile =
            await _unitOfWork.Profiles.GetByIdIncludingDeletedAsync(id);
        if (profile is null)
        {
            return;
        }

        if (profile.DeletedAt is null)
        {
            throw new InvalidOperationException(
                "The profile must be soft-deleted before it can be permanently hard-deleted."
            );
        }

        _unitOfWork.Profiles.Purge(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PurgeRangeProfilesAsync(IEnumerable<int> ids)
    {
        if (!ids.Any())
        {
            return;
        }

        List<Profile> profiles = await _unitOfWork
            .Profiles.GetManyByIdsIncludingDeletedAsync(ids)
            .ToListAsync();

        if (profiles.Any(entity => entity.DeletedAt is null))
        {
            throw new InvalidOperationException(
                "All profiles must be soft-deleted before they can be permanently hard-deleted."
            );
        }

        _unitOfWork.Profiles.PurgeRange(profiles);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PurgeDeletedProfilesAsync()
    {
        List<Profile> profiles = await _unitOfWork
            .Profiles.GetManyDeletedAsync()
            .ToListAsync();

        if (profiles.Count == 0)
        {
            return;
        }

        _unitOfWork.Profiles.PurgeRange(profiles);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreProfileAsync(int id)
    {
        Profile? profile =
            await _unitOfWork.Profiles.GetByIdIncludingDeletedAsync(id);
        if (profile is null)
        {
            return;
        }

        if (profile.DeletedAt is null)
        {
            return;
        }

        _unitOfWork.Profiles.Restore(profile);
        await _unitOfWork.SaveChangesAsync();
    }
}
