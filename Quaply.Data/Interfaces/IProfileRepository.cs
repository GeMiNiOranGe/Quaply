using Quaply.Data.Models;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(int id);
    Task<IEnumerable<Profile>> GetManyAsync();
    Task AddAsync(Profile profile);
    Task RemoveAsync(int id);
}
