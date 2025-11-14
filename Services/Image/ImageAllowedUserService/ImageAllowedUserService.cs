using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.Image;

public class ImageAllowedUserService : IImageAllowedUserService
{
    private readonly IImageAllowedUserRepository _repository;
    private readonly IImageMetadataRepository _imageMetadataRepository;

    public ImageAllowedUserService(
        IImageAllowedUserRepository repository,
        IImageMetadataRepository imageRepository
    )
    {
        _repository = repository;
        _imageMetadataRepository = imageRepository;
    }

    public async Task<ImageAllowedUser> AddAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageMetadataModel? image = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, entity.ImageMetadataId)
            .ConfigureAwait(false);

        if (image == null || image.UserId != userId)
            throw new InvalidOperationException("Image does not exist.");

        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
    }

    public async Task<List<ImageAllowedUser>> GetByImageMetadataIdAsync(
        string userId,
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        ImageMetadataModel? image = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, imageMetadataId)
            .ConfigureAwait(false);

        if (image == null || image.UserId != userId)
            throw new InvalidOperationException("Image does not exist.");

        return await _repository.GetByPredicateAsync(
            au => au.ImageMetadataId == imageMetadataId,
            cancellationToken
        );
    }

    public async Task<List<ImageAllowedUser>> ReplaceAsync(
        string userId,
        int imageMetadataId,
        List<ImageAllowedUser> newAllowedUsers,
        CancellationToken cancellationToken = default
    )
    {
        ImageMetadataModel? image = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, imageMetadataId)
            .ConfigureAwait(false);

        if (image == null || image.UserId != userId)
            throw new InvalidOperationException("Image does not exist.");

        _repository.RemoveByPredicate(au => au.ImageMetadataId == imageMetadataId);

        List<ImageAllowedUser> sanitizedUsers = (newAllowedUsers ?? new List<ImageAllowedUser>())
            .Where(u => !string.IsNullOrWhiteSpace(u.UserId))
            .Select(u => u.UserId.Trim())
            .Distinct(StringComparer.Ordinal)
            .Select(userIdValue => new ImageAllowedUser
            {
                ImageMetadataId = imageMetadataId,
                UserId = userIdValue,
            })
            .ToList();

        if (sanitizedUsers.Count > 0)
            await _repository
                .AddRangeAsync(sanitizedUsers, cancellationToken)
                .ConfigureAwait(false);

        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return sanitizedUsers;
    }

    public async Task RemoveAsync(
        string userId,
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    )
    {
        ImageAllowedUser? existingEntity = await Task.FromResult(
            _repository
                .Query()
                .AsNoTracking()
                .Include(au => au.ImageMetadata)
                .FirstOrDefault(au =>
                    au.ImageMetadataId == entity.ImageMetadataId && au.UserId == entity.UserId
                )
        );
        if (existingEntity == null)
            throw new InvalidOperationException("Entity does not exist.");

        ImageMetadataModel? image = existingEntity.ImageMetadata;

        if (image == null || image.UserId != userId)
            throw new InvalidOperationException("Image does not exist.");

        _repository.Remove(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAllForImageAsync(
        string userId,
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        ImageMetadataModel? image = await _imageMetadataRepository
            .GetByIdAsync(cancellationToken, imageMetadataId)
            .ConfigureAwait(false);

        if (image == null || image.UserId != userId)
            throw new InvalidOperationException("Image does not exist.");

        await RemoveAllForImageAsync(imageMetadataId, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAllForImageAsync(
        int imageMetadataId,
        CancellationToken cancellationToken = default
    )
    {
        List<ImageAllowedUser> allowedUsers = await _repository
            .GetByPredicateAsync(au => au.ImageMetadataId == imageMetadataId, cancellationToken)
            .ConfigureAwait(false);

        if (allowedUsers.Count == 0)
            return;

        _repository.RemoveRange(allowedUsers);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
