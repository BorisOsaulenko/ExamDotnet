using Models;

namespace Repositories;

public class ImageCollectionRepository : GenericRepository<ImageCollection>
{
    public ImageCollectionRepository(ApplicationDbContext context)
        : base(context) { }
}
