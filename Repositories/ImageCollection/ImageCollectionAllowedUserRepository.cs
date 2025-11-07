using Models;

namespace Repositories;

public class ImageCollectionAllowedUserRepository : GenericRepository<ImageCollectionAllowedUser>
{
    public ImageCollectionAllowedUserRepository(ApplicationDbContext context)
        : base(context) { }
}
