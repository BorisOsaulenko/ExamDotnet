using System.Linq.Expressions;
using ImageCollectionModel = Models.ImageCollection;

namespace Services.ImageCollection;

public interface IImageCollectionService
{
    Task<ImageCollectionModel> AddAsync(
        ImageCollectionModel entity,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageCollectionModel>> GetByPredicateAsync(
        Expression<Func<ImageCollectionModel, bool>> predicate,
        string currentUserId,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(ImageCollectionModel entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(ImageCollectionModel entity, CancellationToken cancellationToken = default);
}
