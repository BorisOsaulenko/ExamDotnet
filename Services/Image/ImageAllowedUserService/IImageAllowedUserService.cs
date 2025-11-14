using Models;

namespace Services.Image;

public interface IImageAllowedUserService
{
    Task<ImageAllowedUser> AddAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageAllowedUser>> GetByImageMetadataIdAsync(
        string userId,
        int imageMetadataId,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageAllowedUser>> ReplaceAsync(
        string userId,
        int imageMetadataId,
        List<ImageAllowedUser> newAllowedUsers,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    );

    Task RemoveAllForImageAsync(
        string userId,
        int imageMetadataId,
        CancellationToken cancellationToken = default
    );

    Task RemoveAllForUserAsync(string userId, CancellationToken cancellationToken = default);
}
