using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(int id);

    Task<Profile?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<Profile> GetManyAsync();

    IAsyncEnumerable<Profile> GetManyDeletedAsync();

    void Add(Profile profile);

    void Update(Profile profile);

    void Remove(Profile profile);

    void Purge(Profile profile);

    void Restore(Profile profile);
}
