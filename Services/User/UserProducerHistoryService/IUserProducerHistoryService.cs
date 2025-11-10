using Models;

namespace Services.User;

public interface IUserProducerHistoryService
{
    Task<UserProducerHistory> AddAsync(
        UserProducerHistory entity,
        CancellationToken cancellationToken = default
    );
    Task<Dictionary<UserHistoryType, List<UserHistoryKey>>> GetUserHistory(
        string userId,
        CancellationToken cancellationToken = default
    );
    Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default);
}
