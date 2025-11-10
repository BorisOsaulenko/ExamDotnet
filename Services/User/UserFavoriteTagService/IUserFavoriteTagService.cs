using Models;

namespace Services.User;

public interface IUserFavoriteTagService
{
    Task<UserFavoriteTag> AddAsync(
        UserFavoriteTag entity,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(UserFavoriteTag entity, CancellationToken cancellationToken = default);
    Task RemoveAllAsync(int userPreferencesId, CancellationToken cancellationToken = default);
}
