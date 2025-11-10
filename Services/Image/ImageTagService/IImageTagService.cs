using Models;

namespace Services.Image;

public interface IImageTagService
{
    Task<ImageTag> AddAsync(string userId, ImageTag entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(string userId, ImageTag entity, CancellationToken cancellationToken = default);
}
