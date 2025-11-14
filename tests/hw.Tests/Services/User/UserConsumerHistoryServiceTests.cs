using System.Linq;
using FluentValidation;
using Models;
using NSubstitute;
using Repositories;
using Services.User;

namespace hw.Tests.Services.User;

public sealed class UserConsumerHistoryServiceTests
{
    private readonly IUserConsumerHistoryRepository _repository;
    private readonly UserConsumerHistoryService _service;

    public UserConsumerHistoryServiceTests()
    {
        _repository = Substitute.For<IUserConsumerHistoryRepository>();
        _service = new UserConsumerHistoryService(_repository);
    }

    [Fact]
    public async Task AddAsync_ShouldResetIdAndPersist()
    {
        // Arrange
        var history = CreateValidHistory(h => h.Id = 42);
        _repository
            .AddAsync(Arg.Any<UserConsumerHistory>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<UserConsumerHistory>()));

        // Act
        var result = await _service.AddAsync(history, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Id);
        await _repository
            .Received(1)
            .AddAsync(Arg.Is<UserConsumerHistory>(h => h.Id == 0), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldValidateInput()
    {
        // Arrange
        var invalid = CreateValidHistory(h => h.UserId = string.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.AddAsync(invalid));
        await _repository
            .DidNotReceive()
            .AddAsync(Arg.Any<UserConsumerHistory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserHistory_ShouldGroupByActivityType()
    {
        // Arrange
        var histories = new List<UserConsumerHistory>
        {
            CreateValidHistory(h =>
            {
                h.UserId = "user-1";
                h.ActivityType = ConsumerActivityType.ImageDownload;
            }),
            CreateValidHistory(h =>
            {
                h.UserId = "user-1";
                h.ActivityType = ConsumerActivityType.CollectionView;
                h.ImageId = null;
                h.CollectionId = 5;
            }),
            CreateValidHistory(h =>
            {
                h.UserId = "user-2";
                h.ActivityType = ConsumerActivityType.CollectionShare;
                h.ImageId = null;
                h.CollectionId = 6;
            }),
        };

        _repository.Query().Returns(histories.AsQueryable());

        // Act
        var result = await _service.GetUserHistory("user-1", CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(ConsumerActivityType.ImageDownload, result.Keys);
        Assert.Contains(ConsumerActivityType.CollectionView, result.Keys);
        Assert.Single(result[ConsumerActivityType.ImageDownload]);
        Assert.Single(result[ConsumerActivityType.CollectionView]);
    }

    [Fact]
    public async Task RemoveAllAsync_ShouldRemoveAndPersistChanges()
    {
        // Arrange
        var removable = new List<UserConsumerHistory>
        {
            CreateValidHistory(h => h.UserId = "remove-me"),
            CreateValidHistory(h => h.UserId = "remove-me"),
        };

        _repository.Query().Returns(removable.AsQueryable());

        // Act
        await _service.RemoveAllAsync("remove-me", CancellationToken.None);

        // Assert
        _repository
            .Received(1)
            .RemoveRange(Arg.Is<IEnumerable<UserConsumerHistory>>(list => list.Count() == 2));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UserConsumerHistory CreateValidHistory(
        Action<UserConsumerHistory>? configure = null
    )
    {
        var history = new UserConsumerHistory
        {
            Id = 1,
            UserId = "user-id",
            ActivityType = ConsumerActivityType.ImageDownload,
            ActivityDate = DateTime.UtcNow,
            ImageId = 3,
        };

        configure?.Invoke(history);
        return history;
    }
}
