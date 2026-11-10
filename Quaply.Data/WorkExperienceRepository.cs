using Microsoft.EntityFrameworkCore;
using Quaply.Data.Base;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.WorkExperiences;

namespace Quaply.Data;

internal sealed class WorkExperienceRepository(QuaplyDbContext context)
    : Repository<
        WorkExperience,
        WorkExperienceSortField,
        DeletedWorkExperienceSortField
    >(context),
        IWorkExperienceRepository
{
    protected override IQueryable<WorkExperience> IncludeForPaging(
        IQueryable<WorkExperience> source
    )
    {
        return source.Include(entity => entity.Projects);
    }

    protected override IQueryable<WorkExperience> Filter(
        IQueryable<WorkExperience> source,
        ItemsQuery<WorkExperienceSortField> query
    )
    {
        return source
            .ApplySearch(query.SearchText)
            .ApplyUpdatedAtRange(query.UpdatedRange);
    }

    protected override IOrderedQueryable<WorkExperience> Sort(
        IQueryable<WorkExperience> source,
        ItemsQuery<WorkExperienceSortField> query
    )
    {
        return source.ApplySort(query.Sorting);
    }

    protected override IQueryable<WorkExperience> FilterDeleted(
        IQueryable<WorkExperience> source,
        DeletedItemsQuery<DeletedWorkExperienceSortField> query
    )
    {
        return source
            .ApplySearch(query.SearchText)
            .ApplyDeletedAtRange(query.DeletedRange);
    }

    protected override IOrderedQueryable<WorkExperience> SortDeleted(
        IQueryable<WorkExperience> source,
        DeletedItemsQuery<DeletedWorkExperienceSortField> query
    )
    {
        return source.ApplySort(query.Sorting);
    }
}
