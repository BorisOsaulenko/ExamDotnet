using System.Linq.Expressions;
using Models;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.Image;

public interface IImageMetadataService
{
    Task<ImageMetadataModel> AddAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(ImageMetadataModel entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(ImageMetadataModel entity, CancellationToken cancellationToken = default);
    Task<List<ImageMetadataModel>> GetWithPaginationAsync(
        string userId,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageMetadataModel>> GetByPredicateAsync(
        string userId,
        Expression<Func<ImageMetadataModel, bool>> predicate,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    );
    Task<ImageMetadataModel?> GetByIdAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default
    );
}
