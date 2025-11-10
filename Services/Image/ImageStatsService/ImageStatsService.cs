using System.Linq.Expressions;
using Models;
using Repositories;

namespace Services.Image;

public class ImageStatsService : IImageStatsService
{
    public ImageStatsService(IImageStatsRepository repository)
    {
        _repository = repository;
    }

    private readonly IImageStatsRepository _repository;

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
        ImageStats entity,
        Action<ImageStats> updateAction,
        CancellationToken cancellationToken = default
    )
    {
        ImageStats existingStats = await GetOrThrowAsync(entity.ImageId, cancellationToken)
            .ConfigureAwait(false);

        updateAction(existingStats);

        _repository.Update(existingStats);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<ImageStats> GetOrThrowAsync(
        int imageId,
        CancellationToken cancellationToken = default
    )
    {
        ImageStats? stats =
            await _repository.GetByImageIdAsync(imageId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("ImageStats not found");
        return stats;
    }

    public async Task IncrementViewsAsync(
        int imageId,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateAsync(
                new ImageStats { ImageId = imageId },
                stats =>
                {
                    stats.Views += 1;
                },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public async Task IncrementLikesAsync(
        int imageId,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateAsync(
                new ImageStats { ImageId = imageId },
                stats =>
                {
                    stats.Likes += 1;
                },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public async Task IncrementDownloadsAsync(
        int imageId,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateAsync(
                new ImageStats { ImageId = imageId },
                stats =>
                {
                    stats.Downloads += 1;
                },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public async Task IncrementSharesAsync(
        int imageId,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateAsync(
                new ImageStats { ImageId = imageId },
                stats =>
                {
                    stats.Shares += 1;
                },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public async Task RemoveAsync(ImageStats entity, CancellationToken cancellationToken = default)
    {
        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
