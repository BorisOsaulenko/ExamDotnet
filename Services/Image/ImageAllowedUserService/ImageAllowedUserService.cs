using Models;
using Repositories;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.Image;

public class ImageAllowedUserService : IImageAllowedUserService
{
    private readonly IImageAllowedUserRepository _repository;
    private readonly IImageMetadataRepository _imageMetadataRepository;

    public ImageAllowedUserService(
        IImageAllowedUserRepository repository,
        IImageMetadataRepository imageRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageRepository;
    }

    public async Task<ImageAllowedUser> AddAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageMetadataModel? image = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, entity.ImageMetadataId)
            .ConfigureAwait(false);

        if (image == null || image.UserId != userId)
            throw new InvalidOperationException("Image does not exist.");

        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageAllowedUser? existingEntity = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);
        if (existingEntity == null || existingEntity.UserId != userId)
            throw new InvalidOperationException("Entity does not exist.");

        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAllForImageAsync(
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        List<ImageAllowedUser> allowedUsers = await _repository.GetByPredicateAsync(
            au => au.ImageMetadataId == imageMetadataId,
            cancellationToken
        );

        if (allowedUsers.Count == 0)
            return;

        _repository.RemoveRange(allowedUsers);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAllForUserAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        List<ImageAllowedUser> allowedUsers = await _repository.GetByPredicateAsync(
            au => au.UserId == userId,
            cancellationToken
        );

        _repository.RemoveRange(allowedUsers);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
