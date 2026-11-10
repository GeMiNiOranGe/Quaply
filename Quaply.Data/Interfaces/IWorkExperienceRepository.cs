using Quaply.Data.Models;
using Quaply.Data.Querying.WorkExperiences;

namespace Quaply.Data.Interfaces;

public interface IWorkExperienceRepository
    : IRepository<
        WorkExperience,
        WorkExperienceSortField,
        DeletedWorkExperienceSortField
    >;
