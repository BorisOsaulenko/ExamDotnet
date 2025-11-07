using System.Linq.Expressions;
using Models;

namespace Services.User;

public interface IUserConsumerHistoryService
{
    IQueryable<UserConsumerHistory> Query();
    Task<List<UserConsumerHistory>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<UserConsumerHistory?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues);
    Task<UserConsumerHistory> AddAsync(
        UserConsumerHistory entity,
        CancellationToken cancellationToken = default);
    void Update(UserConsumerHistory entity);
    void Remove(UserConsumerHistory entity);
    Task<bool> ExistsAsync(
        Expression<Func<UserConsumerHistory, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
