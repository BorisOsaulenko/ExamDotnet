using Models;
using Repositories;
using MetadataModel = Models.ImageMetadata;

namespace Services.Image;

public class ImageTagService : IImageTagService
{
    public ImageTagService(
        ImageTagRepository repository,
        ImageMetadataRepository imageMetadataRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageMetadataRepository;
    }

    private readonly ImageTagRepository _repository;
    private readonly ImageMetadataRepository _imageMetadataRepository;

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
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
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
