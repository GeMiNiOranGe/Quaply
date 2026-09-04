using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(int id);

    Task<Profile?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<Profile> GetManyAsync();

    IAsyncEnumerable<Profile> GetManyDeletedAsync();

    void Add(Profile entity);

    void Update(Profile entity);

    void Remove(Profile entity);

    void Purge(Profile entity);

    void Restore(Profile entity);
}
