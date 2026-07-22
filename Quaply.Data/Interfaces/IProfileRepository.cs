using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(int id);
    Task<IEnumerable<Profile>> GetManyAsync();
    void Add(Profile profile);
    void Remove(Profile profile);
}
