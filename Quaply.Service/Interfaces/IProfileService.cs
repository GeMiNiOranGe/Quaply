using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;

namespace Quaply.Service.Interfaces;

public interface IProfileService
{
    Task<Profile?> GetProfileByIdAsync(int id);

    Task<PagedResult<Profile>> GetProfilesPagedAsync(
        ItemsQuery<ProfileSortField> query
    );

    Task<PagedResult<Profile>> GetDeletedProfilesPagedAsync(
        DeletedItemsQuery<DeletedProfileSortField> query
    );

    Task<int> GetProfileCountAsync();

    Task<int> GetDeletedProfileCountAsync();

    Task CreateProfileAsync(Profile profile);

    Task UpdateProfileAsync(Profile profile);

    Task DeleteProfileAsync(int id);

    Task PurgeProfileAsync(int id);

    Task PurgeRangeProfilesAsync(IEnumerable<int> ids);

    Task PurgeDeletedProfilesAsync();

    Task RestoreProfileAsync(int id);

    Task RestoreRangeProfilesAsync(IEnumerable<int> ids);
}
