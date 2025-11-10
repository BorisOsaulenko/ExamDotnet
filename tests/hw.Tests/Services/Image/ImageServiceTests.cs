using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;
using Services.Storage;
using Xunit;
using ImageModel = Models.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageServiceTests
{
    private readonly ImageService _imageService;

    private readonly IImageRepository _imageRepositoryMock;
    private readonly IImageMetadataRepository _imageMetadataRepositoryMock;
    private readonly IImageCollectionRepository _imageCollectionRepositoryMock;
    private readonly IBlobContainerClient _blobContainerClientMock;

    public ImageServiceTests()
    {
        _imageRepositoryMock = Substitute.For<IImageRepository>();
        _imageMetadataRepositoryMock = Substitute.For<IImageMetadataRepository>();
        _imageCollectionRepositoryMock = Substitute.For<IImageCollectionRepository>();
        _blobContainerClientMock = Substitute.For<IBlobContainerClient>();

        _imageService = new ImageService(
            _imageRepositoryMock,
            _blobContainerClientMock,
            _imageCollectionRepositoryMock,
            _imageMetadataRepositoryMock
        );
    }

    [Fact]
    public async Task ShouldUploadImageAsync()
    {
        ImageModel image = GetTestImage();

        _blobContainerClientMock
            .UploadAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<BlobHttpHeaders>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult);

        _blobContainerClientMock
            .GetBlobUri(Arg.Any<string>())
            .Returns(callInfo => new Uri($"https://cdn.local/{callInfo.Arg<string>()}"));

        _imageRepositoryMock
            .AddAsync(Arg.Any<ImageModel>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ImageModel>()));

        // Act
        ImageModel result = await _imageService.AddAsync(
            image.ContentType,
            Stream.Null,
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.BlobName);
        Assert.NotNull(result.BlobUri);
        Assert.Equal(image.ContentType, result.ContentType);

        await _blobContainerClientMock
            .Received(1)
            .UploadAsync(
                Arg.Is<string>(name => name == result.BlobName),
                Arg.Any<Stream>(),
                Arg.Is<BlobHttpHeaders>(headers =>
                    headers.ContentType == ImageService.GetMimeType(result.ContentType)
                ),
                Arg.Any<CancellationToken>()
            );

        await _imageRepositoryMock
            .Received(1)
            .AddAsync(Arg.Any<ImageModel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldDeleteImageAsync()
    {
        ImageModel image = GetTestImage(customize: img => img.Id = 42);
        _imageRepositoryMock
            .GetByIdAsync(Arg.Any<CancellationToken>(), image.Id)
            .Returns(Task.FromResult<ImageModel?>(image));
        _blobContainerClientMock
            .DeleteIfExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        // Act
        await _imageService.RemoveAsync(image.Id, CancellationToken.None);
        // Assert
        await _blobContainerClientMock
            .Received(1)
            .DeleteIfExistsAsync(
                Arg.Is<string>(name => name == image.BlobName),
                Arg.Any<CancellationToken>()
            );

        _imageRepositoryMock.Received(1).Remove(Arg.Is<ImageModel>(img => img.Id == image.Id));
        await _imageRepositoryMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowWhenDeletingNonExistingImageAsync()
    {
        _imageRepositoryMock
            .GetByIdAsync(Arg.Any<CancellationToken>(), Arg.Any<int>())
            .Returns(Task.FromResult<ImageModel?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _imageService.RemoveAsync(999, CancellationToken.None);
        });

        await _blobContainerClientMock
            .DidNotReceive()
            .DeleteIfExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        _imageRepositoryMock.DidNotReceive().Remove(Arg.Any<ImageModel>());
        await _imageRepositoryMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    public static ImageModel GetTestImage(Action<ImageModel> customize = null)
    {
        var image = new ImageModel { Id = 1, ContentType = ImageContentType.Jpeg };
        customize?.Invoke(image);
        return image;
    }
}
