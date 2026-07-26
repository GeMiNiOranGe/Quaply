using Quaply.Data.Models;

namespace Quaply.Service.Interfaces;

public interface IProfileService
{
    Task<Profile?> GetProfileByIdAsync(int id);
    Task<IEnumerable<Profile>> GetProfilesAsync();
    Task CreateProfileAsync(Profile profile);
    Task UpdateProfileAsync(Profile profile);
    Task DeleteProfileAsync(int id);
}
