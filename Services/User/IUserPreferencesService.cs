using System.Linq.Expressions;
using Models;

namespace Services.User;

public interface IUserPreferencesService
{
    IQueryable<UserPreferences> Query();
    Task<List<UserPreferences>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<UserPreferences?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues);
    Task<UserPreferences> AddAsync(UserPreferences entity, CancellationToken cancellationToken = default);
    void Update(UserPreferences entity);
    void Remove(UserPreferences entity);
    Task<bool> ExistsAsync(
        Expression<Func<UserPreferences, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
