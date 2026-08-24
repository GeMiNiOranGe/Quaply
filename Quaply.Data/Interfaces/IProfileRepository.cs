using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(int id);
    Task<Profile?> GetByIdIncludingDeletedAsync(int id);
    Task<IEnumerable<Profile>> GetManyAsync();
    Task<IEnumerable<Profile>> GetManyDeletedAsync();
    void Add(Profile profile);
    void Update(Profile profile);
    void Remove(Profile profile);
    void Purge(Profile profile);
}
