using Models;

namespace Services.Image;

public interface IImageTagService
{
    Task<ImageTag> AddAsync(ImageTag entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(ImageTag entity, CancellationToken cancellationToken = default);
}
