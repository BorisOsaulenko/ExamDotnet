using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using MetadataModel = Models.ImageMetadata;

namespace Services.Image;

public class ImageTagService : IImageTagService
{
    public ImageTagService(
        IImageTagRepository repository,
        IImageMetadataRepository imageMetadataRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageMetadataRepository;
    }

    private readonly IImageTagRepository _repository;
    private readonly IImageMetadataRepository _imageMetadataRepository;

    public async Task<ImageTag> AddAsync(
        string userId,
        ImageTag entity,
        CancellationToken cancellationToken = default
    )
    {
        MetadataModel? imageMetadata = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, entity.ImageMetadataId)
            .ConfigureAwait(false);

        if (imageMetadata == null || imageMetadata.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to tag this image.");

        if (
            await ExistsAsync(entity.ImageMetadataId, entity.Tag, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            throw new InvalidOperationException("Tag already exists for this image.");
        }

        entity.Id = 0;
        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
    }

    public async Task<List<ImageTag>> GetByImageMetadataIdAsync(
        string userId,
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        MetadataModel? metadata = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, imageMetadataId)
            .ConfigureAwait(false);

        if (metadata == null || metadata.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to view these tags.");

        return await _repository
            .GetByPredicateAsync(it => it.ImageMetadataId == imageMetadataId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<ImageTag>> ReplaceAsync(
        string userId,
        int imageMetadataId,
        List<ImageTag> newTags,
        CancellationToken cancellationToken = default
    )
    {
        MetadataModel? metadata = await _imageMetadataRepository
            .Query()
            .AsNoTracking()
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == imageMetadataId, cancellationToken);

        if (metadata == null || metadata.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to modify these tags."
            );

        _repository.RemoveRange(metadata.Tags);

        List<ImageTag> addedTags = new List<ImageTag>();
        foreach (var tag in newTags)
        {
            tag.ImageMetadataId = imageMetadataId;
            ImageTag addedTag = _repository.Add(tag);
            addedTags.Add(addedTag);
        }

        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return addedTags;
    }

    public async Task RemoveAsync(
        string userId,
        ImageTag entity,
        CancellationToken cancellationToken = default
    )
    {
        MetadataModel? metadata = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, entity.ImageMetadataId)
            .ConfigureAwait(false);

        if (metadata == null || metadata.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to remove this tag.");

        if (
            await ExistsAsync(entity.ImageMetadataId, entity.Tag, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            _repository.Remove(entity);
            await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            throw new InvalidOperationException("Tag does not exist for this image.");
        }
    }

    private async Task<bool> ExistsAsync(
        int imageMetadataId,
        string tag,
        CancellationToken cancellationToken = default
    )
    {
        return await _repository
            .ExistsAsync(
                it => it.Tag == tag && it.ImageMetadataId == imageMetadataId,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
