using Models;

namespace Repositories;

public class ImageRepository : GenericRepository<Image>, IImageRepository
{
    public ImageRepository(ApplicationDbContext context)
        : base(context) { }
}
