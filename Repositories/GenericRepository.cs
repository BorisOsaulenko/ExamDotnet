using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repositories;

public abstract class GenericRepository<TEntity>
    where TEntity : class
{
    protected GenericRepository(ApplicationDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Entities = Context.Set<TEntity>();
    }

    protected ApplicationDbContext Context { get; }
    protected DbSet<TEntity> Entities { get; }

    public virtual IQueryable<TEntity> Query() => Entities;

    public virtual Task<TEntity?> GetByIdAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues
    ) => FindByKeyAsync(cancellationToken, keyValues).AsTask();

    public virtual Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Entities.ToListAsync(cancellationToken);

    public virtual Task<List<TEntity>> GetByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Entities.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual Task<List<TEntity>> GetWithPaginationAsync(
        int skip,
        int size,
        CancellationToken cancellationToken = default
    )
    {
        if (skip < 0 || size <= 0)
            throw new ArgumentOutOfRangeException(
                "Skip must be non-negative and size must be positive."
            );

        return Entities.Skip(skip).Take(size).ToListAsync(cancellationToken);
    }

    public virtual ValueTask<TEntity?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues
    )
    {
        if (keyValues == null || keyValues.Length == 0)
        {
            throw new ArgumentException(
                "At least one key value must be provided.",
                nameof(keyValues)
            );
        }

        return Entities.FindAsync(keyValues, cancellationToken);
    }

    public virtual TEntity Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Add(entity);

        return entity;
    }

    public virtual async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Add(entity);
        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity;
    }

    public virtual Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        return Entities.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        Entities.RemoveRange(entities);
    }

    public virtual void RemoveByPredicate(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var entitiesToRemove = Entities.Where(predicate).ToList();
        Entities.RemoveRange(entitiesToRemove);
    }

    public virtual Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Entities.AnyAsync(predicate, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Context.SaveChangesAsync(cancellationToken);
}
