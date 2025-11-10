using System.Linq.Expressions;
using ImageModel = Models.Image;

namespace Services.Image;

public interface IImageService
{
    Task<ImageModel> AddAsync(
        ImageModel entity,
        Stream imageStream,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageModel>> GetWithPaginationAsync(
        string userId,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageModel>> GetByPredicateAsync(
        string userId,
        Expression<Func<ImageModel, bool>> predicate,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(ImageModel entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(ImageModel entity, CancellationToken cancellationToken = default);
}
