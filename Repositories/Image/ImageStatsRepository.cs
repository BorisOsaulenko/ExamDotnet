using Models;

namespace Repositories;

public class ImageStatsRepository : GenericRepository<ImageStats>
{
    public ImageStatsRepository(ApplicationDbContext context)
        : base(context) { }
}
