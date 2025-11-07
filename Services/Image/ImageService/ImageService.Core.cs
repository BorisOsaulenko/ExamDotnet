using Azure.Storage.Blobs;
using Models;
using Repositories;
using Services.Identity;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService : IImageService
{
    private readonly ImageRepository _repository;
    private readonly BlobContainerClient _publicContainerClient;
    private readonly BlobContainerClient _privateContainerClient;
    private readonly ICurrentUserService _currentUserService;

    public ImageService(
        ImageRepository repository,
        [FromKeyedServices("PublicImages")] BlobContainerClient publicContainerClient,
        [FromKeyedServices("PrivateImages")] BlobContainerClient privateContainerClient,
        ICurrentUserService currentUserService
    )
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _publicContainerClient =
            publicContainerClient ?? throw new ArgumentNullException(nameof(publicContainerClient));
        _privateContainerClient =
            privateContainerClient
            ?? throw new ArgumentNullException(nameof(privateContainerClient));
        _currentUserService =
            currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    private BlobContainerClient GetContainerClientForImage(ImageModel image) =>
        image.AccessLevel == ImageAccessLevel.Public
            ? _publicContainerClient
            : _privateContainerClient;

    private BlobContainerClient GetContainerClientForAccessLevel(ImageAccessLevel accessLevel) =>
        accessLevel == ImageAccessLevel.Public ? _publicContainerClient : _privateContainerClient;

    private static string GenerateBlobName(string userId) => $"{userId}/{Guid.NewGuid():N}";

    private static string GetMimeType(ImageContentType contentType) =>
        ImageContentTypeExtensions.ToMimeType(contentType);
}
