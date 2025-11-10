using Azure.Storage.Sas;
using Models;
using Repositories;
using Services.Storage;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService : IImageService
{
    private readonly IImageRepository _repository;
    private readonly IBlobContainerClient _containerClient;
    private readonly IImageCollectionRepository _imageCollectionRepository;
    private readonly IImageMetadataRepository _imageMetadataRepository;

    public ImageService(
        IImageRepository repository,
        [FromKeyedServices("PublicImages")] IBlobContainerClient containerClient,
        IImageCollectionRepository imageCollectionRepository,
        IImageMetadataRepository imageMetadataRepository
    )
    {
        _repository = repository;
        _containerClient = containerClient;
        _imageCollectionRepository = imageCollectionRepository;
        _imageMetadataRepository = imageMetadataRepository;
    }

    public static string GetMimeType(ImageContentType contentType) =>
        contentType switch
        {
            ImageContentType.Jpeg => "image/jpeg",
            ImageContentType.Png => "image/png",
            ImageContentType.Gif => "image/gif",
            ImageContentType.Bmp => "image/bmp",
            ImageContentType.WebP => "image/webp",
            _ => "application/octet-stream",
        };

    private static string GenerateBlobName() => $"{Guid.NewGuid():N}";

    private static readonly TimeSpan DefaultSasLifetime = TimeSpan.FromMinutes(5);

    private ImageModel AttachSASInfo(ImageModel image, string userId)
    {
        if (image == null || string.IsNullOrEmpty(image.BlobName))
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (!_containerClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "BlobContainerClient is not authorized to generate SAS URIs."
            );
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = image.BlobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.Add(DefaultSasLifetime),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        Uri sasUri = _containerClient.GenerateSasUri(image.BlobName, sasBuilder);
        image.BlobUri = sasUri.ToString();
        return image;
    }
}
