using Models;
using Repositories;
using ImageModel = Models.Image;

namespace Services.Image;

public class ImageAllowedUserService : IImageAllowedUserService
{
    private readonly IImageAllowedUserRepository _repository;
    private readonly IImageRepository _imageRepository;

    public ImageAllowedUserService(
        IImageAllowedUserRepository repository,
        IImageRepository imageRepository
    )
    {
        _repository = repository;
        _imageRepository = imageRepository;
    }

    public async Task<ImageAllowedUser> AddAsync(
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageModel? image = await _imageRepository
            .GetByIdAsync(cancellationToken, entity.ImageId)
            .ConfigureAwait(false);

        if (image == null || image.Id != entity.ImageId)
        {
            throw new InvalidOperationException("Image does not exist.");
        }
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageAllowedUser? existingEntity = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);
        if (existingEntity == null || existingEntity.Id != entity.Id)
        {
            throw new InvalidOperationException("Entity does not exist.");
        }

        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAllForImageAsync(
        int imageId,
        CancellationToken cancellationToken = default
    )
    {
        List<ImageAllowedUser> allowedUsers = await _repository.GetByPredicateAsync(
            au => au.ImageId == imageId,
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
