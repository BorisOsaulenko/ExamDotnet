using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hw.Tests.TestInfrastructure.AsyncEnumerable;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;
using Services.Storage;
using Services.Util;
using Xunit;

namespace hw.Tests.Services.Image;

/// <summary>
/// Placeholder for ImageService unit tests.
/// </summary>
public sealed class ImageServiceTests
{
    private readonly TestImageFactory _imageFactory = new();
    private readonly IImageRepository _repository = Substitute.For<IImageRepository>();
    private readonly IImageCollectionRepository _imageCollectionRepository =
        Substitute.For<IImageCollectionRepository>();
    private readonly IBlobContainerClient _containerClient = Substitute.For<IBlobContainerClient>();

    private readonly Models.Image testImage;

    private readonly List<Models.Image> _imageStore = new();
    private readonly List<Models.Image> accessibleImages = new();
    private readonly List<Models.Image> notAccessibleImages = new();

    private readonly ImageService service;

    public ImageServiceTests()
    {
        testImage = _imageFactory.Create();
        _imageStore.Add(testImage);

        service = new ImageService(_repository, _containerClient, _imageCollectionRepository);

        _containerClient
            .UploadAsync(
                Arg.Any<string>(),
                Arg.Any<System.IO.Stream>(),
                Arg.Any<Azure.Storage.Blobs.Models.BlobHttpHeaders>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);
        _containerClient.GetBlobUri(Arg.Any<string>()).Returns(new Uri(testImage.BlobUri));
        _containerClient.Name.Returns("test-container");
        _containerClient.CanGenerateSasUri.Returns(true);
        _containerClient
            .GenerateSasUri(Arg.Any<string>(), Arg.Any<Azure.Storage.Sas.BlobSasBuilder>())
            .Returns(new Uri(testImage.BlobUri));
        _repository
            .AddAsync(Arg.Any<Models.Image>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Models.Image>());

        _imageStore.Add(testImage);

        Models.Image storeImage1 = _imageFactory.Create(im =>
        {
            im.UserId = "user2";
            im.Title = "Image2";
        });
        Models.Image storeImage2 = _imageFactory.Create(im =>
        {
            im.UserId = "user3";
            im.AccessLevel = ImageAccessLevel.Private;
            im.Title = "Image3";
        });
        Models.Image storeImage3 = _imageFactory.Create(im =>
        {
            im.UserId = "user4";
            im.AccessLevel = ImageAccessLevel.AllowedUsers;
            im.AllowedUsers = new[]
            {
                new ImageAllowedUser { UserId = "different", ImageId = im.Id },
            }.ToList();
            im.Description = "Image4 Description";
        });
        Models.Image storeImage4 = _imageFactory.Create(im =>
        {
            im.UserId = "user5";
            im.AccessLevel = ImageAccessLevel.AllowedUsers;
            im.AllowedUsers = new[]
            {
                new ImageAllowedUser { UserId = testImage.UserId, ImageId = im.Id },
            }.ToList();
            im.Description = "Image5";
        });
        Models.Image storeImage5 = _imageFactory.Create(im =>
        {
            im.UserId = testImage.UserId;
            im.AccessLevel = ImageAccessLevel.Private;
        });

        _imageStore.AddRange([storeImage1, storeImage2, storeImage3, storeImage4, storeImage5]);

        accessibleImages.AddRange(
            _imageStore.Where(im => ServiceUtils.Image.UserHasAccess(im, testImage.UserId))
        );
        notAccessibleImages.AddRange(
            _imageStore.Where(im => !ServiceUtils.Image.UserHasAccess(im, testImage.UserId))
        );

        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), Arg.Any<int>())
            .Returns(Task.FromResult<Models.Image?>(testImage));

        ArrangeRepositoryQuery(_imageStore.ToArray());
    }

    [Fact]
    public async Task ShouldCreateImage()
    {
        Models.Image created = await service.AddAsync(
            testImage,
            Stream.Null,
            CancellationToken.None
        );
        Assert.Equal(testImage.UserId, created.UserId);
        Assert.Equal(testImage.Title, created.Title);
        Assert.Equal(testImage.Description, created.Description);
        Assert.Equal(testImage.AccessLevel, created.AccessLevel);
        Assert.Equal(testImage.ContentType, created.ContentType);

        // Expect blob upload to have been called
        await _containerClient
            .Received(1)
            .UploadAsync(
                Arg.Any<string>(),
                Arg.Any<System.IO.Stream>(),
                Arg.Any<Azure.Storage.Blobs.Models.BlobHttpHeaders>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ShouldThrowIfRequiredFieldsMissing()
    {
        Models.Image missing1 = _imageFactory.Create(template => template.UserId = string.Empty);
        Models.Image missing2 = _imageFactory.Create(template => template.Title = string.Empty);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => service.AddAsync(missing1, Stream.Null, CancellationToken.None)
        );

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => service.AddAsync(missing2, Stream.Null, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldBeFineWithOptionalFieldsMissing()
    {
        Models.Image withoutDescription = _imageFactory.Create(template =>
        {
            template.Description = null;
        });

        Models.Image created = await service.AddAsync(
            withoutDescription,
            Stream.Null,
            CancellationToken.None
        );

        Assert.Equal(testImage.UserId, created.UserId);
        Assert.Equal(testImage.Title, created.Title);
        Assert.Null(created.Description);
    }

    [Fact]
    public async Task ShouldThrowIfImageCollectionDoesNotExist()
    {
        Models.Image withCollection = _imageFactory.Create(template => template.ImageCollectionId = 999);

        _imageCollectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 999)
            .Returns(Task.FromResult<Models.ImageCollection?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddAsync(withCollection, Stream.Null, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldThrowIfNotOwnerOfImageCollection()
    {
        Models.Image withCollection = _imageFactory.Create(template => template.ImageCollectionId = 999);

        _imageCollectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 999)
            .Returns(
                Task.FromResult<Models.ImageCollection?>(
                    new Models.ImageCollection
                    {
                        Id = 999,
                        UserId = "differentUser",
                        Title = "Some Collection",
                    }
                )
            );

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddAsync(withCollection, Stream.Null, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldNotRetrievePrivateImages()
    {
        List<Models.Image> images = await service.GetWithPaginationAsync(
            testImage.UserId,
            new PaginationParams { Skip = 0, Size = 10 },
            CancellationToken.None
        );

        Assert.DoesNotContain(images, notAccessibleImages.Contains);
        Assert.Contains(images, accessibleImages.Contains);

        images = await service.GetByPredicateAsync(
            testImage.UserId,
            img => true,
            new PaginationParams { Skip = 0, Size = 10 },
            CancellationToken.None
        );

        Assert.DoesNotContain(images, notAccessibleImages.Contains);
        Assert.Contains(images, accessibleImages.Contains);
    }

    [Fact]
    public async Task ShouldFilterByPredicate()
    {
        List<Models.Image> images = await service.GetByPredicateAsync(
            testImage.UserId,
            img => img.Description != null,
            new PaginationParams { Skip = 0, Size = 10 },
            CancellationToken.None
        );

        Assert.All(
            images,
            img =>
            {
                Assert.NotNull(img.Description);
                Assert.Contains(img, accessibleImages);
            }
        );
    }

    [Fact]
    public async Task ShouldPaginateResults()
    {
        List<Models.Image> images = await service.GetWithPaginationAsync(
            testImage.UserId,
            new PaginationParams { Skip = 1, Size = 1 },
            CancellationToken.None
        );

        List<Models.Image> expected = accessibleImages.Skip(1).Take(1).ToList();
        Assert.Equal(expected.Count, images.Count);
        Assert.Equal(expected, images);
    }

    [Fact]
    public async Task ShouldBeAddingSAS()
    {
        List<Models.Image> images = await service.GetWithPaginationAsync(
            testImage.UserId,
            new PaginationParams { Skip = 0, Size = 10 },
            CancellationToken.None
        );

        _containerClient
            .Received(accessibleImages.Count)
            .GenerateSasUri(Arg.Any<string>(), Arg.Any<Azure.Storage.Sas.BlobSasBuilder>());

        List<Models.Image> imagesByPredicate = await service.GetByPredicateAsync(
            testImage.UserId,
            img => true,
            new PaginationParams { Skip = 0, Size = 10 },
            CancellationToken.None
        );

        _containerClient
            .Received(accessibleImages.Count * 2)
            .GenerateSasUri(Arg.Any<string>(), Arg.Any<Azure.Storage.Sas.BlobSasBuilder>());
    }

    [Fact]
    public async Task ShouldUpdateImage()
    {
        Models.Image toUpdate = _imageFactory.Create(im => im.Title = "Updated Title");

        await service.UpdateAsync(toUpdate, CancellationToken.None);

        _repository.Received(1).Update(Arg.Is<Models.Image>(im => im.Title == "Updated Title"));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowOnUnauthorizedUpdate()
    {
        Models.Image toUpdate = _imageFactory.Create(im =>
        {
            im.UserId = "differentUser";
            im.Title = "Updated Title";
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateAsync(toUpdate, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldThrowIfImageNotFoundOnUpdate()
    {
        Models.Image toUpdate = _imageFactory.Create(im => im.Id = 9999);

        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 9999)
            .Returns(Task.FromResult<Models.Image?>(null));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(toUpdate, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldThrowIfRequiredFieldsMissingOnUpdate()
    {
        Models.Image missingTitle = _imageFactory.Create(im =>
        {
            im.Title = string.Empty;
        });

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => service.UpdateAsync(missingTitle, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldThrowIfImageCollectionDoesNotExistOnUpdate()
    {
        Models.Image withCollection = _imageFactory.Create(im =>
        {
            im.ImageCollectionId = 999;
        });

        _imageCollectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 999)
            .Returns(Task.FromResult<Models.ImageCollection?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(withCollection, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldDeleteImage()
    {
        await service.RemoveAsync(testImage, CancellationToken.None);

        await _containerClient
            .Received(1)
            .DeleteIfExistsAsync(testImage.BlobName, Arg.Any<CancellationToken>());
        _repository.Received(1).Remove(Arg.Is<Models.Image>(im => im.Id == testImage.Id));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldNotThrowIfNotOwnerOnDelete()
    {
        Models.Image notOwner = _imageFactory.Create(im => im.UserId = "differentUser");

        await service.RemoveAsync(notOwner, CancellationToken.None);

        await _containerClient
            .Received(0)
            .DeleteIfExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _repository.Received(0).Remove(Arg.Is<Models.Image>(im => true));
        await _repository.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldNotThrowIfAllowedUserOnDelete()
    {
        Models.Image allowedUser = _imageFactory.Create(im =>
        {
            im.UserId = "user4";
            im.AccessLevel = ImageAccessLevel.AllowedUsers;
            im.AllowedUsers = new[]
            {
                new ImageAllowedUser { UserId = testImage.UserId, ImageId = im.Id },
            }.ToList();
        });

        await service.RemoveAsync(allowedUser, CancellationToken.None);
        await _containerClient
            .Received(0)
            .DeleteIfExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _repository.Received(0).Remove(Arg.Is<Models.Image>(im => true));
        await _repository.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldNotThrowIfImageNotFoundOnDelete()
    {
        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), testImage.Id)
            .Returns(Task.FromResult<Models.Image?>(null));
        await service.RemoveAsync(testImage, CancellationToken.None);

        await _containerClient
            .Received(0)
            .DeleteIfExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _repository.Received(0).Remove(Arg.Is<Models.Image>(im => true));
        await _repository.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void ArrangeRepositoryQuery(params Models.Image[] images)
    {
        _repository.Query().Returns(new TestAsyncEnumerable<Models.Image>(images.AsQueryable()));
    }
}
