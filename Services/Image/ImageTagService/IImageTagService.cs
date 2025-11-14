using Models;

namespace Services.Image;

public interface IImageTagService
{
    Task<ImageTag> AddAsync(
        string userId,
        ImageTag entity,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageTag>> GetByImageMetadataIdAsync(
        string userId,
        int imageMetadataId,
        CancellationToken cancellationToken = default
    );
    Task<List<ImageTag>> ReplaceAsync(
        string userId,
        int imageMetadataId,
        List<ImageTag> newTags,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(string userId, ImageTag entity, CancellationToken cancellationToken = default);
}
