using Azure.Storage.Blobs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using Services.Util;
using ImageCollectionModel = Models.ImageCollection;
using ImageModel = Models.Image;

namespace Services.ImageCollection;

public partial class ImageCollectionService : IImageCollectionService
{
    public ImageCollectionService(
        ImageCollectionRepository repository,
        ImageMetadataRepository imageMetadataRepository,
        [FromKeyedServices("PublicImages")] BlobContainerClient publicContainerClient,
        [FromKeyedServices("PrivateImages")] BlobContainerClient privateContainerClient
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageMetadataRepository;
        _publicContainerClient = publicContainerClient;
        _privateContainerClient = privateContainerClient;
    }

    private readonly ImageCollectionRepository _repository;
    private readonly ImageMetadataRepository _imageMetadataRepository;
    private readonly BlobContainerClient _publicContainerClient;
    private readonly BlobContainerClient _privateContainerClient;

    public async Task<ImageCollectionModel> AddAsync(
        ImageCollectionModel entity,
        CancellationToken cancellationToken = default
    )
    {
        CreateImageCollectionValidator().ValidateAndThrow(entity);

        return await _repository.AddAsync(entity, cancellationToken);
    }

    public async Task<List<ImageCollectionModel>> GetByPredicateAsync(
        System.Linq.Expressions.Expression<Func<ImageCollectionModel, bool>> predicate,
        string currentUserId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ImageCollectionModel> query = _repository.Query();
        query = query.Where(predicate);
        query = ServiceUtils.ImageCollection.ApplyUserFilter(query, currentUserId);

        return await query.AsNoTracking().ToListAsync(cancellationToken);
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

        ImageCollectionModel? existingEntity =
            await _repository.GetByIdAsync(cancellationToken, entity.Id).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Image collection does not exist.");

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

        if (entity.CoverImageMetadataId != null)
        {
            ImageMetadata? coverImage = await _imageMetadataRepository
                .GetByIdAsync(cancellationToken, entity.CoverImageMetadataId)
                .ConfigureAwait(false);
            if (coverImage == null || coverImage.ImageCollectionId != entity.Id)
            {
                throw new UnauthorizedAccessException(
                    "Cover image does not belong to this image collection."
                );
            }
        }

        existingEntity.Title = entity.Title;
        existingEntity.Description = entity.Description;
        existingEntity.AccessLevel = entity.AccessLevel;
        existingEntity.CoverImageMetadataId = entity.CoverImageMetadataId;

        _repository.Update(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return existingEntity;
    }
}
