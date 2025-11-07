using Models;

namespace Repositories;

public class UserRepository : GenericRepository<User>
{
    public UserRepository(ApplicationDbContext context)
        : base(context) { }
}
