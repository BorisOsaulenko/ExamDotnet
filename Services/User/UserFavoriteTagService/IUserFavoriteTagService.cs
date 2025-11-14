using Models;

namespace Services.User;

public interface IUserFavoriteTagService
{
    Task<UserFavoriteTag> AddAsync(
        string userId,
        UserFavoriteTag entity,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(
        string userId,
        UserFavoriteTag entity,
        CancellationToken cancellationToken = default
    );
    Task RemoveAllAsync(int userPreferencesId, CancellationToken cancellationToken = default);
}
