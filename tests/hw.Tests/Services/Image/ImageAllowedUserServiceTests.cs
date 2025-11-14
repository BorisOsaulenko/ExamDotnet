using System.Linq.Expressions;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageAllowedUserServiceTests
{
    private readonly IImageAllowedUserRepository _allowedUserRepository =
        Substitute.For<IImageAllowedUserRepository>();
    private readonly IImageMetadataRepository _imageMetadataRepository =
        Substitute.For<IImageMetadataRepository>();

    private readonly ImageAllowedUserService _service;

    public ImageAllowedUserServiceTests()
    {
        _service = new ImageAllowedUserService(_allowedUserRepository, _imageMetadataRepository);
    }

    [Fact]
    public async Task AddAsync_ShouldPersist_WhenUserOwnsImage()
    {
        const string ownerId = "owner";
        ImageAllowedUser request = CreateAllowedUser();
        ImageMetadata image = new()
        {
            Id = request.ImageMetadataId,
            UserId = ownerId,
            Title = "Image",
        };

        _imageMetadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(image));

        _allowedUserRepository
            .AddAsync(Arg.Any<ImageAllowedUser>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ImageAllowedUser>()));

        ImageAllowedUser result = await _service.AddAsync(ownerId, request, CancellationToken.None);

        Assert.Equal(request, result);
        await _allowedUserRepository
            .Received(1)
            .AddAsync(Arg.Is<ImageAllowedUser>(au => au == request), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenImageMissing()
    {
        ImageAllowedUser request = CreateAllowedUser();

        _imageMetadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync("user", request, CancellationToken.None)
        );

        await _allowedUserRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageAllowedUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenUserDoesNotOwnImage()
    {
        ImageAllowedUser request = CreateAllowedUser();
        ImageMetadata image = new()
        {
            Id = request.ImageMetadataId,
            UserId = "different",
            Title = "Image",
        };

        _imageMetadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(image));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync("owner", request, CancellationToken.None)
        );

        await _allowedUserRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageAllowedUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldDelete_WhenEntityBelongsToUser()
    {
        const string ownerId = "owner";
        ImageAllowedUser request = CreateAllowedUser(
            userId: ownerId,
            imageMetadataId: 5,
            Image: new ImageMetadata
            {
                Id = 5,
                UserId = ownerId,
                Title = "Image",
            }
        );

        _allowedUserRepository.Query().Returns(new[] { request }.AsQueryable());

        await _service.RemoveAsync(ownerId, request, CancellationToken.None);

        _allowedUserRepository.Received(1).Remove(Arg.Is<ImageAllowedUser>(au => au == request));
        await _allowedUserRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenEntityMissing()
    {
        ImageAllowedUser request = CreateAllowedUser();

        _allowedUserRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.Id)
            .Returns(Task.FromResult<ImageAllowedUser?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync("owner", request, CancellationToken.None)
        );

        _allowedUserRepository.DidNotReceive().Remove(Arg.Any<ImageAllowedUser>());
        await _allowedUserRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenEntityOwnedByDifferentUser()
    {
        ImageAllowedUser request = CreateAllowedUser();
        ImageAllowedUser stored = CreateAllowedUser(userId: "different");

        _allowedUserRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.Id)
            .Returns(Task.FromResult<ImageAllowedUser?>(stored));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync("owner", request, CancellationToken.None)
        );

        _allowedUserRepository.DidNotReceive().Remove(Arg.Any<ImageAllowedUser>());
        await _allowedUserRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAllForImageAsync_ShouldRemoveRange_WhenEntriesExist()
    {
        int metadataId = 10;
        List<ImageAllowedUser> allowedUsers = new()
        {
            CreateAllowedUser(imageMetadataId: metadataId),
            CreateAllowedUser(imageMetadataId: metadataId + 1),
        };

        _allowedUserRepository
            .GetByPredicateAsync(
                Arg.Any<Expression<Func<ImageAllowedUser, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(allowedUsers));

        await _service.RemoveAllForImageAsync(metadataId, CancellationToken.None);

        _allowedUserRepository
            .Received(1)
            .RemoveRange(Arg.Is<IEnumerable<ImageAllowedUser>>(users => users == allowedUsers));
        await _allowedUserRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAllForImageAsync_ShouldSkip_WhenNoEntries()
    {
        _allowedUserRepository
            .GetByPredicateAsync(
                Arg.Any<Expression<Func<ImageAllowedUser, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(new List<ImageAllowedUser>()));

        await _service.RemoveAllForImageAsync(5, CancellationToken.None);

        _allowedUserRepository
            .DidNotReceive()
            .RemoveRange(Arg.Any<IEnumerable<ImageAllowedUser>>());
        await _allowedUserRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAllForUserAsync_ShouldRemoveRange_AndPersist()
    {
        string userId = "user";
        List<ImageAllowedUser> allowedUsers = new()
        {
            CreateAllowedUser(userId: userId),
            CreateAllowedUser(userId: userId),
        };

        _allowedUserRepository
            .GetByPredicateAsync(
                Arg.Any<Expression<Func<ImageAllowedUser, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(allowedUsers));

        await _service.RemoveAllForUserAsync(userId, CancellationToken.None);

        _allowedUserRepository
            .Received(1)
            .RemoveRange(Arg.Is<IEnumerable<ImageAllowedUser>>(users => users == allowedUsers));
        await _allowedUserRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ImageAllowedUser CreateAllowedUser(
        int id = 1,
        int imageMetadataId = 2,
        string userId = "user",
        ImageMetadata? Image = null
    )
    {
        return new ImageAllowedUser
        {
            Id = id,
            ImageMetadataId = imageMetadataId,
            UserId = userId,
            ImageMetadata = Image,
        };
    }
}
