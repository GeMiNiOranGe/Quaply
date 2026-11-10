using Microsoft.EntityFrameworkCore;
using Quaply.Data.Base;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying.Base;
using Quaply.Data.Querying.Profiles;

namespace Quaply.Data;

internal sealed class ProfileRepository(QuaplyDbContext context)
    : Repository<Profile, ProfileSortField, DeletedProfileSortField>(context),
        IProfileRepository
{
    protected override IQueryable<Profile> IncludeForPaging(
        IQueryable<Profile> source
    )
    {
        return source.Include(entity => entity.ResumeProfiles);
    }

    protected override IQueryable<Profile> Filter(
        IQueryable<Profile> source,
        ItemsQuery<ProfileSortField> query
    )
    {
        return source
            .ApplySearch(query.SearchText)
            .ApplyUpdatedAtRange(query.UpdatedRange);
    }

    protected override IOrderedQueryable<Profile> Sort(
        IQueryable<Profile> source,
        ItemsQuery<ProfileSortField> query
    )
    {
        return source.ApplySort(query.Sorting);
    }

    protected override IQueryable<Profile> FilterDeleted(
        IQueryable<Profile> source,
        DeletedItemsQuery<DeletedProfileSortField> query
    )
    {
        return source
            .ApplySearch(query.SearchText)
            .ApplyDeletedAtRange(query.DeletedRange);
    }

    protected override IOrderedQueryable<Profile> SortDeleted(
        IQueryable<Profile> source,
        DeletedItemsQuery<DeletedProfileSortField> query
    )
    {
        return source.ApplySort(query.Sorting);
    }
}
