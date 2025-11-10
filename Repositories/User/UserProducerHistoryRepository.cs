using Models;

namespace Repositories;

public class UserProducerHistoryRepository
    : GenericRepository<UserProducerHistory>,
        IUserProducerHistoryRepository
{
    public UserProducerHistoryRepository(ApplicationDbContext context)
        : base(context) { }
}
