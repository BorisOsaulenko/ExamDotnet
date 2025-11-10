using Models;

namespace Services.User;

public interface IUserPreferencesService
{
    Task<UserPreferences> AddAsync(
        UserPreferences entity,
        CancellationToken cancellationToken = default
    );
    Task<UserPreferences?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(UserPreferences entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(UserPreferences entity, CancellationToken cancellationToken = default);
}
