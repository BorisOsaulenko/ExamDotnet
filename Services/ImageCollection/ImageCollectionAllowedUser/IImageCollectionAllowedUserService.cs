using Models;

namespace Services.ImageCollection;

public interface IImageCollectionAllowedUserService
{
    Task<ImageCollectionAllowedUser> AddAsync(
        ImageCollectionAllowedUser entity,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(
        ImageCollectionAllowedUser entity,
        CancellationToken cancellationToken = default
    );

    Task ReplaceAllowedUsersAsync(
        string userId,
        int collectionId,
        List<ImageCollectionAllowedUser> newAllowedUsers,
        CancellationToken cancellationToken = default
    );

    Task<List<ImageCollectionAllowedUser>> GetByCollectionIdAsync(
        string userId,
        int collectionId,
        CancellationToken cancellationToken = default
    );

    Task RemoveAllByCollectionIdAsync(
        int collectionId,
        CancellationToken cancellationToken = default
    );
}
