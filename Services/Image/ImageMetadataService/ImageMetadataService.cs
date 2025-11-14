using System.Linq.Expressions;
using Controllers.Image;
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
    private readonly IImageRepository _imageRepository;
    private readonly IImageCollectionRepository _imageCollectionRepository;
    private readonly IImageStatsRepository _imageStatsRepository;
    private readonly IImageTagRepository _imageTagRepository;
    private readonly IImageAllowedUserRepository _imageAllowedUserRepository;
    private readonly ILogger<ImageMetadataService> _logger;

    public ImageMetadataService(
        IImageMetadataRepository repository,
        IImageRepository imageRepository,
        IImageCollectionRepository imageCollectionRepository,
        IImageStatsRepository imageStatsRepository,
        IImageTagRepository imageTagRepository,
        IImageAllowedUserRepository imageAllowedUserRepository,
        ILogger<ImageMetadataService> logger
    )
    {
        _repository = repository;
        _imageRepository = imageRepository;
        _imageCollectionRepository = imageCollectionRepository;
        _imageStatsRepository = imageStatsRepository;
        _imageTagRepository = imageTagRepository;
        _imageAllowedUserRepository = imageAllowedUserRepository;
        _logger = logger;
    }

    public async Task<ImageMetadataModel> AddAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        CreateImageValidator().ValidateAndThrow(entity);

        if (entity.AccessLevel != ImageAccessLevel.AllowedUsers)
            entity.AllowedUsers.Clear();

        entity.Tags = entity.Tags.Distinct().ToList();
        entity.AllowedUsers = entity.AllowedUsers.Distinct().ToList();

        await ValidateImageCollectionAsync(entity, cancellationToken).ConfigureAwait(false);
        entity.Id = 0;
        entity.EditedAt = DateTime.UtcNow;

        ImageStats stats = new ImageStats { ImageMetadata = entity };
        entity.ImageStats = stats;

        ImageMetadataModel result = await _repository
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        return result;
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

        entity.Tags = entity
            .Tags.Distinct()
            .Select(tag => new ImageTag { Tag = tag.Tag, ImageMetadataId = entity.Id })
            .ToList();
        entity.AllowedUsers = entity
            .AllowedUsers.Distinct()
            .Select(u => new ImageAllowedUser { UserId = u.UserId, ImageMetadataId = entity.Id })
            .ToList();

        existingEntity.Title = entity.Title;
        existingEntity.Description = entity.Description;
        existingEntity.Location = entity.Location;
        existingEntity.AccessLevel = entity.AccessLevel;
        existingEntity.ImageCollectionId = entity.ImageCollectionId;
        existingEntity.AllowedUsers = entity.AllowedUsers;
        existingEntity.Tags = entity.Tags;
        existingEntity.EditedAt = DateTime.UtcNow;

        _imageTagRepository.RemoveByPredicate(tag => tag.ImageMetadataId == existingEntity.Id);
        _imageAllowedUserRepository.RemoveByPredicate(user =>
            user.ImageMetadataId == existingEntity.Id
        );
        _repository.Update(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        ImageMetadataModel? existingEntity = await Task.FromResult(
            BuildAccessibleMetadataQuery(entity.UserId)
                .FirstOrDefault(metadata => metadata.Id == entity.Id)
        );

        if (existingEntity is null || existingEntity.UserId != entity.UserId)
            throw new InvalidOperationException(
                "You do not have permission to remove this image metadata."
            );

        _repository.Remove(existingEntity);
        _imageTagRepository.RemoveByPredicate(tag => tag.ImageMetadataId == existingEntity.Id);
        _imageAllowedUserRepository.RemoveByPredicate(user =>
            user.ImageMetadataId == existingEntity.Id
        );

        if (existingEntity.Image != null)
            _imageRepository.Remove(existingEntity.Image);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveByCollectionIdAsync(
        string userId,
        int collectionId,
        CancellationToken cancellationToken = default
    )
    {
        List<ImageMetadataModel> imagesToRemove = await _repository
            .Query()
            .Where(metadata => metadata.ImageCollectionId == collectionId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (ImageMetadataModel metadata in imagesToRemove)
        {
            if (metadata.UserId != userId)
                throw new InvalidOperationException(
                    "You do not have permission to remove this image metadata."
                );
            _repository.Remove(metadata);
            _imageTagRepository.RemoveByPredicate(tag => tag.ImageMetadataId == metadata.Id);
            _imageAllowedUserRepository.RemoveByPredicate(user =>
                user.ImageMetadataId == metadata.Id
            );

            if (metadata.Image != null)
                _imageRepository.Remove(metadata.Image);
        }

        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<ImageMetadataModel>> GetWithPaginationAsync(
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

        return await Task.FromResult(query.ToList());
    }

    public async Task<List<ImageMetadataModel>> GetByPredicateAsync(
        string? userId,
        Expression<Func<ImageMetadataModel, bool>> predicate,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(pagination);

        IQueryable<ImageMetadataModel> query = BuildAccessibleMetadataQuery(userId)
            .Where(predicate)
            .OrderBy(metadata => metadata.Id)
            .Skip(pagination.Skip)
            .Take(pagination.Size);

        return await Task.FromResult(query.ToList());
    }

    public async Task<List<ImageMetadataModel>> GetByFilterAsync(
        string? userId,
        FilterParams filterParams,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(filterParams);
        ArgumentNullException.ThrowIfNull(pagination);

        IQueryable<ImageMetadataModel> query = BuildAccessibleMetadataQuery(userId);
        query = ServiceUtils.Image.ApplyFilter(query, filterParams, _imageStatsRepository);

        query = query.OrderBy(metadata => metadata.Id).Skip(pagination.Skip).Take(pagination.Size);

        List<ImageMetadataModel> result = await Task.FromResult(query.ToList());
        return result;
    }

    public async Task<ImageMetadataModel?> GetByIdAsync(
        string? userId,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        ImageMetadataModel? metadata = await BuildAccessibleMetadataQuery(userId)
            .FirstOrDefaultAsync(metadata => metadata.Id == id, cancellationToken);

        return metadata;
    }

    private IQueryable<ImageMetadataModel> BuildAccessibleMetadataQuery(string? userId) =>
        ServiceUtils
            .Image.ApplyAccessFilter(
                _repository
                    .Query()
                    .Include(metadata => metadata.Image)
                    .Include(metadata => metadata.Tags)
                    .Include(metadata => metadata.AllowedUsers)
                    .Include(metadata => metadata.ImageStats)
                    .Include(metadata => metadata.User)
                    .Include(metadata => metadata.ImageCollection),
                userId
            )
            .AsNoTracking();

    private async Task ValidateImageCollectionAsync(
        ImageMetadataModel entity,
        CancellationToken cancellationToken
    )
    {
        if (!entity.ImageCollectionId.HasValue)
            return;

        ImageCollectionModel? collection = await _imageCollectionRepository
            .GetByIdAsync(cancellationToken, entity.ImageCollectionId.Value)
            .ConfigureAwait(false);

        if (collection == null || collection.UserId != entity.UserId)
            throw new InvalidOperationException("Image collection does not exist.");
    }
}
