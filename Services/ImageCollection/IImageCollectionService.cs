using System.Linq.Expressions;
using ImageCollectionModel = Models.ImageCollection;

namespace Services.ImageCollection;

public interface IImageCollectionService
{
    IQueryable<ImageCollectionModel> Query();
    Task<List<ImageCollectionModel>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<ImageCollectionModel?> FindByKeyAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues);
    Task<ImageCollectionModel> AddAsync(
        ImageCollectionModel entity,
        CancellationToken cancellationToken = default);
    void Update(ImageCollectionModel entity);
    void Remove(ImageCollectionModel entity);
    Task<bool> ExistsAsync(
        Expression<Func<ImageCollectionModel, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
