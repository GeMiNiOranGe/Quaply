using Quaply.Data.Models;

namespace Quaply.Service.Interfaces;

public interface IProfileService
{
    Task<Profile?> GetProfileByIdAsync(int id);
    Task<IEnumerable<Profile>> GetProfilesAsync();
    Task<IEnumerable<Profile>> GetDeletedProfilesAsync();
    Task CreateProfileAsync(Profile profile);
    Task UpdateProfileAsync(Profile profile);
    Task DeleteProfileAsync(int id);
    Task PurgeProfileAsync(int id);
}
