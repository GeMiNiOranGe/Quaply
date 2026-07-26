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

    public Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        return _unitOfWork.Profiles.GetManyAsync();
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
}
