using System.Linq.Expressions;
using ModelsUser = Models.User;

namespace Services.User;

public interface IUserService
{
    IQueryable<ModelsUser> Query();
    Task<List<ModelsUser>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<ModelsUser?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues);
    Task<ModelsUser> AddAsync(ModelsUser entity, CancellationToken cancellationToken = default);
    void Update(ModelsUser entity);
    void Remove(ModelsUser entity);
    Task<bool> ExistsAsync(
        Expression<Func<ModelsUser, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
