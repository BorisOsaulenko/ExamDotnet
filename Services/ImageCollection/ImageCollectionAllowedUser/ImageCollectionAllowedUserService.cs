using Models;
using Repositories;
using ImageCollectionModel = Models.ImageCollection;

namespace Services.ImageCollection;

public class ImageCollectionAllowedUserService : IImageCollectionAllowedUserService
{
    public ImageCollectionAllowedUserService(
        ImageCollectionAllowedUserRepository repository,
        ImageCollectionRepository imageCollectionRepository
    )
    {
        _repository = repository;
        _imageCollectionRepository = imageCollectionRepository;
    }

    private readonly ImageCollectionAllowedUserRepository _repository;
    private readonly ImageCollectionRepository _imageCollectionRepository;

    public async Task<ImageCollectionAllowedUser> AddAsync(
        ImageCollectionAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageCollectionModel? imageCollection =
            await _imageCollectionRepository
                .GetByIdAsync(cancellationToken, entity.ImageCollectionId)
                .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Image collection does not exist.");

        if (imageCollection.UserId != entity.UserId)
        {
            throw new InvalidOperationException("Image collection does not exist.");
        }

        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        ImageCollectionAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageCollectionModel? imageCollection = await _imageCollectionRepository
            .GetByIdAsync(cancellationToken, entity.ImageCollectionId)
            .ConfigureAwait(false);

        if (imageCollection == null)
        {
            throw new InvalidOperationException("Image collection does not exist.");
        }

        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
