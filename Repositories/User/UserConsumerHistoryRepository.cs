using Models;

namespace Repositories;

public class UserConsumerHistoryRepository
    : GenericRepository<UserConsumerHistory>,
        IUserConsumerHistoryRepository
{
    public UserConsumerHistoryRepository(ApplicationDbContext context)
        : base(context) { }
}
