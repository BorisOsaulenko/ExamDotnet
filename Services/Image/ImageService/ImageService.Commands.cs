using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ImageCollectionModel = Models.ImageCollection;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task UpdateAsync(ImageModel entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        CreateImageValidator().ValidateAndThrow(entity);

        ImageModel existingImage =
            await _repository.GetByIdAsync(cancellationToken, entity.Id)
            ?? throw new KeyNotFoundException("Image not found.");

        if (existingImage.UserId != entity.UserId)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to update this image."
            );
        }

        if (entity.ImageCollectionId != null)
        {
            ImageCollectionModel? imageCollection = await _imageCollectionRepository
                .GetByIdAsync(cancellationToken, entity.ImageCollectionId)
                .ConfigureAwait(false);

            if (imageCollection == null || imageCollection.UserId != entity.UserId)
            {
                throw new InvalidOperationException("Image collection does not exist.");
            }
        }

        existingImage.AccessLevel = entity.AccessLevel;
        existingImage.Title = entity.Title;
        existingImage.Description = entity.Description;
        existingImage.ImageCollectionId = entity.ImageCollectionId;
        existingImage.Location = entity.Location;

        _repository.Update(existingImage);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(ImageModel entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        ImageModel? existingImage = await _repository
            .GetByIdAsync(cancellationToken, entity.Id)
            .ConfigureAwait(false);

        if (existingImage is null || existingImage.UserId != entity.UserId)
        {
            return;
        }

        await _containerClient.DeleteIfExistsAsync(existingImage.BlobName, cancellationToken);

        _repository.Remove(existingImage);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
