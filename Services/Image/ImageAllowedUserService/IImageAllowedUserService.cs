using Models;

namespace Services.Image;

public interface IImageAllowedUserService
{
    Task<ImageAllowedUser> AddAsync(
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(ImageAllowedUser entity, CancellationToken cancellationToken = default);
}
