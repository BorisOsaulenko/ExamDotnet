using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Services.Util;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task Update(ImageModel entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        CreateImageValidator().ValidateAndThrow(entity);

        var currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);

        var existingImage =
            await _repository
                .Query()
                .Where(image => image.Id == entity.Id && image.UserId == currentUserId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Image not found.");

        if (existingImage.AccessLevel != entity.AccessLevel)
        {
            await MoveBlobAsync(existingImage, entity.AccessLevel, cancellationToken);
            entity.ContainerName = existingImage.ContainerName;
            entity.BlobUri = existingImage.BlobUri;
            entity.BlobName = existingImage.BlobName;
        }

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task Remove(ImageModel entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);

        var existingImage = await _repository
            .Query()
            .Where(image => image.Id == entity.Id && image.UserId == currentUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingImage is null)
        {
            return;
        }

        var container = GetContainerClientForImage(existingImage);
        var blobClient = container.GetBlobClient(existingImage.BlobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        _repository.Remove(existingImage);
    }

    public Task<bool> ExistsAsync(
        Expression<Func<ImageModel, bool>> predicate,
        CancellationToken cancellationToken = default
    ) => _repository.ExistsAsync(predicate, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _repository.SaveChangesAsync(cancellationToken);

    private async Task MoveBlobAsync(
        ImageModel sourceImage,
        ImageAccessLevel targetAccessLevel,
        CancellationToken cancellationToken = default
    )
    {
        var sourceContainer = GetContainerClientForAccessLevel(sourceImage.AccessLevel);
        var destinationContainer = GetContainerClientForAccessLevel(targetAccessLevel);

        if (sourceContainer.Name == destinationContainer.Name)
        {
            return;
        }

        var sourceBlob = sourceContainer.GetBlobClient(sourceImage.BlobName);
        var destinationBlob = destinationContainer.GetBlobClient(sourceImage.BlobName);

        destinationBlob.SyncCopyFromUri(sourceBlob.Uri, cancellationToken: cancellationToken);
        await sourceBlob.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        sourceImage.ContainerName = destinationContainer.Name;
        sourceImage.BlobUri = destinationBlob.Uri.ToString();
        sourceImage.AccessLevel = targetAccessLevel;
    }
}
