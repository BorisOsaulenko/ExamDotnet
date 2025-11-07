using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentValidation;
using Models;
using Services.Util;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task<ImageModel> AddAsync(
        ImageModel image,
        Stream imageStream,
        ImageContentType contentType,
        CancellationToken cancellationToken = default
    )
    {
        var currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);
        image.UserId = currentUserId;
        image.Id = 0;

        CreateImageValidator().ValidateAndThrow(image);

        var container = GetContainerClientForImage(image);
        var blobName = GenerateBlobName(currentUserId);
        var blobClient = container.GetBlobClient(blobName);

        try
        {
            await blobClient.UploadAsync(
                imageStream,
                new BlobHttpHeaders { ContentType = GetMimeType(contentType) },
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to upload image to blob storage.", ex);
        }

        image.BlobName = blobName;
        image.ContainerName = container.Name;
        image.BlobUri = blobClient.Uri.ToString();

        return await _repository.AddAsync(image, cancellationToken);
    }
}
