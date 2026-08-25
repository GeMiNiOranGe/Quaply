using Quaply.Data.Interfaces;
using Quaply.Data.Models;
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

    public async Task<IEnumerable<Profile>> GetDeletedProfilesAsync()
    {
        return await _unitOfWork.Profiles.GetManyDeletedAsync().ToListAsync();
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

        IEnumerable<ResumeProfile> links =
            _unitOfWork.ResumeProfiles.GetByProfileId(id);
        _unitOfWork.ResumeProfiles.RemoveRange(links);

        _unitOfWork.Profiles.Purge(profile);

        await _unitOfWork.SaveChangesAsync();
    }
}
