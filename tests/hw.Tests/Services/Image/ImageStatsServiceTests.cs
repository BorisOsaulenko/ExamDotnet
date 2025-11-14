using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;
using UserModel = Models.User;
using ImageModel = Models.Image;

namespace hw.Tests.Services.Image;

public sealed class ImageStatsServiceTests
{
    private readonly IImageStatsRepository _statsRepository =
        Substitute.For<IImageStatsRepository>();
    private readonly IImageMetadataRepository _metadataRepository =
        Substitute.For<IImageMetadataRepository>();
    private readonly IUserPreferencesRepository _preferencesRepository =
        Substitute.For<IUserPreferencesRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    private readonly ImageStatsService _service;

    public ImageStatsServiceTests()
    {
        _service = new ImageStatsService(
            _statsRepository,
            _metadataRepository,
            _preferencesRepository,
            _userRepository
        );
    }

    [Fact]
    public async Task AddAsync_ShouldResetIdAndPersist()
    {
        ImageStats stats = CreateStats(id: 7);

        _statsRepository.AddAsync(Arg.Any<ImageStats>()).Returns(callInfo => callInfo.Arg<ImageStats>());

        ImageStats result = await _service.AddAsync(stats, CancellationToken.None);

        Assert.Equal(0, stats.Id);
        Assert.Equal(stats, result);
        _statsRepository.Received(1).AddAsync(Arg.Is<ImageStats>(s => s == stats));
    }

    [Fact]
    public async Task IncrementViewsAsync_ShouldUpdateAndPersist()
    {
        ImageStats stats = CreateStats(views: 1);
        SetupMetadataQuery(stats);

        await _service.IncrementViewsAsync(5, CancellationToken.None);

        Assert.Equal(2, stats.Views);
        _statsRepository.Received(1).Update(Arg.Is<ImageStats>(s => s == stats));
        await _statsRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IncrementDownloadsAsync_ShouldUpdateAndPersist()
    {
        ImageStats stats = CreateStats(downloads: 3);
        SetupMetadataQuery(stats);

        await _service.IncrementDownloadsAsync(5, CancellationToken.None);

        Assert.Equal(4, stats.Downloads);
        _statsRepository.Received(1).Update(Arg.Is<ImageStats>(s => s == stats));
        await _statsRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IncrementSharesAsync_ShouldUpdateAndPersist()
    {
        ImageStats stats = CreateStats(shares: 2);
        SetupMetadataQuery(stats);

        await _service.IncrementSharesAsync(5, CancellationToken.None);

        Assert.Equal(3, stats.Shares);
        _statsRepository.Received(1).Update(Arg.Is<ImageStats>(s => s == stats));
        await _statsRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IncrementViewsAsync_ShouldThrow_WhenMetadataMissing()
    {
        _metadataRepository.Query().Returns(Enumerable.Empty<ImageMetadata>().AsQueryable());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.IncrementViewsAsync(9, CancellationToken.None)
        );

        _statsRepository.DidNotReceive().Update(Arg.Any<ImageStats>());
        await _statsRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IncrementViewsAsync_ShouldThrow_WhenStatsMissing()
    {
        ImageMetadata metadata = new()
        {
            Id = 5,
            Title = "image",
            UserId = "owner",
            ImageStats = null,
        };

        _metadataRepository.Query().Returns(new[] { metadata }.AsQueryable());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.IncrementViewsAsync(5, CancellationToken.None)
        );

        _statsRepository.DidNotReceive().Update(Arg.Any<ImageStats>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteAndPersist()
    {
        ImageStats stats = CreateStats();

        await _service.RemoveAsync(stats, CancellationToken.None);

        _statsRepository.Received(1).Remove(Arg.Is<ImageStats>(s => s == stats));
        await _statsRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldAddLike_WhenUserHasNotLiked()
    {
        await using ApplicationDbContext context = CreateContext();
        ImageStatsService service = CreateServiceWithContext(context);

        ImageMetadata metadata = await SeedImageAsync(context);
        UserModel user = context.Users.Single();

        bool liked = await service.ToggleLikeAsync(metadata.Id, user.Id, CancellationToken.None);

        Assert.True(liked);

        UserPreferences storedPreferences = context.UserPreferences
            .Include(p => p.LikedImages)
            .Single();

        Assert.Single(storedPreferences.LikedImages);
        Assert.Equal(
            metadata.ImageStats!.Id,
            storedPreferences.LikedImages.First().Id
        );
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldRemoveLike_WhenUserAlreadyLiked()
    {
        await using ApplicationDbContext context = CreateContext();
        ImageStatsService service = CreateServiceWithContext(context);

        ImageMetadata metadata = await SeedImageAsync(context);
        UserModel user = context.Users.Single();

        UserPreferences preferences = context.UserPreferences
            .Include(p => p.LikedImages)
            .Single();
        preferences.LikedImages.Add(metadata.ImageStats!);
        context.Update(preferences);
        await context.SaveChangesAsync();

        bool liked = await service.ToggleLikeAsync(metadata.Id, user.Id, CancellationToken.None);

        Assert.False(liked);

        UserPreferences storedPreferences = context.UserPreferences
            .Include(p => p.LikedImages)
            .Single();

        Assert.Empty(storedPreferences.LikedImages);
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldCreatePreferences_WhenMissing()
    {
        await using ApplicationDbContext context = CreateContext();
        ImageStatsService service = CreateServiceWithContext(context);

        ImageMetadata metadata = await SeedImageAsync(context);
        UserModel user = context.Users.Single();

        context.UserPreferences.RemoveRange(context.UserPreferences);
        await context.SaveChangesAsync();

        bool liked = await service.ToggleLikeAsync(metadata.Id, user.Id, CancellationToken.None);

        Assert.True(liked);

        UserPreferences storedPreferences = context.UserPreferences
            .Include(p => p.LikedImages)
            .Single();

        Assert.Single(storedPreferences.LikedImages);
    }

    private void SetupMetadataQuery(ImageStats stats)
    {
        ImageMetadata metadata = new()
        {
            Id = 5,
            Title = "image",
            UserId = "owner",
            ImageStats = stats,
        };

        _metadataRepository.Query().Returns(new[] { metadata }.AsQueryable());
    }

    private static ImageStats CreateStats(
        int id = 1,
        int views = 0,
        int downloads = 0,
        int shares = 0
    )
    {
        return new ImageStats
        {
            Id = id,
            Views = views,
            Downloads = downloads,
            Shares = shares,
        };
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<ImageMetadata> SeedImageAsync(ApplicationDbContext context)
    {
        UserModel user = new()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user",
            Email = "user@example.com",
        };

        ImageModel image = new()
        {
            ContentType = ImageContentType.Jpeg,
            Size = 10,
            BlobName = "blob",
            BlobUri = "uri",
        };

        ImageStats stats = new();
        ImageMetadata metadata = new()
        {
            Title = "image",
            UserId = user.Id,
            Image = image,
            AccessLevel = ImageAccessLevel.Public,
            ImageStats = stats,
        };

        stats.ImageMetadata = metadata;

        UserPreferences preferences = new()
        {
            UserId = user.Id,
            User = user,
        };

        context.Users.Add(user);
        context.UserPreferences.Add(preferences);
        context.Images.Add(image);
        context.ImageStats.Add(stats);
        context.ImageMetadata.Add(metadata);

        await context.SaveChangesAsync();

        return metadata;
    }

    private static ImageStatsService CreateServiceWithContext(ApplicationDbContext context)
    {
        var statsRepository = new ImageStatsRepository(context);
        var metadataRepository = new ImageMetadataRepository(context);
        var preferencesRepository = new UserPreferencesRepository(context);
        var userRepository = new UserRepository(context);

        return new ImageStatsService(
            statsRepository,
            metadataRepository,
            preferencesRepository,
            userRepository
        );
    }
}
