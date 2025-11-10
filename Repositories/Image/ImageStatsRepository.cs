using Microsoft.EntityFrameworkCore;
using Models;

namespace Repositories;

public class ImageStatsRepository : GenericRepository<ImageStats>, IImageStatsRepository
{
    public ImageStatsRepository(ApplicationDbContext context)
        : base(context) { }

    public async Task<ImageStats?> GetByImageIdAsync(
        int imageId,
        CancellationToken cancellationToken
    )
    {
        return await Entities
            .FirstOrDefaultAsync(stats => stats.ImageId == imageId, cancellationToken)
            .ConfigureAwait(false);
    }
}
