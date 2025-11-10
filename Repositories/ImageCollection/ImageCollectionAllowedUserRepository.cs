using Models;

namespace Repositories;

public class ImageCollectionAllowedUserRepository
    : GenericRepository<ImageCollectionAllowedUser>,
        IImageCollectionAllowedUserRepository
{
    public ImageCollectionAllowedUserRepository(ApplicationDbContext context)
        : base(context) { }
}
