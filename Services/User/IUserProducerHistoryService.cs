using System.Linq.Expressions;
using Models;

namespace Services.User;

public interface IUserProducerHistoryService
{
    IQueryable<UserProducerHistory> Query();
    Task<List<UserProducerHistory>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<UserProducerHistory?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues);
    Task<UserProducerHistory> AddAsync(
        UserProducerHistory entity,
        CancellationToken cancellationToken = default);
    void Update(UserProducerHistory entity);
    void Remove(UserProducerHistory entity);
    Task<bool> ExistsAsync(
        Expression<Func<UserProducerHistory, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
