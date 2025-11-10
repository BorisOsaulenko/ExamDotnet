using Models;

namespace Repositories;

public interface IImageStatsRepository : IGenericRepository<ImageStats>
{
    Task<ImageStats?> GetByImageIdAsync(int imageId, CancellationToken cancellationToken);
}
