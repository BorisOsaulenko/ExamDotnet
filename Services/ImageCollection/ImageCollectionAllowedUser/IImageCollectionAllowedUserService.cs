using Models;

namespace Services.ImageCollection;

public interface IImageCollectionAllowedUserService
{
    Task<ImageCollectionAllowedUser> AddAsync(
        ImageCollectionAllowedUser entity,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(ImageCollectionAllowedUser entity, CancellationToken cancellationToken = default);
}
