using Microsoft.Extensions.Logging;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageMetadataServiceTests
{
    private readonly IImageMetadataRepository _metadataRepository =
        Substitute.For<IImageMetadataRepository>();
    private readonly IImageCollectionRepository _collectionRepository =
        Substitute.For<IImageCollectionRepository>();
    private readonly ImageMetadataService _service;
    private readonly IImageStatsRepository _statsRepository =
        Substitute.For<IImageStatsRepository>();
    private readonly IImageRepository _imageRepository = Substitute.For<IImageRepository>();
    private readonly IImageTagRepository _tagRepository = Substitute.For<IImageTagRepository>();
    private readonly IImageAllowedUserRepository _allowedUserRepository =
        Substitute.For<IImageAllowedUserRepository>();

    public ImageMetadataServiceTests()
    {
        _service = new ImageMetadataService(
            _metadataRepository,
            _imageRepository,
            _collectionRepository,
            _statsRepository,
            _tagRepository,
            _allowedUserRepository,
            Substitute.For<ILogger<ImageMetadataService>>()
        );
    }

    [Fact]
    public async Task AddAsync_ShouldValidateAndPersist()
    {
        ImageMetadata metadata = CreateMetadata(meta => meta.ImageCollectionId = null);

        ImageMetadata? storedEntity = null;
        _metadataRepository
            .AddAsync(
                Arg.Do<ImageMetadata>(meta => storedEntity = meta),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ImageMetadata>()));

        ImageMetadata result = await _service.AddAsync(metadata, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(storedEntity);
        Assert.Equal(0, storedEntity!.Id);
        Assert.Equal(metadata.Title, storedEntity.Title);
        await _metadataRepository
            .Received(1)
            .AddAsync(Arg.Any<ImageMetadata>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrowWhenCollectionMissing()
    {
        ImageMetadata metadata = CreateMetadata(meta =>
        {
            meta.ImageCollectionId = 42;
            meta.UserId = "owner";
        });

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 42)
            .Returns(Task.FromResult<Models.ImageCollection?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(metadata, CancellationToken.None)
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowWhenCollectionNotOwned()
    {
        ImageMetadata metadata = CreateMetadata(meta =>
        {
            meta.ImageCollectionId = 42;
            meta.UserId = "owner";
        });

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 42)
            .Returns(
                Task.FromResult<Models.ImageCollection?>(
                    new Models.ImageCollection { UserId = "differentOwner", Title = "Collection" }
                )
            );

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(metadata, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldNotRetrieveUnaccessibleMetadataPagination()
    {
        string userId = "testUser";
        List<ImageMetadata> metadataList = new List<ImageMetadata>
        {
            CreateMetadata(meta => meta.AccessLevel = ImageAccessLevel.Private),
            CreateMetadata(meta => meta.AccessLevel = ImageAccessLevel.AllowedUsers),
            CreateMetadata(meta =>
            {
                meta.AccessLevel = ImageAccessLevel.AllowedUsers;
                meta.AllowedUsers.Add(
                    new ImageAllowedUser { UserId = userId, ImageMetadataId = meta.Id }
                );
            }),
            CreateMetadata(meta => meta.AccessLevel = ImageAccessLevel.Public),
        };

        _metadataRepository.Query().Returns(metadataList.AsQueryable());

        List<ImageMetadata> results = await _service.GetWithPaginationAsync(
            userId,
            new PaginationParams { Size = 10, Skip = 0 },
            CancellationToken.None
        );

        Assert.Contains(results, meta => meta == metadataList[2]);
        Assert.Contains(results, meta => meta == metadataList[3]);
    }

    [Fact]
    public async Task ShouldApplyPagination()
    {
        string userId = "testUser";
        List<ImageMetadata> metadataList = new List<ImageMetadata>
        {
            CreateMetadata(meta => meta.Title = "First"),
            CreateMetadata(meta => meta.Title = "Second"),
            CreateMetadata(meta => meta.Title = "Third"),
            CreateMetadata(meta => meta.Title = "Fourth"),
        };

        _metadataRepository.Query().Returns(metadataList.AsQueryable());

        List<ImageMetadata> results = await _service.GetWithPaginationAsync(
            userId,
            new PaginationParams { Size = 2, Skip = 1 },
            CancellationToken.None
        );

        Assert.Equal(2, results.Count);
        Assert.Equal("Second", results[0].Title);
        Assert.Equal("Third", results[1].Title);
    }

    [Fact]
    public async Task ShouldAttachSASInfo()
    {
        string userId = "testUser";
        ImageMetadata metadata = CreateMetadata(meta =>
        {
            meta.UserId = userId;
            meta.AccessLevel = ImageAccessLevel.Public;
        });

        _metadataRepository.Query().Returns(new[] { metadata }.AsQueryable());

        List<ImageMetadata> results = await _service.GetWithPaginationAsync(
            userId,
            new PaginationParams { Size = 10, Skip = 0 },
            CancellationToken.None
        );

        Assert.Single(results);
        ImageMetadata result = results[0];
        Assert.NotNull(result.Image);
        Assert.Equal("https://example.com/blob", result.Image!.BlobUri);
    }

    [Fact]
    public async Task ShouldNotRetrieveUnaccessibleMetadataPredicate()
    {
        string userId = "testUser";
        List<ImageMetadata> metadataList = new List<ImageMetadata>
        {
            CreateMetadata(meta => meta.AccessLevel = ImageAccessLevel.Private),
            CreateMetadata(meta => meta.AccessLevel = ImageAccessLevel.AllowedUsers),
            CreateMetadata(meta =>
            {
                meta.AccessLevel = ImageAccessLevel.AllowedUsers;
                meta.AllowedUsers.Add(
                    new ImageAllowedUser { UserId = userId, ImageMetadataId = meta.Id }
                );
            }),
            CreateMetadata(meta => meta.AccessLevel = ImageAccessLevel.Public),
        };

        _metadataRepository.Query().Returns(metadataList.AsQueryable());

        List<ImageMetadata> results = await _service.GetByPredicateAsync(
            userId,
            meta => true,
            new PaginationParams { Size = 10, Skip = 0 },
            CancellationToken.None
        );

        Assert.Contains(results, meta => meta == metadataList[2]);
        Assert.Contains(results, meta => meta == metadataList[3]);
    }

    [Fact]
    public async Task ShouldApplyPredicate()
    {
        string userId = "testUser";
        List<ImageMetadata> metadataList = new List<ImageMetadata>
        {
            CreateMetadata(meta => meta.Title = "Match"),
            CreateMetadata(meta => meta.Title = "NoMatch"),
            CreateMetadata(meta => meta.Title = "Match"),
        };

        _metadataRepository.Query().Returns(metadataList.AsQueryable());

        List<ImageMetadata> results = await _service.GetByPredicateAsync(
            userId,
            meta => meta.Title == "Match",
            new PaginationParams { Size = 10, Skip = 0 },
            CancellationToken.None
        );

        Assert.Equal(2, results.Count);
        Assert.All(results, meta => Assert.Equal("Match", meta.Title));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        ImageMetadata existing = CreateMetadata(meta =>
        {
            meta.Id = 5;
            meta.UserId = "owner";
            meta.Title = "Original";
        });

        ImageMetadata update = CreateMetadata(meta =>
        {
            meta.Id = 5;
            meta.UserId = "owner";
            meta.Title = "Updated";
        });

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 5)
            .Returns(Task.FromResult<ImageMetadata?>(existing));

        await _service.UpdateAsync(update, CancellationToken.None);

        await _metadataRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _metadataRepository
            .Received(1)
            .Update(Arg.Is<ImageMetadata>(meta => meta.Title == "Updated"));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowWhenUnauthorized()
    {
        ImageMetadata existing = CreateMetadata(meta =>
        {
            meta.Id = 7;
            meta.UserId = "owner";
        });

        ImageMetadata update = CreateMetadata(meta =>
        {
            meta.Id = 7;
            meta.UserId = "intruder";
        });

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 7)
            .Returns(Task.FromResult<ImageMetadata?>(existing));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateAsync(update, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowWhenCollectionNotOwned()
    {
        ImageMetadata existing = CreateMetadata(meta =>
        {
            meta.Id = 7;
            meta.UserId = "owner";
        });

        ImageMetadata update = CreateMetadata(meta =>
        {
            meta.Id = 7;
            meta.UserId = "owner";
            meta.ImageCollectionId = 99;
        });

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 7)
            .Returns(Task.FromResult<ImageMetadata?>(existing));

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 99)
            .Returns(
                Task.FromResult<Models.ImageCollection?>(
                    new Models.ImageCollection { UserId = "differentOwner", Title = "Collection" }
                )
            );

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(update, CancellationToken.None)
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteWhenOwner()
    {
        ImageMetadata existing = CreateMetadata(meta =>
        {
            meta.Id = 9;
            meta.UserId = "owner";
        });

        _metadataRepository.Query().Returns(new[] { existing }.AsQueryable());

        await _service.RemoveAsync(existing, CancellationToken.None);

        _metadataRepository.Received(1).Remove(existing);
        await _metadataRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowWhenNotFound()
    {
        ImageMetadata entity = CreateMetadata(meta => meta.Id = 11);

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 11)
            .Returns(Task.FromResult<ImageMetadata?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(entity, CancellationToken.None)
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowWhenUnauthorized()
    {
        ImageMetadata existing = CreateMetadata(meta =>
        {
            meta.Id = 12;
            meta.UserId = "owner";
        });

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 12)
            .Returns(Task.FromResult<ImageMetadata?>(existing));

        ImageMetadata toRemove = CreateMetadata(meta =>
        {
            meta.Id = 12;
            meta.UserId = "intruder";
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(toRemove, CancellationToken.None)
        );
    }

    private static int _idCounter = 1;

    private static ImageMetadata CreateMetadata(Action<ImageMetadata>? configure = null)
    {
        var metadata = new ImageMetadata
        {
            Id = _idCounter++,
            Title = "Sample",
            Description = "Description",
            Location = "Location",
            AccessLevel = ImageAccessLevel.Public,
            Tags = new List<ImageTag>(),
            AllowedUsers = new List<ImageAllowedUser>(),
            UserId = "user",
            ImageCollectionId = null,
            Image = new Models.Image
            {
                Id = 50,
                BlobName = "blob",
                BlobUri = "https://example.com/blob",
                ContentType = ImageContentType.Jpeg,
                Size = 1024,
            },
        };

        metadata.ImageId = metadata.Image.Id;

        configure?.Invoke(metadata);
        return metadata;
    }
}
