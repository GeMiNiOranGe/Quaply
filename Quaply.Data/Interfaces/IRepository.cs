using Quaply.Data.Models;
using Quaply.Data.Querying.Base;

namespace Quaply.Data.Interfaces;

public interface IRepository<TEntity, TSortField, TDeletedSortField>
    where TEntity : class, IEntity, ISoftDeletable
    where TSortField : struct, Enum
    where TDeletedSortField : struct, Enum
{
    Task<TEntity?> GetByIdAsync(int id);

    Task<TEntity?> GetByIdIncludingDeletedAsync(int id);

    IAsyncEnumerable<TEntity> GetManyByIdsAsync(IEnumerable<int> ids);

    IAsyncEnumerable<TEntity> GetManyByIdsIncludingDeletedAsync(
        IEnumerable<int> ids
    );

    IAsyncEnumerable<TEntity> GetManyDeletedAsync();

    Task<PagedResult<TEntity>> GetManyPagedAsync(ItemsQuery<TSortField> query);

    Task<PagedResult<TEntity>> GetManyDeletedPagedAsync(
        DeletedItemsQuery<TDeletedSortField> query
    );

    Task<int> CountAsync();

    Task<int> CountDeletedAsync();

    void Add(TEntity entity);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    void Restore(TEntity entity);

    void RestoreRange(IEnumerable<TEntity> entities);

    void Purge(TEntity entity);

    void PurgeRange(IEnumerable<TEntity> entities);
}
