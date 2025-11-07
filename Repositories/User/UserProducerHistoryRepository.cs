using Models;

namespace Repositories;

public class UserProducerHistoryRepository : GenericRepository<UserProducerHistory>
{
    public UserProducerHistoryRepository(ApplicationDbContext context)
        : base(context) { }
}
