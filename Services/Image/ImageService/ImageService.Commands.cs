using Azure.Storage.Blobs.Models;
using Models;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task RemoveAsync(int imageId, CancellationToken cancellationToken = default)
    {
        ImageModel? existingImage = await _repository
            .GetByIdAsync(cancellationToken, imageId)
            .ConfigureAwait(false);

        if (existingImage == null || existingImage.BlobName == null)
            throw new InvalidOperationException($"Image with ID {imageId} not found.");

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
        Stream moderatedStream = await PrepareSeekableStreamAsync(imageStream, cancellationToken)
            .ConfigureAwait(false);

        await _computerVision
            .EnsureSafeContentAsync(moderatedStream, cancellationToken)
            .ConfigureAwait(false);

        if (moderatedStream.CanSeek)
            moderatedStream.Position = 0;


        string blobName = GenerateBlobName();
        try
        {
            await _containerClient.UploadAsync(
                blobName,
                moderatedStream,
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

        return await Task.FromResult(await _repository.AddAsync(image, cancellationToken));
    }
}
