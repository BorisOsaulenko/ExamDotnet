using Models;

namespace Repositories;

public class UserConsumerHistoryRepository : GenericRepository<UserConsumerHistory>
{
    public UserConsumerHistoryRepository(ApplicationDbContext context)
        : base(context) { }
}
