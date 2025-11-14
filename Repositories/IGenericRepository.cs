using System.Linq.Expressions;

namespace Repositories;

public interface IGenericRepository<TEntity>
    where TEntity : class
{
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByIdAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues
    );

    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<List<TEntity>> GetWithPaginationAsync(
        int skip,
        int size,
        CancellationToken cancellationToken = default
    );

    ValueTask<TEntity?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues
    );

    TEntity Add(TEntity entity);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    void Update(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    void RemoveByPredicate(Expression<Func<TEntity, bool>> predicate);

    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
