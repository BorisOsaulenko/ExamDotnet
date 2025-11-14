using System.Linq.Expressions;
using FluentValidation;
using Models;
using Repositories;
using Services.Util;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.Image;

public partial class ImageCommentService : IImageCommentService
{
    public ImageCommentService(
        IImageCommentRepository repository,
        IImageMetadataRepository imageMetadataRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageMetadataRepository;
    }

    private readonly IImageCommentRepository _repository;
    private readonly IImageMetadataRepository _imageMetadataRepository;

    public async Task<ImageComment> AddAsync(
        ImageComment entity,
        CancellationToken cancellationToken = default
    )
    {
        CreateImageCommentValidator().ValidateAndThrow(entity);
        IQueryable<ImageMetadataModel> metadataQ = _imageMetadataRepository
            .Query()
            .Where(im => im.ImageStatsId == entity.ImageStatsId);

        ImageMetadataModel? metadata = await Task.FromResult(metadataQ.FirstOrDefault());

        if (metadata == null || !ServiceUtils.Image.UserHasAccess(metadata, entity.UserId))
        {
            throw new InvalidOperationException(
                "You do not have permission to comment on this image."
            );
        }
        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
    }

    public async Task<List<ImageComment>> GetByPredicateAsync(
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
                _imageMetadataRepository.Query().Where(accessPredicate),
                comment => comment.ImageStatsId,
                metadata => metadata.ImageStatsId,
                (comment, image) => new { comment, image }
            )
            .Select(joined => joined.comment)
            .Skip(pagination.Skip)
            .Take(pagination.Size);
        return await Task.FromResult(q.ToList());
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
