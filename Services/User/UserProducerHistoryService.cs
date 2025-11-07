using Models;
using Repositories;

namespace Services.User;

public class UserProducerHistoryService : GenericService<UserProducerHistory>, IUserProducerHistoryService
{
    public UserProducerHistoryService(UserProducerHistoryRepository repository)
        : base(repository) { }
}
