using Quaply.Data.Models;
using Quaply.Data.Querying.Profiles;

namespace Quaply.Data.Interfaces;

public interface IProfileRepository
    : IRepository<Profile, ProfileSortField, DeletedProfileSortField>;
