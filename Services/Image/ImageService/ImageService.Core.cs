using System.IO;
using Azure.Storage.Sas;
using Models;
using Repositories;
using Services.Azure;
using Services.Storage;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService : IImageService
{
    private readonly IImageRepository _repository;
    private readonly IBlobContainerClient _containerClient;
    private readonly IImageCollectionRepository _imageCollectionRepository;
    private readonly IImageMetadataRepository _imageMetadataRepository;
    private readonly IComputerVision _computerVision;

    public ImageService(
        IImageRepository repository,
        [FromKeyedServices("PublicImages")] IBlobContainerClient containerClient,
        IImageCollectionRepository imageCollectionRepository,
        IImageMetadataRepository imageMetadataRepository,
        IComputerVision computerVision
    )
    {
        _repository = repository;
        _containerClient = containerClient;
        _imageCollectionRepository = imageCollectionRepository;
        _imageMetadataRepository = imageMetadataRepository;
        _computerVision = computerVision;
    }

    public static string GetMimeType(ImageContentType contentType) =>
        contentType switch
        {
            ImageContentType.Jpeg => "image/jpeg",
            ImageContentType.Png => "image/png",
            ImageContentType.Gif => "image/gif",
            ImageContentType.Bmp => "image/bmp",
            ImageContentType.WebP => "image/webp",
            ImageContentType.Avif => "image/avif",
            _ => "application/octet-stream",
        };

    public static ImageContentType? ParseFromFileName(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".jpeg" or ".jpg" => ImageContentType.Jpeg,
            ".png" => ImageContentType.Png,
            ".gif" => ImageContentType.Gif,
            ".bmp" => ImageContentType.Bmp,
            ".webp" => ImageContentType.WebP,
            ".avif" => ImageContentType.Avif,
            _ => null,
        };
    }

    private static string GenerateBlobName() => $"{Guid.NewGuid():N}";

    private static readonly TimeSpan DefaultSasLifetime = TimeSpan.FromMinutes(5);

    public ImageModel AttachSASInfo(ImageModel image)
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

    private static async Task<Stream> PrepareSeekableStreamAsync(
        Stream source,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.CanSeek)
        {
            source.Position = 0;
            return source;
        }

        var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;
        return buffer;
    }
}
