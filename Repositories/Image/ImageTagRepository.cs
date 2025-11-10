using Models;

namespace Repositories;

public class ImageTagRepository : GenericRepository<ImageTag>, IImageTagRepository
{
    public ImageTagRepository(ApplicationDbContext context)
        : base(context) { }
}
