using Models;
using Repositories;

namespace Services.User;

public class UserFavoriteTagService : GenericService<UserFavoriteTag>, IUserFavoriteTagService
{
    public UserFavoriteTagService(UserFavoriteTagRepository repository)
        : base(repository) { }
}
