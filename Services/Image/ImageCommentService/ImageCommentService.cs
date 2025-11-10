using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using Services.Util;
using ImageStatsModel = Models.ImageStats;

namespace Services.Image;

public partial class ImageCommentService : IImageCommentService
{
    public ImageCommentService(
        IImageCommentRepository repository,
        IImageStatsRepository imageStatsRepository
    )
    {
        _repository = repository;
        _imageStatsRepository = imageStatsRepository;
    }

    private readonly IImageCommentRepository _repository;
    private readonly IImageStatsRepository _imageStatsRepository;

    public async Task<ImageComment> AddAsync(
        ImageComment entity,
        CancellationToken cancellationToken = default
    )
    {
        CreateImageCommentValidator().ValidateAndThrow(entity);
        ImageStatsModel? image = await _imageStatsRepository
            .GetByIdAsync(cancellationToken, entity.ImageId)
            .ConfigureAwait(false);

        if (image == null || !ServiceUtils.Image.UserHasAccess(image, entity.UserId))
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to comment on this image."
            );
        }
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task<List<ImageComment>> GetByPredicateAsync(
        string currentUserId,
        Expression<Func<ImageComment, bool>> predicate,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        var accessPredicate = ServiceUtils.Image.BuildAccessPredicate(currentUserId);

        IQueryable<ImageComment> q = _repository
            .Query()
            .Where(predicate)
            .Join(
                _imageStatsRepository.Query().Where(accessPredicate),
                comment => comment.ImageId,
                image => image.Id,
                (comment, image) => new { comment, image }
            )
            .Select(joined => joined.comment)
            .Skip(pagination.Skip)
            .Take(pagination.Size);
        return q.ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        ImageComment entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        ImageComment? existingComment = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingComment == null || existingComment.UserId != entity.UserId)
        {
            throw new InvalidOperationException("Comment does not exist.");
        }

        _repository.Remove(existingComment);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        ImageComment entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        CreateImageCommentValidator().ValidateAndThrow(entity);

        ImageComment? existingComment = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingComment == null || existingComment.UserId != entity.UserId)
        {
            throw new InvalidOperationException("Comment does not exist.");
        }

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
