using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;

namespace Quaply.Service.Interfaces;

public interface IProfileService
{
    Task<Profile?> GetProfileByIdAsync(int id);

    Task<IEnumerable<Profile>> GetProfilesAsync();

    Task<IEnumerable<Profile>> GetDeletedProfilesAsync();

    Task<PagedResult<Profile>> GetDeletedProfilesPagedAsync(
        DeletedProfileQuery query
    );

    Task<int> GetDeletedProfileCountAsync();

    Task CreateProfileAsync(Profile profile);

    Task UpdateProfileAsync(Profile profile);

    Task DeleteProfileAsync(int id);

    Task PurgeProfileAsync(int id);

    Task RestoreProfileAsync(int id);
}
