using Models;

namespace Services.User;

public interface IUserConsumerHistoryService
{
    Task<UserConsumerHistory> AddAsync(
        UserConsumerHistory entity,
        CancellationToken cancellationToken = default
    );

    Task<Dictionary<ConsumerActivityType, List<UserConsumerHistory>>> GetUserHistory(
        string userId,
        CancellationToken cancellationToken = default
    );

    Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default);
}
