using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(int id);

    Task<Profile?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<Profile> GetManyByIdsAsync(IEnumerable<int> ids);

    IAsyncEnumerable<Profile> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    );

    IAsyncEnumerable<Profile> GetManyDeletedAsync();

    Task<PagedResult<Profile>> GetManyPagedAsync(
        ItemsQuery<ProfileSortField> query
    );

    Task<PagedResult<Profile>> GetManyDeletedPagedAsync(
        DeletedItemsQuery<DeletedProfileSortField> query
    );

    Task<int> CountAsync();

    Task<int> CountDeletedAsync();

    void Add(Profile entity);

    void Update(Profile entity);

    void Remove(Profile entity);

    void RemoveRange(IEnumerable<Profile> entities);

    void Purge(Profile entity);

    void PurgeRange(IEnumerable<Profile> entities);

    void Restore(Profile entity);

    void RestoreRange(IEnumerable<Profile> entities);
}
