using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;

namespace Quaply.Service;

public class ProfileService(IUnitOfWork unitOfWork) : IProfileService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        return _unitOfWork.Profiles.GetManyAsync();
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
