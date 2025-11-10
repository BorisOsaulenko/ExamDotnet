using System;
using System.Threading;
using System.Threading.Tasks;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;
using Xunit;
using ImageModel = Models.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageAllowedUserServiceTests
{
    private readonly IImageAllowedUserRepository _allowedRepo =
        Substitute.For<IImageAllowedUserRepository>();
    private readonly IImageRepository _imageRepository = Substitute.For<IImageRepository>();
    private readonly TestImageFactory _imageFactory = new();
    private readonly ImageAllowedUserService _service;

    public ImageAllowedUserServiceTests()
    {
        _service = new ImageAllowedUserService(_allowedRepo, _imageRepository);
    }

    [Fact]
    public async Task ShouldAddAllowedUserWhenImageExists()
    {
        ImageModel image = _imageFactory.Create(img => img.Id = 15);
        ImageAllowedUser allowed = CreateAllowedUser(image.Id, "userB");

        _imageRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), image.Id)
            .Returns(Task.FromResult<ImageModel?>(image));
        _allowedRepo
            .AddAsync(allowed, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(allowed));

        ImageAllowedUser result = await _service.AddAsync(allowed, CancellationToken.None);

        Assert.Same(allowed, result);
        await _allowedRepo.Received(1).AddAsync(allowed, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowOnAddWhenImageMissing()
    {
        ImageAllowedUser allowed = CreateAllowedUser(999, "userC");
        _imageRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowed.ImageId)
            .Returns(Task.FromResult<ImageModel?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(allowed, CancellationToken.None)
        );

        await _allowedRepo
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageAllowedUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldRemoveAllowedUserWhenExists()
    {
        ImageModel image = _imageFactory.Create(img => img.Id = 22);
        ImageAllowedUser allowed = CreateAllowedUser(image.Id, "userD");

        _allowedRepo
            .GetByIdAsync(Arg.Any<CancellationToken>(), allowed.Id)
            .Returns(Task.FromResult<ImageAllowedUser?>(allowed));

        await _service.RemoveAsync(allowed, CancellationToken.None);

        _allowedRepo.Received(1).Remove(allowed);
        await _allowedRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowOnRemoveWhenAllowedUserMissing()
    {
        ImageModel image = _imageFactory.Create(img => img.Id = 33);
        ImageAllowedUser allowed = CreateAllowedUser(image.Id, "userF");

        _allowedRepo
            .GetByPredicateAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<ImageAllowedUser, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<List<ImageAllowedUser>>(new List<ImageAllowedUser>()));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(allowed, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldRemoveAllAllowedUsersForImage()
    {
        ImageModel image = _imageFactory.Create(img => img.Id = 44);
        List<ImageAllowedUser> allowedUsers = new()
        {
            CreateAllowedUser(image.Id, "user1"),
            CreateAllowedUser(image.Id, "user2"),
            CreateAllowedUser(image.Id, "user3")
        };

        _allowedRepo
            .GetByPredicateAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<ImageAllowedUser, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(allowedUsers));

        await _service.RemoveAllForImageAsync(image.Id, CancellationToken.None);

        _allowedRepo.Received(1).RemoveRange(allowedUsers);
        await _allowedRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldDoNothingWhenNoAllowedUsersForImage()
    {
        ImageModel image = _imageFactory.Create(img => img.Id = 55);

        _allowedRepo
            .GetByPredicateAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<ImageAllowedUser, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(new List<ImageAllowedUser>()));

        await _service.RemoveAllForImageAsync(image.Id, CancellationToken.None);

        _allowedRepo.DidNotReceive().RemoveRange(Arg.Any<IEnumerable<ImageAllowedUser>>());
    }

    [Fact]
    public async Task ShouldRemoveAllAllowedUsersForUser()
    {
        List<ImageAllowedUser> allowedUsers = new()
        {
            CreateAllowedUser(1, "userX"),
            CreateAllowedUser(2, "userX"),
            CreateAllowedUser(3, "userX")
        };

        _allowedRepo
            .GetByPredicateAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<ImageAllowedUser, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(allowedUsers));

        await _service.RemoveAllForUserAsync("userX", CancellationToken.None);

        _allowedRepo.Received(1).RemoveRange(allowedUsers);
        await _allowedRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ImageAllowedUser CreateAllowedUser(int imageId, string userId) =>
        new() { ImageId = imageId, UserId = userId };
}
