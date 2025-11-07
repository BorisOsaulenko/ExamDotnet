using System.Linq.Expressions;
using Models;
using ImageModel = Models.Image;

namespace Services.Image;

public interface IImageService
{
    Task<ImageModel> AddAsync(
        ImageModel entity,
        Stream imageStream,
        ImageContentType contentType,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageModel>> GetWithPaginationAsync(
        int skip,
        int size,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageModel>> GetByPredicateAsync(
        Expression<Func<ImageModel, bool>> predicate,
        CancellationToken cancellationToken = default
    );
    Task Update(ImageModel entity, CancellationToken cancellationToken = default);
    Task Remove(ImageModel entity, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Expression<Func<ImageModel, bool>> predicate,
        CancellationToken cancellationToken = default
    );
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
