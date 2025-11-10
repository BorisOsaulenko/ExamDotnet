using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services.Image;

public class ImageStatsService : IImageStatsService
{
    public ImageStatsService(
        IImageStatsRepository repository,
        IImageMetadataRepository imageMetadataRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageMetadataRepository;
    }

    private readonly IImageStatsRepository _repository;
    private readonly IImageMetadataRepository _imageMetadataRepository;

    public async Task<ImageStats> AddAsync(
        ImageStats entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task<List<ImageStats>> GetByPredicateAsync(
        Expression<Func<ImageStats, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return _repository.GetByPredicateAsync(predicate, cancellationToken);
    }

    private async Task UpdateAsync(
        int imageMetadataId,
        Action<ImageStats> updateAction,
        CancellationToken cancellationToken = default
    )
    {
        ImageStats existingStats = await GetByMetadataIdOrThrowAsync(
                imageMetadataId,
                cancellationToken
            )
            .ConfigureAwait(false);

        updateAction(existingStats);

        _repository.Update(existingStats);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<ImageStats> GetByMetadataIdOrThrowAsync(
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        ImageMetadata? existingMetadata = await _imageMetadataRepository
            .Query()
            .AsNoTracking()
            .Include(im => im.ImageStats)
            .FirstOrDefaultAsync(im => im.Id == imageMetadataId, cancellationToken)
            .ConfigureAwait(false);

        if (existingMetadata == null || existingMetadata.ImageStats == null)
            throw new InvalidOperationException("ImageStats do not exist for the specified image.");

        return existingMetadata.ImageStats;
    }

    public async Task IncrementViewsAsync(
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateAsync(imageMetadataId, stats => stats.Views += 1, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task IncrementDownloadsAsync(
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateAsync(imageMetadataId, stats => stats.Downloads += 1, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task IncrementSharesAsync(
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateAsync(imageMetadataId, stats => stats.Shares += 1, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task RemoveAsync(ImageStats entity, CancellationToken cancellationToken = default)
    {
        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
