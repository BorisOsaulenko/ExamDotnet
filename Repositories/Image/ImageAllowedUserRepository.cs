using Models;

namespace Repositories;

public class ImageAllowedUserRepository
    : GenericRepository<ImageAllowedUser>,
        IImageAllowedUserRepository
{
    public ImageAllowedUserRepository(ApplicationDbContext context)
        : base(context) { }
}
