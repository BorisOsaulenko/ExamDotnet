using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using Services.Identity;
using Services.Util;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageCommentService : IImageCommentService
{
    public ImageCommentService(
        ImageCommentRepository repository,
        ImageRepository imageRepository,
        ICurrentUserService currentUserService
    )
    {
        _repository = repository;
        _imageRepository = imageRepository;
        _currentUserService = currentUserService;
    }

    private readonly ImageCommentRepository _repository;
    private readonly ImageRepository _imageRepository;
    private readonly ICurrentUserService _currentUserService;

    public async Task<ImageComment> AddAsync(
        ImageComment entity,
        CancellationToken cancellationToken = default
    )
    {
        CreateImageCommentValidator().ValidateAndThrow(entity);
        ImageModel? image = await _imageRepository
            .GetByIdAsync(cancellationToken, entity.ImageId)
            .ConfigureAwait(false);

        if (image == null || !ServiceUtils.UserHasAccess(image, entity.UserId))
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to comment on this image."
            );
        }
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task<List<ImageComment>> GetByPredicateAsync(
        Expression<Func<ImageComment, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        string currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);
        var accessPredicate = ServiceUtils.BuildAccessPredicate(currentUserId);

        IQueryable<ImageComment> q = _repository
            .Query()
            .Where(predicate)
            .Join(
                _imageRepository.Query().Where(accessPredicate),
                comment => comment.ImageId,
                image => image.Id,
                (comment, image) => new { comment, image }
            )
            .Select(joined => joined.comment);
        return q.ToListAsync(cancellationToken);
    }

    public async Task Remove(ImageComment entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        string currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);

        ImageComment? existingComment = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingComment == null || existingComment.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "Comment does not exist or you do not have permission to delete it."
            );
        }

        _repository.Remove(existingComment);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(ImageComment entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        CreateImageCommentValidator().ValidateAndThrow(entity);

        string currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);

        ImageComment? existingComment = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingComment == null || existingComment.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "Comment does not exist or you do not have permission to delete it."
            );
        }

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
