using Models;

namespace Services.Image;

public interface IImageStatsService
{
    Task<ImageStats> AddAsync(ImageStats entity, CancellationToken cancellationToken = default);
    Task IncrementViewsAsync(int imageMetadataId, CancellationToken cancellationToken = default);
    Task IncrementDownloadsAsync(int imageId, CancellationToken cancellationToken = default);
    Task IncrementSharesAsync(int imageId, CancellationToken cancellationToken = default);
    Task<bool> ToggleLikeAsync(
        int imageMetadataId,
        string userId,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(ImageStats entity, CancellationToken cancellationToken = default);
}
