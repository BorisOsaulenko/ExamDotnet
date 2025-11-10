using Models;

namespace Repositories;

public class ImageCollectionRepository
    : GenericRepository<ImageCollection>,
        IImageCollectionRepository
{
    public ImageCollectionRepository(ApplicationDbContext context)
        : base(context) { }
}
