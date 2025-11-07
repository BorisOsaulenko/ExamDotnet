using System.Linq.Expressions;
using Models;
using Repositories;
using Services.Identity;
using Services.Util;
using ImageModel = Models.Image;

namespace Services.Image;

public class ImageAllowedUserService : IImageAllowedUserService
{
    private readonly ImageAllowedUserRepository _repository;
    private readonly ImageRepository _imageRepository;
    private readonly ICurrentUserService _currentUserService;

    public ImageAllowedUserService(
        ImageAllowedUserRepository repository,
        ImageRepository imageRepository,
        ICurrentUserService currentUserService
    )
    {
        _repository = repository;
        _imageRepository = imageRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ImageAllowedUser> AddAsync(
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        string currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);
        ImageModel? image = await _imageRepository
            .GetByIdAsync(cancellationToken, entity.ImageId)
            .ConfigureAwait(false);

        if (image == null || image.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to modify this image."
            );
        }
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task<List<ImageAllowedUser>> GetByPredicateAsync(
        Expression<Func<ImageAllowedUser, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return _repository.GetByPredicateAsync(predicate, cancellationToken);
    }

    public async Task Remove(ImageAllowedUser entity, CancellationToken cancellationToken = default)
    {
        string currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);
        ImageModel? image = await _imageRepository.GetByIdAsync(cancellationToken, entity.ImageId);

        if (image == null || image.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to modify this image."
            );
        }

        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
