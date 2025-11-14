using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using ImageCollectionModel = Models.ImageCollection;

namespace Services.ImageCollection;

public class ImageCollectionAllowedUserService : IImageCollectionAllowedUserService
{
    public ImageCollectionAllowedUserService(
        IImageCollectionAllowedUserRepository repository,
        IImageCollectionRepository imageCollectionRepository
    )
    {
        _repository = repository;
        _imageCollectionRepository = imageCollectionRepository;
    }

    private readonly IImageCollectionAllowedUserRepository _repository;
    private readonly IImageCollectionRepository _imageCollectionRepository;

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

        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
    }

    public async Task<List<ImageCollectionAllowedUser>> GetByCollectionIdAsync(
        string userId,
        int collectionId,
        CancellationToken cancellationToken = default
    )
    {
        ImageCollectionModel? imageCollection = await _imageCollectionRepository
            .GetByIdAsync(cancellationToken, collectionId)
            .ConfigureAwait(false);

        if (imageCollection == null || imageCollection.UserId != userId)
            throw new InvalidOperationException("Image collection does not exist.");

        List<ImageCollectionAllowedUser> users = _repository
            .Query()
            .AsNoTracking()
            .Where(user => user.ImageCollectionId == collectionId)
            .ToList();

        return await Task.FromResult(users);
    }

    public async Task RemoveAsync(
        ImageCollectionAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageCollectionModel? imageCollection = await _imageCollectionRepository
            .GetByIdAsync(cancellationToken, entity.ImageCollectionId)
            .ConfigureAwait(false);

        if (imageCollection == null || imageCollection.UserId != entity.UserId)
        {
            throw new InvalidOperationException("Image collection does not exist.");
        }

        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ReplaceAllowedUsersAsync(
        string userId,
        int collectionId,
        List<ImageCollectionAllowedUser> newAllowedUsers,
        CancellationToken cancellationToken = default
    )
    {
        if (newAllowedUsers.Count == 0)
            throw new ArgumentException("The list of new allowed users cannot be empty.");

        ImageCollectionModel? imageCollection = await _imageCollectionRepository
            .GetByIdAsync(cancellationToken, collectionId)
            .ConfigureAwait(false);

        if (imageCollection == null || imageCollection.UserId != userId)
            throw new InvalidOperationException("Image collection does not exist.");

        await RemoveAllByCollectionIdAsync(collectionId, cancellationToken).ConfigureAwait(false);
        await _repository.AddRangeAsync(newAllowedUsers, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAllByCollectionIdAsync(
        int collectionId,
        CancellationToken cancellationToken = default
    )
    {
        List<ImageCollectionAllowedUser> allowedUsersQuery = _repository
            .Query()
            .Where(user => user.ImageCollectionId == collectionId)
            .ToList();

        List<ImageCollectionAllowedUser> allowedUsers = await Task.FromResult(allowedUsersQuery)
            .ConfigureAwait(false);

        _repository.RemoveRange(allowedUsers);
    }
}
