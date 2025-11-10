using Models;

namespace Repositories;

public class UserFavoriteTagRepository
    : GenericRepository<UserFavoriteTag>,
        IUserFavoriteTagRepository
{
    public UserFavoriteTagRepository(ApplicationDbContext context)
        : base(context) { }
}
