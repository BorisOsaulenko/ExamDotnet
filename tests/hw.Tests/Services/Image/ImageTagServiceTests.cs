using System.Linq.Expressions;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageTagServiceTests
{
    private readonly IImageTagRepository _tagRepository = Substitute.For<IImageTagRepository>();
    private readonly IImageMetadataRepository _metadataRepository =
        Substitute.For<IImageMetadataRepository>();

    private readonly ImageTagService _service;

    public ImageTagServiceTests()
    {
        _service = new ImageTagService(_tagRepository, _metadataRepository);
    }

    [Fact]
    public async Task AddAsync_ShouldPersist_WhenOwnerAndUnique()
    {
        ImageTag tag = CreateTag();
        ImageMetadata metadata = CreateMetadata(tag.ImageMetadataId, "owner");

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(metadata));
        _tagRepository
            .ExistsAsync(
                Arg.Any<Expression<Func<ImageTag, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(false));
        _tagRepository
            .AddAsync(Arg.Any<ImageTag>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ImageTag>()));

        ImageTag result = await _service.AddAsync("owner", tag, CancellationToken.None);

        Assert.Equal(0, tag.Id);
        Assert.Equal(tag, result);
        await _tagRepository
            .Received(1)
            .AddAsync(Arg.Is<ImageTag>(t => t == tag), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenMetadataMissingOrNotOwned()
    {
        ImageTag tag = CreateTag();
        ImageMetadata metadata = CreateMetadata(tag.ImageMetadataId, "different");

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(null));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.AddAsync("owner", tag, CancellationToken.None)
        );

        await _tagRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageTag>(), Arg.Any<CancellationToken>());

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(metadata));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.AddAsync("owner", tag, CancellationToken.None)
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenDuplicateTagExists()
    {
        ImageTag tag = CreateTag();
        ImageMetadata metadata = CreateMetadata(tag.ImageMetadataId, "owner");

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(metadata));
        _tagRepository
            .ExistsAsync(
                Arg.Any<Expression<Func<ImageTag, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddAsync("owner", tag, CancellationToken.None)
        );

        await _tagRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<ImageTag>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldDelete_WhenOwnerAndTagExists()
    {
        ImageTag tag = CreateTag();
        ImageMetadata metadata = CreateMetadata(tag.ImageMetadataId, "owner");

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(metadata));
        _tagRepository
            .ExistsAsync(
                Arg.Any<Expression<Func<ImageTag, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(true));

        await _service.RemoveAsync("owner", tag, CancellationToken.None);

        _tagRepository.Received(1).Remove(Arg.Is<ImageTag>(t => t == tag));
        await _tagRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenUnauthorized()
    {
        ImageTag tag = CreateTag();

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(null));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.RemoveAsync("owner", tag, CancellationToken.None)
        );

        _tagRepository.DidNotReceive().Remove(Arg.Any<ImageTag>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenOwnerMismatch()
    {
        ImageTag tag = CreateTag();
        ImageMetadata metadata = CreateMetadata(tag.ImageMetadataId, "different");

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(metadata));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.RemoveAsync("owner", tag, CancellationToken.None)
        );

        _tagRepository.DidNotReceive().Remove(Arg.Any<ImageTag>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenTagMissing()
    {
        ImageTag tag = CreateTag();
        ImageMetadata metadata = CreateMetadata(tag.ImageMetadataId, "owner");

        _metadataRepository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.ImageMetadataId)
            .Returns(Task.FromResult<ImageMetadata?>(metadata));
        _tagRepository
            .ExistsAsync(
                Arg.Any<Expression<Func<ImageTag, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(false));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemoveAsync("owner", tag, CancellationToken.None)
        );

        _tagRepository.DidNotReceive().Remove(Arg.Any<ImageTag>());
    }

    private static ImageTag CreateTag(int imageMetadataId = 1, string tagValue = "tag")
    {
        return new ImageTag
        {
            Id = 5,
            ImageMetadataId = imageMetadataId,
            ImageMetadata = CreateMetadata(imageMetadataId, "owner"),
            Tag = tagValue,
        };
    }

    private static ImageMetadata CreateMetadata(int id, string userId)
    {
        return new ImageMetadata
        {
            Id = id,
            Title = "image",
            UserId = userId,
        };
    }
}
