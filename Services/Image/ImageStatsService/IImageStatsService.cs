using System.Linq.Expressions;
using Models;

namespace Services.Image;

public interface IImageStatsService
{
    Task<ImageStats> AddAsync(ImageStats entity, CancellationToken cancellationToken = default);
    Task<List<ImageStats>> GetByPredicateAsync(
        Expression<Func<ImageStats, bool>> predicate,
        CancellationToken cancellationToken = default
    );
    Task IncrementViewsAsync(int imageId, CancellationToken cancellationToken = default);
    Task IncrementDownloadsAsync(int imageId, CancellationToken cancellationToken = default);
    Task IncrementSharesAsync(int imageId, CancellationToken cancellationToken = default);
    Task RemoveAsync(ImageStats entity, CancellationToken cancellationToken = default);
}
