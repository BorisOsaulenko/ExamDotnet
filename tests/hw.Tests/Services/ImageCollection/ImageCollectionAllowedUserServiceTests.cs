using Models;
using NSubstitute;
using Repositories;
using Services.ImageCollection;
using ImageCollectionModel = Models.ImageCollection;

namespace hw.Tests.Services.ImageCollection;

public sealed class ImageCollectionAllowedUserServiceTests
{
    private readonly IImageCollectionAllowedUserRepository _allowedUserRepository =
        Substitute.For<IImageCollectionAllowedUserRepository>();
    private readonly IImageCollectionRepository _collectionRepository =
        Substitute.For<IImageCollectionRepository>();

    private readonly ImageCollectionAllowedUserService _service;

    public ImageCollectionAllowedUserServiceTests()
    {
        _service = new ImageCollectionAllowedUserService(
            _allowedUserRepository,
            _collectionRepository
        );
    }

    [Fact]
    public async Task AddAsync_ShouldPersist_WhenCollectionExistsAndOwned()
    {
        ImageCollectionAllowedUser allowedUser = CreateAllowedUser();
        ImageCollectionModel collection = CreateCollection(
            allowedUser.ImageCollectionId,
            allowedUser.UserId
        );

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowedUser.ImageCollectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(collection));
        _allowedUserRepository
            .AddAsync(Arg.Any<ImageCollectionAllowedUser>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ImageCollectionAllowedUser>()));

        ImageCollectionAllowedUser result = await _service.AddAsync(
            allowedUser,
            CancellationToken.None
        );

        Assert.Equal(allowedUser, result);
        await _allowedUserRepository
            .Received(1)
            .AddAsync(
                Arg.Is<ImageCollectionAllowedUser>(au => au == allowedUser),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenCollectionMissing()
    {
        ImageCollectionAllowedUser allowedUser = CreateAllowedUser();

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowedUser.ImageCollectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(allowedUser, CancellationToken.None)
        );

        await _allowedUserRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageCollectionAllowedUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenOwnerMismatch()
    {
        ImageCollectionAllowedUser allowedUser = CreateAllowedUser(userId: "owner");
        ImageCollectionModel collection = CreateCollection(
            allowedUser.ImageCollectionId,
            "otherOwner"
        );

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowedUser.ImageCollectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(collection));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(allowedUser, CancellationToken.None)
        );

        await _allowedUserRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageCollectionAllowedUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByCollectionIdAsync_ShouldReturnAllowedUsers()
    {
        int collectionId = 1;
        string ownerId = "owner";
        List<ImageCollectionAllowedUser> allowedUsers = new()
        {
            CreateAllowedUser(id: 1, collectionId: collectionId, userId: "user1"),
            CreateAllowedUser(id: 2, collectionId: collectionId, userId: "user2"),
        };

        _allowedUserRepository.Query().Returns(allowedUsers.AsQueryable());
        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), collectionId)
            .Returns(
                Task.FromResult<ImageCollectionModel?>(CreateCollection(collectionId, ownerId))
            );

        List<ImageCollectionAllowedUser> result = await _service.GetByCollectionIdAsync(
            ownerId,
            collectionId,
            CancellationToken.None
        );

        Assert.Equal(2, result.Count);
        Assert.Contains(result, au => au.UserId == "user1");
        Assert.Contains(result, au => au.UserId == "user2");
    }

    [Fact]
    public async Task GetByCollectionIdAsync_ShouldThrow_WhenOwnerMismatch()
    {
        int collectionId = 1;
        string ownerId = "owner";

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), collectionId)
            .Returns(
                Task.FromResult<ImageCollectionModel?>(
                    CreateCollection(collectionId, "differentOwner")
                )
            );

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetByCollectionIdAsync(ownerId, collectionId, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ReplaceAllowedUsersAsync_ShouldUpdateAllowedUsers()
    {
        int collectionId = 1;
        string ownerId = "owner";
        List<ImageCollectionAllowedUser> newAllowedUsers = new()
        {
            CreateAllowedUser(id: 0, collectionId: collectionId, userId: "userA"),
            CreateAllowedUser(id: 0, collectionId: collectionId, userId: "userB"),
        };

        ImageCollectionModel collection = CreateCollection(collectionId, ownerId);

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), collectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(collection));

        _allowedUserRepository
            .Query()
            .Returns(
                new List<ImageCollectionAllowedUser>
                {
                    CreateAllowedUser(id: 1, collectionId: collectionId, userId: "oldUser"),
                }.AsQueryable()
            );

        await _service.ReplaceAllowedUsersAsync(
            ownerId,
            collectionId,
            newAllowedUsers,
            CancellationToken.None
        );

        await _allowedUserRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _allowedUserRepository
            .Received(1)
            .AddRangeAsync(
                Arg.Is<IEnumerable<ImageCollectionAllowedUser>>(aus =>
                    aus.Count() == 2
                    && aus.Any(au => au.UserId == "userA")
                    && aus.Any(au => au.UserId == "userB")
                ),
                Arg.Any<CancellationToken>()
            );

        _allowedUserRepository
            .Received(1)
            .RemoveRange(
                Arg.Is<IEnumerable<ImageCollectionAllowedUser>>(aus =>
                    aus.Count() == 1 && aus.Any(au => au.UserId == "oldUser")
                )
            );
    }

    [Fact]
    public async Task ReplaceAllowedUsersAsync_ShouldThrow_WhenOwnerMismatch()
    {
        int collectionId = 1;
        string ownerId = "owner";
        List<ImageCollectionAllowedUser> newAllowedUsers = new()
        {
            CreateAllowedUser(id: 0, collectionId: collectionId, userId: "userA"),
        };

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), collectionId)
            .Returns(
                Task.FromResult<ImageCollectionModel?>(
                    CreateCollection(collectionId, "differentOwner")
                )
            );

        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                _service.ReplaceAllowedUsersAsync(
                    ownerId,
                    collectionId,
                    newAllowedUsers,
                    CancellationToken.None
                )
        );

        await _allowedUserRepository
            .DidNotReceive()
            .AddRangeAsync(
                Arg.Any<IEnumerable<ImageCollectionAllowedUser>>(),
                Arg.Any<CancellationToken>()
            );
        _allowedUserRepository
            .DidNotReceive()
            .RemoveRange(Arg.Any<IEnumerable<ImageCollectionAllowedUser>>());
    }

    [Fact]
    public async Task ReplaceAllowedUsersAsync_ShouldThrow_WhenCollectionMissing()
    {
        int collectionId = 1;
        string ownerId = "owner";
        List<ImageCollectionAllowedUser> newAllowedUsers = new()
        {
            CreateAllowedUser(id: 0, collectionId: collectionId, userId: "userA"),
        };

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), collectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                _service.ReplaceAllowedUsersAsync(
                    ownerId,
                    collectionId,
                    newAllowedUsers,
                    CancellationToken.None
                )
        );

        await _allowedUserRepository
            .DidNotReceive()
            .AddRangeAsync(
                Arg.Any<IEnumerable<ImageCollectionAllowedUser>>(),
                Arg.Any<CancellationToken>()
            );
        _allowedUserRepository
            .DidNotReceive()
            .RemoveRange(Arg.Any<IEnumerable<ImageCollectionAllowedUser>>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldDelete_WhenCollectionExists()
    {
        ImageCollectionAllowedUser allowedUser = CreateAllowedUser();
        ImageCollectionModel collection = CreateCollection(
            allowedUser.ImageCollectionId,
            allowedUser.UserId
        );

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowedUser.ImageCollectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(collection));

        await _service.RemoveAsync(allowedUser, CancellationToken.None);

        _allowedUserRepository
            .Received(1)
            .Remove(Arg.Is<ImageCollectionAllowedUser>(au => au == allowedUser));
        await _allowedUserRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenCollectionMissing()
    {
        ImageCollectionAllowedUser allowedUser = CreateAllowedUser();

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowedUser.ImageCollectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(allowedUser, CancellationToken.None)
        );

        _allowedUserRepository.DidNotReceive().Remove(Arg.Any<ImageCollectionAllowedUser>());
        await _allowedUserRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenOwnerMismatch()
    {
        ImageCollectionAllowedUser allowedUser = CreateAllowedUser(userId: "owner");
        ImageCollectionModel collection = CreateCollection(
            allowedUser.ImageCollectionId,
            "otherOwner"
        );

        _collectionRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowedUser.ImageCollectionId)
            .Returns(Task.FromResult<ImageCollectionModel?>(collection));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(allowedUser, CancellationToken.None)
        );

        _allowedUserRepository.DidNotReceive().Remove(Arg.Any<ImageCollectionAllowedUser>());
        await _allowedUserRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ImageCollectionAllowedUser CreateAllowedUser(
        int id = 1,
        int collectionId = 2,
        string userId = "owner"
    )
    {
        return new ImageCollectionAllowedUser
        {
            Id = id,
            ImageCollectionId = collectionId,
            UserId = userId,
        };
    }

    private static ImageCollectionModel CreateCollection(int id, string ownerId)
    {
        return new ImageCollectionModel
        {
            Id = id,
            Title = "Collection",
            Description = "Desc",
            UserId = ownerId,
        };
    }
}
