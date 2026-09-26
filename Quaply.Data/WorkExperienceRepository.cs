using Microsoft.EntityFrameworkCore;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
using Quaply.Data.Models;
using Quaply.Data.Querying;

namespace Quaply.Data;

internal class WorkExperienceRepository(QuaplyDbContext context)
    : IWorkExperienceRepository
{
    private readonly QuaplyDbContext _context = context;

    public Task<WorkExperience?> GetByIdAsync(int id)
    {
        return _context.WorkExperiences.FirstOrDefaultAsync(entity =>
            entity.Id == id
        );
    }

    public Task<WorkExperience?> GetByIdIncludingDeletedAsync(int id)
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public IAsyncEnumerable<WorkExperience> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    )
    {
        return _context
            .WorkExperiences.IgnoreQueryFilters()
            .Where(entity => ids.Contains(entity.Id))
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<WorkExperience> GetManyAsync()
    {
        return _context
            .WorkExperiences.AsNoTracking()
            .Include(entity => entity.Projects)
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<WorkExperience> GetManyDeletedAsync(
        WorkExperienceDeletedQuery query
    )
    {
        IQueryable<WorkExperience> filteredQuery = _context
            .WorkExperiences.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(entity => entity.DeletedAt != null);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            // Escape LIKE wildcard characters ('%', '_') if the user types them
            // literally, to prevent the input from being interpreted
            // as an unintended pattern.
            string term = query
                .SearchText.Trim()
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            // NOTE: SQLite's LIKE operator is case-insensitive only for
            // ASCII characters. Vietnamese characters with diacritics (e.g.,
            //"Đà Nẵng" vs. "đà nẵng") may not match if the casing differs.
            // This limitation is acceptable for the initial version;
            // it can be addressed later if necessary.
            filteredQuery = filteredQuery.Where(entity =>
                EF.Functions.Like(entity.CompanyName, $"%{term}%", "\\")
                || EF.Functions.Like(entity.PositionTitle, $"%{term}%", "\\")
            );
        }

        filteredQuery = query.Sort.Field switch
        {
            WorkExperienceSortField.CompanyName => query.Sort.Descending
                ? filteredQuery.OrderByDescending(e => e.CompanyName)
                : filteredQuery.OrderBy(e => e.CompanyName),
            _ => query.Sort.Descending
                ? filteredQuery.OrderByDescending(e => e.DeletedAt)
                : filteredQuery.OrderBy(e => e.DeletedAt),
        };

        return filteredQuery.AsAsyncEnumerable();
    }

    public void Add(WorkExperience entity)
    {
        _context.WorkExperiences.Add(entity);
    }

    public void Update(WorkExperience entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
    }

    public void Remove(WorkExperience entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
    }

    public void Purge(WorkExperience entity)
    {
        _context.WorkExperiences.Remove(entity);
    }

    public void PurgeRange(IEnumerable<WorkExperience> entities)
    {
        _context.WorkExperiences.RemoveRange(entities);
    }

    public void Restore(WorkExperience entity)
    {
        entity.DeletedAt = null;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.WorkExperiences.Update(entity);
    }

    public void RestoreRange(IEnumerable<WorkExperience> entities)
    {
        foreach (WorkExperience entity in entities)
        {
            entity.DeletedAt = null;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        _context.WorkExperiences.UpdateRange(entities);
    }
}
