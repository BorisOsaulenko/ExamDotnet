using Models;

namespace Services.Image;

public interface IImageAllowedUserService
{
    Task<ImageAllowedUser> AddAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    );
}
