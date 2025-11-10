using Azure.Storage.Blobs.Models;
using FluentValidation;
using ImageCollectionModel = Models.ImageCollection;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task<ImageModel> AddAsync(
        ImageModel image,
        Stream imageStream,
        CancellationToken cancellationToken = default
    )
    {
        image.Id = 0;
        CreateImageValidator().ValidateAndThrow(image);

        if (image.ImageCollectionId != null)
        {
            ImageCollectionModel? imageCollection = await _imageCollectionRepository
                .GetByIdAsync(cancellationToken, image.ImageCollectionId)
                .ConfigureAwait(false);

            if (imageCollection == null || imageCollection.UserId != image.UserId)
            {
                throw new InvalidOperationException("Image collection does not exist.");
            }
        }

        string blobName = GenerateBlobName(image.UserId);
        try
        {
            await _containerClient.UploadAsync(
                blobName,
                imageStream,
                new BlobHttpHeaders { ContentType = GetMimeType(image.ContentType) },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to upload image to blob storage.", ex);
        }

        image.BlobName = blobName;
        image.BlobUri = _containerClient.GetBlobUri(blobName).ToString();

        return await _repository.AddAsync(image, cancellationToken);
    }
}
