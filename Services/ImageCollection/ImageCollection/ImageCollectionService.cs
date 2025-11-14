using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using Services.Storage;
using Services.Util;
using ImageCollectionModel = Models.ImageCollection;

namespace Services.ImageCollection;

public partial class ImageCollectionService : IImageCollectionService
{
    public ImageCollectionService(
        IImageCollectionRepository repository,
        IImageMetadataRepository imageMetadataRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageMetadataRepository;
    }

    private readonly IImageCollectionRepository _repository;
    private readonly IImageMetadataRepository _imageMetadataRepository;

    public async Task<ImageCollectionModel> AddAsync(
        ImageCollectionModel entity,
        CancellationToken cancellationToken = default
    )
    {
        CreateImageCollectionValidator().ValidateAndThrow(entity);

        bool isCoverImageValid = await CheckCoverImageAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        if (!isCoverImageValid)
            throw new UnauthorizedAccessException(
                "Cover image does not belong to this image collection."
            );

        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
    }

    public async Task<List<ImageCollectionModel>> GetByPredicateAsync(
        Expression<Func<ImageCollectionModel, bool>> predicate,
        string currentUserId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ImageCollectionModel> query = BuildCollectionQuery().Where(predicate);
        query = ServiceUtils.ImageCollection.ApplyUserFilter(query, currentUserId);

        List<ImageCollectionModel> result = query.ToList();
        return await Task.FromResult(result);
    }

    public async Task UpdateAsync(
        ImageCollectionModel entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageCollectionModel updatedEntity = await VerifyImageCollection(entity, cancellationToken)
            .ConfigureAwait(false);

        _repository.Update(updatedEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        ImageCollectionModel entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        ImageCollectionModel? existingEntity = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingEntity == null || existingEntity.UserId != entity.UserId)
            throw new InvalidOperationException("Image collection does not exist.");

        _repository.Remove(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<ImageCollectionModel> VerifyImageCollection(
        ImageCollectionModel entity,
        CancellationToken cancellationToken = default
    )
    {
        CreateImageCollectionValidator().ValidateAndThrow(entity);

        ImageCollectionModel? existingEntity = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingEntity == null || existingEntity.UserId != entity.UserId)
        {
            throw new InvalidOperationException("Image collection does not exist.");
        }

        bool isCoverImageValid = await CheckCoverImageAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        if (!isCoverImageValid)
        {
            throw new UnauthorizedAccessException(
                "Cover image does not belong to this image collection."
            );
        }

        existingEntity.Title = entity.Title;
        existingEntity.Description = entity.Description;
        existingEntity.AccessLevel = entity.AccessLevel;
        existingEntity.CoverImageMetadataId = entity.CoverImageMetadataId;

        return existingEntity;
    }

    private async Task<bool> CheckCoverImageAsync(
        ImageCollectionModel imageCollection,
        CancellationToken cancellationToken = default
    )
    {
        if (imageCollection.CoverImageMetadataId == null)
            return true;

        ImageMetadata? coverImage = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, imageCollection.CoverImageMetadataId.Value)
            .ConfigureAwait(false);

        return coverImage != null && coverImage.ImageCollectionId == imageCollection.Id;
    }

    private IQueryable<ImageCollectionModel> BuildCollectionQuery() =>
        _repository
            .Query()
            .Include(collection => collection.Images)
                .ThenInclude(image => image.Image)
            .Include(collection => collection.Images)
                .ThenInclude(image => image.Tags)
            .Include(collection => collection.Images)
                .ThenInclude(image => image.AllowedUsers)
            .Include(collection => collection.Images)
                .ThenInclude(image => image.ImageStats)
            .Include(collection => collection.AllowedUsers)
            .Include(collection => collection.CoverImageMetadata)
            .Include(collection => collection.Subscribers)
            .AsNoTracking();
}
