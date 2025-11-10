using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hw.Tests.TestInfrastructure.AsyncEnumerable;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;
using Xunit;
using ImageModel = Models.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageCommentServiceTests
{
    private readonly IImageCommentRepository _commentRepository =
        Substitute.For<IImageCommentRepository>();
    private readonly IImageRepository _imageRepository = Substitute.For<IImageRepository>();
    private readonly TestImageFactory _imageFactory = new();
    private readonly ImageCommentService _service;
    private int _commentId;

    public ImageCommentServiceTests()
    {
        _service = new ImageCommentService(_commentRepository, _imageRepository);
    }

    [Fact]
    public async Task ShouldAddCommentWhenUserHasAccess()
    {
        ImageModel image = _imageFactory.Create(img => img.Id = 10);
        ImageComment comment = CreateComment(image.Id, image.UserId);

        _imageRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), image.Id)
            .Returns(Task.FromResult<ImageModel?>(image));
        _commentRepository
            .AddAsync(comment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(comment));

        ImageComment result = await _service.AddAsync(comment, CancellationToken.None);

        Assert.Same(comment, result);
        await _commentRepository.Received(1).AddAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowWhenImageMissingOnAdd()
    {
        ImageComment comment = CreateComment(999, "user123");
        _imageRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), comment.ImageId)
            .Returns(Task.FromResult<ImageModel?>(null));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.AddAsync(comment, CancellationToken.None)
        );
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<ImageComment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowWhenUserNotAllowedOnAdd()
    {
        ImageModel image = _imageFactory.Create(img =>
        {
            img.Id = 42;
            img.AccessLevel = ImageAccessLevel.Private;
            img.UserId = "owner";
        });
        ImageComment comment = CreateComment(image.Id, "otherUser");

        _imageRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), image.Id)
            .Returns(Task.FromResult<ImageModel?>(image));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.AddAsync(comment, CancellationToken.None)
        );
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<ImageComment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldFilterCommentsByAccess()
    {
        string currentUser = "viewer";

        ImageModel ownImage = _imageFactory.Create(img =>
        {
            img.Id = 1;
            img.UserId = currentUser;
        });
        ImageModel allowedImage = _imageFactory.Create(img =>
        {
            img.Id = 2;
            img.AccessLevel = ImageAccessLevel.AllowedUsers;
            img.AllowedUsers = new List<ImageAllowedUser>
            {
                new() { ImageId = img.Id, UserId = currentUser },
            };
        });
        ImageModel blockedImage = _imageFactory.Create(img =>
        {
            img.Id = 3;
            img.AccessLevel = ImageAccessLevel.Private;
            img.UserId = "stranger";
        });

        var comments = new[]
        {
            CreateComment(ownImage.Id, "someone"),
            CreateComment(allowedImage.Id, "another"),
            CreateComment(blockedImage.Id, "blocked"),
        };

        _commentRepository
            .Query()
            .Returns(new TestAsyncEnumerable<ImageComment>(comments.AsQueryable()));
        _imageRepository
            .Query()
            .Returns(
                new TestAsyncEnumerable<ImageModel>(
                    new[] { ownImage, allowedImage, blockedImage }.AsQueryable()
                )
            );

        PaginationParams pagination = new() { Skip = 0, Size = 10 };
        List<ImageComment> result = await _service.GetByPredicateAsync(
            currentUser,
            _ => true,
            pagination,
            CancellationToken.None
        );

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, c => c.ImageId == blockedImage.Id);
    }

    [Fact]
    public async Task ShouldRemoveCommentWhenOwner()
    {
        ImageComment existing = CreateComment(10, "owner");
        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), existing.Id)
            .Returns(Task.FromResult<ImageComment?>(existing));

        await _service.RemoveAsync(existing, CancellationToken.None);

        _commentRepository.Received(1).Remove(existing);
        await _commentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowOnRemoveWhenNotOwner()
    {
        ImageComment existing = CreateComment(10, "owner");
        ImageComment request = CreateComment(10, "other");

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.Id)
            .Returns(Task.FromResult<ImageComment?>(existing));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemoveAsync(request, CancellationToken.None)
        );

        _commentRepository.DidNotReceive().Remove(Arg.Any<ImageComment>());
    }

    [Fact]
    public async Task ShouldUpdateCommentWhenOwner()
    {
        ImageComment request = CreateComment(5, "owner", "Updated");
        ImageComment existing = CreateComment(5, "owner", "Old");

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.Id)
            .Returns(Task.FromResult<ImageComment?>(existing));

        await _service.UpdateAsync(request, CancellationToken.None);

        _commentRepository.Received(1).Update(request);
        await _commentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowOnUpdateWhenCommentMissing()
    {
        ImageComment request = CreateComment(5, "owner");
        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.Id)
            .Returns(Task.FromResult<ImageComment?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ShouldThrowOnUpdateWhenNotOwner()
    {
        ImageComment request = CreateComment(5, "user1");
        ImageComment existing = CreateComment(5, "user2");

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), request.Id)
            .Returns(Task.FromResult<ImageComment?>(existing));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(request, CancellationToken.None)
        );
    }

    private ImageComment CreateComment(int imageId, string userId, string content = "Nice shot!")
    {
        return new ImageComment
        {
            Id = Interlocked.Increment(ref _commentId),
            ImageId = imageId,
            UserId = userId,
            Content = content,
        };
    }
}
