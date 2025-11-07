using Models;
using Repositories;

namespace Services.User;

public class UserConsumerHistoryService : GenericService<UserConsumerHistory>, IUserConsumerHistoryService
{
    public UserConsumerHistoryService(UserConsumerHistoryRepository repository)
        : base(repository) { }
}
