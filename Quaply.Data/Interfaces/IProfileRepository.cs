using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(int id);

    Task<Profile?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<Profile> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    );

    IAsyncEnumerable<Profile> GetManyAsync();

    IAsyncEnumerable<Profile> GetManyDeletedAsync();

    Task<PagedResult<Profile>> GetManyDeletedPagedAsync(
        DeletedProfileQuery query
    );

    Task<int> CountDeletedAsync();

    void Add(Profile entity);

    void Update(Profile entity);

    void Remove(Profile entity);

    void Purge(Profile entity);

    void PurgeRange(IEnumerable<Profile> entities);

    void Restore(Profile entity);
}
