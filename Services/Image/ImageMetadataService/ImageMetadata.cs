using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using Services.Util;
using ImageCollectionModel = Models.ImageCollection;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.Image;

public partial class ImageMetadataService : IImageMetadataService
{
    private readonly IImageMetadataRepository _repository;
    private readonly IImageCollectionRepository _imageCollectionRepository;

    public ImageMetadataService(
        IImageMetadataRepository repository,
        IImageCollectionRepository imageCollectionRepository
    )
    {
        _repository = repository;
        _imageCollectionRepository = imageCollectionRepository;
    }

    public async Task<ImageMetadataModel> AddAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        CreateImageValidator().ValidateAndThrow(entity);

        await ValidateImageCollectionAsync(entity, cancellationToken).ConfigureAwait(false);
        entity.Id = 0;

        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        CreateImageValidator().ValidateAndThrow(entity);

        ImageMetadataModel? existingEntity = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingEntity == null || existingEntity.UserId != entity.UserId)
            throw new UnauthorizedAccessException(
                "You do not have permission to update this image metadata."
            );

        await ValidateImageCollectionAsync(entity, cancellationToken).ConfigureAwait(false);

        existingEntity.Title = entity.Title;
        existingEntity.Description = entity.Description;
        existingEntity.Location = entity.Location;
        existingEntity.AccessLevel = entity.AccessLevel;
        existingEntity.ImageCollectionId = entity.ImageCollectionId;

        _repository.Update(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        ImageMetadataModel? existingEntity = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingEntity is null || existingEntity.UserId != entity.UserId)
            throw new InvalidOperationException(
                "You do not have permission to remove this image metadata."
            );

        _repository.Remove(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<List<ImageMetadataModel>> GetWithPaginationAsync(
        string userId,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(pagination);

        IQueryable<ImageMetadataModel> query = BuildAccessibleMetadataQuery(userId)
            .OrderBy(metadata => metadata.Id)
            .Skip(pagination.Skip)
            .Take(pagination.Size);

        return query.ToListAsync(cancellationToken);
    }

    public Task<List<ImageMetadataModel>> GetByPredicateAsync(
        string userId,
        Expression<Func<ImageMetadataModel, bool>> predicate,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(pagination);

        IQueryable<ImageMetadataModel> query = BuildAccessibleMetadataQuery(userId)
            .Where(predicate)
            .OrderBy(metadata => metadata.Id)
            .Skip(pagination.Skip)
            .Take(pagination.Size);

        return query.ToListAsync(cancellationToken);
    }

    public Task<ImageMetadataModel?> GetByIdAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        return BuildAccessibleMetadataQuery(userId)
            .FirstOrDefaultAsync(metadata => metadata.Id == id, cancellationToken);
    }

    private IQueryable<ImageMetadataModel> BuildAccessibleMetadataQuery(string userId) =>
        ServiceUtils
            .Image.ApplyAccessFilter(
                _repository.Query().Include(metadata => metadata.Image),
                userId
            )
            .AsNoTracking();

    private async Task ValidateImageCollectionAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken
    )
    {
        if (!entity.ImageCollectionId.HasValue)
        {
            return;
        }

        ImageCollectionModel? collection = await _imageCollectionRepository
            .GetByIdAsync(cancellationToken, entity.ImageCollectionId.Value)
            .ConfigureAwait(false);

        if (collection == null || collection.UserId != entity.UserId)
            throw new InvalidOperationException("Image collection does not exist.");
    }
}
