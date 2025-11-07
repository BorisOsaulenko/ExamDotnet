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
    Task Update(ImageStats entity, CancellationToken cancellationToken = default);
    Task Remove(ImageStats entity, CancellationToken cancellationToken = default);
}
