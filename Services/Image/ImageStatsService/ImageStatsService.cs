using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using Repositories;

namespace Services.Image;

public class ImageStatsService : IImageStatsService
{
    public ImageStatsService(
        IImageStatsRepository repository,
        IImageMetadataRepository imageMetadataRepository,
        IUserPreferencesRepository userPreferencesRepository,
        IUserRepository userRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageMetadataRepository;
        _userPreferencesRepository = userPreferencesRepository;
        _userRepository = userRepository;
    }

    private readonly IImageStatsRepository _repository;
    private readonly IImageMetadataRepository _imageMetadataRepository;
    private readonly IUserPreferencesRepository _userPreferencesRepository;
    private readonly IUserRepository _userRepository;

    public async Task<ImageStats> AddAsync(
        ImageStats entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
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

        // avoid EF trying to update related metadata when we only need stats numbers
        existingStats.ImageMetadata = null;

        _repository.Update(existingStats);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<ImageStats> GetByMetadataIdOrThrowAsync(
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ImageMetadata> query = _imageMetadataRepository
            .Query()
            .AsNoTracking()
            .Include(im => im.ImageStats)
            .Where(im => im.Id == imageMetadataId);

        ImageMetadata? existingMetadata = await Task.FromResult(query.FirstOrDefault())
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

    public async Task<bool> ToggleLikeAsync(
        int imageMetadataId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        ImageStats stats = await GetByMetadataIdOrThrowAsync(imageMetadataId, cancellationToken)
            .ConfigureAwait(false);

        ImageStats trackedStats =
            await Task.FromResult(_repository.Query().FirstOrDefault(s => s.Id == stats.Id))
                .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                "ImageStats do not exist for the specified image."
            );

        UserPreferences? preferences = await Task.FromResult(
                _userPreferencesRepository
                    .Query()
                    .Include(p => p.LikedImages)
                    .FirstOrDefault(p => p.UserId == userId)
            )
            .ConfigureAwait(false);

        if (preferences == null)
        {
            Models.User? user = await Task.FromResult(
                    _userRepository.Query().FirstOrDefault(u => u.Id == userId)
                )
                .ConfigureAwait(false);

            if (user == null)
                throw new InvalidOperationException("User does not exist.");

            preferences = new UserPreferences { UserId = userId, User = user };

            _userPreferencesRepository.Add(preferences);
            await _userPreferencesRepository
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        bool alreadyLiked = preferences.LikedImages.Any(liked => liked.Id == trackedStats.Id);

        if (alreadyLiked)
        {
            ImageStats? likeToRemove = preferences.LikedImages.FirstOrDefault(liked =>
                liked.Id == trackedStats.Id
            );

            if (likeToRemove != null)
                preferences.LikedImages.Remove(likeToRemove);
        }
        else
        {
            preferences.LikedImages.Add(trackedStats);
        }

        _userPreferencesRepository.Update(preferences);
        await _userPreferencesRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return !alreadyLiked;
    }

    public async Task RemoveAsync(ImageStats entity, CancellationToken cancellationToken = default)
    {
        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
