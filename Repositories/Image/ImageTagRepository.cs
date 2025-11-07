using Models;

namespace Repositories;

public class ImageTagRepository : GenericRepository<ImageTag>
{
    public ImageTagRepository(ApplicationDbContext context)
        : base(context) { }
}
