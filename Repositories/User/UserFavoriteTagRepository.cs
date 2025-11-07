using Models;

namespace Repositories;

public class UserFavoriteTagRepository : GenericRepository<UserFavoriteTag>
{
    public UserFavoriteTagRepository(ApplicationDbContext context)
        : base(context) { }
}
