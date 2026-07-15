using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;

namespace Quaply.Service;

public class ProfileService(IProfileRepository repository) : IProfileService
{
    private readonly IProfileRepository _repository = repository;

    public Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        return _repository.GetManyAsync();
    }

    public Task RemoveProfileAsync(int id)
    {
        return _repository.RemoveAsync(id);
    }
}
