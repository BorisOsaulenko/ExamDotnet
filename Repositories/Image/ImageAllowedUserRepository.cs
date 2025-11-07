using Models;

namespace Repositories;

public class ImageAllowedUserRepository : GenericRepository<ImageAllowedUser>
{
    public ImageAllowedUserRepository(ApplicationDbContext context)
        : base(context) { }
}
