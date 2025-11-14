using System.Linq.Expressions;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageCommentServiceTests
{
    private readonly IImageCommentRepository _commentRepository =
        Substitute.For<IImageCommentRepository>();

    private readonly IImageMetadataRepository _metadataRepository =
        Substitute.For<IImageMetadataRepository>();
    private readonly ImageCommentService _service;

    public ImageCommentServiceTests()
    {
        _service = new ImageCommentService(_commentRepository, _metadataRepository);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistWhenUserHasAccess()
    {
        ImageComment comment = CreateComment(c =>
        {
            c.UserId = "viewer";
            c.ImageStatsId = 10;
        });
        ImageMetadata metadata = CreateMetadata(
            comment.ImageStatsId,
            meta =>
            {
                meta.AccessLevel = ImageAccessLevel.Public;
            }
        );

        _metadataRepository.Query().Returns(new[] { metadata }.AsQueryable());
        _commentRepository
            .AddAsync(Arg.Any<ImageComment>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ImageComment>()));

        ImageComment result = await _service.AddAsync(comment, CancellationToken.None);

        Assert.Equal(comment, result);
        await _commentRepository.Received(1).AddAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrowWhenAccessDenied()
    {
        ImageComment comment = CreateComment(c =>
        {
            c.UserId = "viewer";
            c.ImageStatsId = 20;
        });
        ImageMetadata metadata = CreateMetadata(
            comment.ImageStatsId,
            meta =>
            {
                meta.AccessLevel = ImageAccessLevel.Private;
                meta.UserId = "owner";
            }
        );

        _metadataRepository
            .GetByPredicateAsync(
                Arg.Any<Expression<Func<ImageMetadata, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(new List<ImageMetadata> { metadata }));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddAsync(comment, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetByPredicateAsync_ShouldReturnCommentsWithAccessibleImages()
    {
        string viewerId = "viewer";
        ImageComment comment1 = CreateComment(c =>
        {
            c.Id = 1;
            c.ImageStatsId = 101;
        });
        ImageComment comment2 = CreateComment(c =>
        {
            c.Id = 2;
            c.ImageStatsId = 102;
        });

        _commentRepository.Query().Returns(new[] { comment1, comment2 }.AsQueryable());

        ImageMetadata publicMetadata = CreateMetadata(
            101,
            meta =>
            {
                meta.AccessLevel = ImageAccessLevel.Public;
            }
        );
        ImageMetadata privateMetadata = CreateMetadata(
            102,
            meta =>
            {
                meta.AccessLevel = ImageAccessLevel.Private;
                meta.UserId = "other";
            }
        );

        _metadataRepository
            .Query()
            .Returns(new[] { publicMetadata, privateMetadata }.AsQueryable());

        List<ImageComment> result = await _service.GetByPredicateAsync(
            viewerId,
            _ => true,
            new PaginationParams { Skip = 0, Size = 10 },
            CancellationToken.None
        );

        Assert.Single(result);
        Assert.Contains(result, c => c.Id == comment1.Id);
        Assert.DoesNotContain(result, c => c.Id == comment2.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistWhenOwner()
    {
        ImageComment existing = CreateComment(c =>
        {
            c.Id = 5;
            c.UserId = "owner";
            c.Content = "Old";
        });
        ImageComment updated = CreateComment(c =>
        {
            c.Id = 5;
            c.UserId = "owner";
            c.Content = "Updated";
        });

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 5)
            .Returns(Task.FromResult<ImageComment?>(existing));

        await _service.UpdateAsync(updated, CancellationToken.None);

        _commentRepository.Received(1).Update(Arg.Is<ImageComment>(c => c.Content == "Updated"));
        await _commentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowWhenUnauthorized()
    {
        ImageComment existing = CreateComment(c =>
        {
            c.Id = 6;
            c.UserId = "owner";
        });
        ImageComment updated = CreateComment(c =>
        {
            c.Id = 6;
            c.UserId = "other";
        });

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 6)
            .Returns(Task.FromResult<ImageComment?>(existing));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(updated, CancellationToken.None)
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteWhenOwner()
    {
        ImageComment existing = CreateComment(c =>
        {
            c.Id = 7;
            c.UserId = "owner";
        });

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 7)
            .Returns(Task.FromResult<ImageComment?>(existing));

        await _service.RemoveAsync(existing, CancellationToken.None);

        _commentRepository.Received(1).Remove(existing);
        await _commentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowWhenCommentMissingOrUnauthorized()
    {
        ImageComment request = CreateComment(c =>
        {
            c.Id = 8;
            c.UserId = "requester";
        });

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 8)
            .Returns(Task.FromResult<ImageComment?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(request, CancellationToken.None)
        );

        ImageComment stored = CreateComment(c =>
        {
            c.Id = 8;
            c.UserId = "different";
        });

        _commentRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), 8)
            .Returns(Task.FromResult<ImageComment?>(stored));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(request, CancellationToken.None)
        );
    }

    private static int _commentIdSeed;
    private static int _metadataIdSeed;

    private static ImageComment CreateComment(Action<ImageComment>? configure = null)
    {
        var comment = new ImageComment
        {
            Id = ++_commentIdSeed,
            ImageStatsId = 1,
            UserId = "user",
            Content = "Nice photo!",
        };

        configure?.Invoke(comment);
        return comment;
    }

    private static ImageMetadata CreateMetadata(
        int imageStatsId,
        Action<ImageMetadata>? configure = null
    )
    {
        var metadata = new ImageMetadata
        {
            Id = ++_metadataIdSeed,
            ImageStatsId = imageStatsId,
            Title = "Test",
            Description = "Desc",
            Location = "Location",
            AccessLevel = ImageAccessLevel.Public,
            AllowedUsers = new List<ImageAllowedUser>(),
            Tags = new List<ImageTag>(),
            UserId = "owner",
        };

        configure?.Invoke(metadata);
        return metadata;
    }
}
