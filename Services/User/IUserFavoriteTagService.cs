using System.Linq.Expressions;
using Models;

namespace Services.User;

public interface IUserFavoriteTagService
{
    IQueryable<UserFavoriteTag> Query();
    Task<List<UserFavoriteTag>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<UserFavoriteTag?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues);
    Task<UserFavoriteTag> AddAsync(UserFavoriteTag entity, CancellationToken cancellationToken = default);
    void Update(UserFavoriteTag entity);
    void Remove(UserFavoriteTag entity);
    Task<bool> ExistsAsync(
        Expression<Func<UserFavoriteTag, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
