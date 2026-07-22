using Quaply.Data.Models;

namespace Quaply.Service.Interfaces;

public interface IProfileService
{
    Task<IEnumerable<Profile>> GetProfilesAsync();
    Task DeleteProfileAsync(int id);
}
