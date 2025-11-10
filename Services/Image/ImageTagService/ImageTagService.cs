using Models;
using Repositories;
using ImageModel = Models.Image;

namespace Services.Image;

public class ImageTagService : IImageTagService
{
    public ImageTagService(ImageTagRepository repository, ImageRepository imageRepository)
    {
        _repository = repository;
        _imageRepository = imageRepository;
    }

    private readonly ImageTagRepository _repository;
    private readonly ImageRepository _imageRepository;

    public async Task<ImageTag> AddAsync(
        ImageTag entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageModel image =
            await _imageRepository
                .GetByIdAsync(cancellationToken, entity.ImageId)
                .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Image does not exist.");

        if (await ExistsAsync(entity.ImageId, entity.Tag, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Tag already exists for this image.");
        }

        entity.Id = 0;
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(ImageTag entity, CancellationToken cancellationToken = default)
    {
        ImageModel image =
            await _imageRepository
                .GetByIdAsync(cancellationToken, entity.ImageId)
                .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Image does not exist.");

        if (await ExistsAsync(entity.ImageId, entity.Tag, cancellationToken).ConfigureAwait(false))
        {
            _repository.Remove(entity);
            await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> ExistsAsync(
        int imageId,
        string tag,
        CancellationToken cancellationToken = default
    )
    {
        return await _repository
            .ExistsAsync(it => it.Tag == tag && it.ImageId == imageId, cancellationToken)
            .ConfigureAwait(false);
    }
}
