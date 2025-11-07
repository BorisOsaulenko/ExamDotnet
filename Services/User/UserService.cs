using ModelsUser = Models.User;
using Repositories;

namespace Services.User;

public class UserService : GenericService<ModelsUser>, IUserService
{
    public UserService(UserRepository repository)
        : base(repository) { }
}
