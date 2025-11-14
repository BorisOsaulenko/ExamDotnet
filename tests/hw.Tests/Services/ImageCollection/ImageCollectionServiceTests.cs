using System.Linq;
using System.Linq.Expressions;
using FluentValidation;
using NSubstitute;
using Repositories;
using Services.ImageCollection;
using Services.Storage;
using ImageCollectionAccessLevel = Models.ImageCollectionAccessLevel;
using ImageCollectionAllowedUser = Models.ImageCollectionAllowedUser;
using ImageCollectionModel = Models.ImageCollection;
using ImageMetadataModel = Models.ImageMetadata;

namespace hw.Tests.Services.ImageCollection;

public sealed class ImageCollectionServiceTests
{
    private readonly IImageCollectionRepository _collectionRepository =
        Substitute.For<IImageCollectionRepository>();
    private readonly IImageMetadataRepository _metadataRepository =
        Substitute.For<IImageMetadataRepository>();

    private readonly ImageCollectionService _service;

    public ImageCollectionServiceTests()
    {
        _service = new ImageCollectionService(
            _collectionRepository,
            _metadataRepository
        );
    }

    [Fact]
    public async Task AddAsync_ShouldValidateAndPersist()
    {
        ImageCollectionModel collection = CreateCollection();
        _collectionRepository
            .AddAsync(collection, Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ImageCollectionModel>()));

        ImageCollectionModel result = await _service.AddAsync(collection, CancellationToken.None);

        Assert.Equal(collection, result);
        await _collectionRepository.Received(1).AddAsync(collection, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenValidationFails()
    {
        ImageCollectionModel collection = CreateCollection(c => c.Title = string.Empty);

        await Assert.ThrowsAsync<ValidationException>(
            () => _service.AddAsync(collection, CancellationToken.None)
        );

        await _collectionRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageCollectionModel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenCoverImageInvalid()
    {
        ImageCollectionModel collection = CreateCollection(c =>
        {
            c.CoverImageMetadataId = 5;
        });

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), collection.CoverImageMetadataId!.Value)
            .Returns(
                Task.FromResult<ImageMetadataModel?>(
                    new ImageMetadataModel
                    {
                        Id = 5,
                        ImageCollectionId = 999,
                        Title = "Other",
                        UserId = collection.UserId,
                    }
                )
            );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.AddAsync(collection, CancellationToken.None)
        );

        await _collectionRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageCollectionModel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByPredicateAsync_ShouldApplyPredicateAndAccessFilter()
    {
        string userId = "user";
        List<ImageCollectionModel> collections =
        [
            CreateCollection(c =>
            {
                c.Title = "Match";
                c.AccessLevel = ImageCollectionAccessLevel.Public;
            }),
            CreateCollection(c =>
            {
                c.Title = "Match";
                c.AccessLevel = ImageCollectionAccessLevel.AllowedUsers;
                c.AllowedUsers.Add(
                    new ImageCollectionAllowedUser { UserId = userId, ImageCollectionId = c.Id }
                );
            }),
            CreateCollection(c =>
            {
                c.Title = "Match";
                c.AccessLevel = ImageCollectionAccessLevel.Private;
                c.UserId = "different";
            }),
            CreateCollection(c => c.Title = "NoMatch"),
        ];

        _collectionRepository.Query().Returns(collections.AsQueryable());

        List<ImageCollectionModel> result = await _service.GetByPredicateAsync(
            c => c.Title == "Match",
            userId,
            CancellationToken.None
        );

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal("Match", c.Title));
        Assert.DoesNotContain(
            result,
            c => c.AccessLevel == ImageCollectionAccessLevel.Private && c.UserId != userId
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        ImageCollectionModel existing = CreateCollection(c =>
        {
            c.Id = 10;
            c.Title = "Original";
            c.Description = "Desc";
        });

        ImageCollectionModel update = CreateCollection(c =>
        {
            c.Id = 10;
            c.Title = "Updated";
            c.Description = "UpdatedDesc";
            c.AccessLevel = ImageCollectionAccessLevel.AllowedUsers;
            c.CoverImageMetadataId = 5;
        });

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), update.Id)
            .Returns(Task.FromResult<ImageCollectionModel?>(existing));

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), update.CoverImageMetadataId!.Value)
            .Returns(
                Task.FromResult<ImageMetadataModel?>(
                    new ImageMetadataModel
                    {
                        Id = 5,
                        ImageCollectionId = update.Id,
                        Title = "Cover",
                        UserId = update.UserId,
                    }
                )
            );

        await _service.UpdateAsync(update, CancellationToken.None);

        _collectionRepository
            .Received(1)
            .Update(
                Arg.Is<ImageCollectionModel>(c =>
                    c.Title == "Updated"
                    && c.Description == "UpdatedDesc"
                    && c.AccessLevel == ImageCollectionAccessLevel.AllowedUsers
                    && c.CoverImageMetadataId == 5
                )
            );
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCollectionMissing()
    {
        ImageCollectionModel update = CreateCollection(c => c.Id = 99);

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), update.Id)
            .Returns(Task.FromResult<ImageCollectionModel?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(update, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUnauthorized()
    {
        ImageCollectionModel existing = CreateCollection(c =>
        {
            c.Id = 5;
            c.UserId = "owner";
        });
        ImageCollectionModel update = CreateCollection(c =>
        {
            c.Id = 5;
            c.UserId = "other";
        });

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), update.Id)
            .Returns(Task.FromResult<ImageCollectionModel?>(existing));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(update, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCoverImageInvalid()
    {
        ImageCollectionModel existing = CreateCollection(c => c.Id = 5);
        ImageCollectionModel update = CreateCollection(c =>
        {
            c.Id = 5;
            c.CoverImageMetadataId = 7;
        });

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), update.Id)
            .Returns(Task.FromResult<ImageCollectionModel?>(existing));

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), update.CoverImageMetadataId!.Value)
            .Returns(
                Task.FromResult<ImageMetadataModel?>(
                    new ImageMetadataModel
                    {
                        Id = 7,
                        ImageCollectionId = 999,
                        Title = "Other",
                        UserId = update.UserId,
                    }
                )
            );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateAsync(update, CancellationToken.None)
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldDelete_WhenOwnerMatches()
    {
        ImageCollectionModel existing = CreateCollection(c => c.Id = 7);

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), existing.Id)
            .Returns(Task.FromResult<ImageCollectionModel?>(existing));

        await _service.RemoveAsync(existing, CancellationToken.None);

        _collectionRepository.Received(1).Remove(existing);
        await _collectionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenMissingOrUnauthorized()
    {
        ImageCollectionModel entity = CreateCollection(c => c.Id = 8);

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), entity.Id)
            .Returns(Task.FromResult<ImageCollectionModel?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(entity, CancellationToken.None)
        );

        ImageCollectionModel existingDifferentOwner = CreateCollection(c =>
        {
            c.Id = 8;
            c.UserId = "someone-else";
        });

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), entity.Id)
            .Returns(Task.FromResult<ImageCollectionModel?>(existingDifferentOwner));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(entity, CancellationToken.None)
        );
    }

    private static ImageCollectionModel CreateCollection(
        Action<ImageCollectionModel>? configure = null
    )
    {
        var collection = new ImageCollectionModel
        {
            Id = 1,
            Title = "Collection",
            Description = "Description",
            AccessLevel = ImageCollectionAccessLevel.Public,
            UserId = "owner",
        };

        configure?.Invoke(collection);
        return collection;
    }
}
