using System.Linq.Expressions;
using Models;

namespace Services.ImageCollection;

public interface IImageCollectionAllowedUserService
{
    IQueryable<ImageCollectionAllowedUser> Query();
    Task<List<ImageCollectionAllowedUser>> GetAllAsync(
        CancellationToken cancellationToken = default);
    ValueTask<ImageCollectionAllowedUser?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues);
    Task<ImageCollectionAllowedUser> AddAsync(
        ImageCollectionAllowedUser entity,
        CancellationToken cancellationToken = default);
    void Update(ImageCollectionAllowedUser entity);
    void Remove(ImageCollectionAllowedUser entity);
    Task<bool> ExistsAsync(
        Expression<Func<ImageCollectionAllowedUser, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
