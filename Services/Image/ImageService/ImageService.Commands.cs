using Azure.Storage.Blobs.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using ImageCollectionModel = Models.ImageCollection;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task RemoveAsync(int imageId, CancellationToken cancellationToken = default)
    {
        ImageModel? existingImage =
            await _repository.GetByIdAsync(cancellationToken, imageId).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Image does not exist.");

        await _containerClient.DeleteIfExistsAsync(existingImage.BlobName, cancellationToken);

        _repository.Remove(existingImage);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ImageModel> AddAsync(
        ImageContentType contentType,
        Stream imageStream,
        CancellationToken cancellationToken = default
    )
    {
        string blobName = GenerateBlobName();
        try
        {
            await _containerClient.UploadAsync(
                blobName,
                imageStream,
                new BlobHttpHeaders { ContentType = GetMimeType(contentType) },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to upload image to blob storage.", ex);
        }

        string blobUri = _containerClient.GetBlobUri(blobName).ToString();

        ImageModel image = new ImageModel
        {
            BlobName = blobName,
            BlobUri = blobUri,
            ContentType = contentType,
        };

        return await _repository.AddAsync(image, cancellationToken);
    }
}
