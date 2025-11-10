using Microsoft.EntityFrameworkCore;
using Models;

namespace Repositories;

public class ImageStatsRepository : GenericRepository<ImageStats>, IImageStatsRepository
{
    public ImageStatsRepository(ApplicationDbContext context)
        : base(context) { }
}
