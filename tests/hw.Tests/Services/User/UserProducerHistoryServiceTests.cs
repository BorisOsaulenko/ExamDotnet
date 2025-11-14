using System.Linq;
using FluentValidation;
using Models;
using NSubstitute;
using Repositories;
using Services.User;

namespace hw.Tests.Services.User;

public sealed class UserProducerHistoryServiceTests
{
    private readonly IUserProducerHistoryRepository _repository;
    private readonly UserProducerHistoryService _service;

    public UserProducerHistoryServiceTests()
    {
        _repository = Substitute.For<IUserProducerHistoryRepository>();
        _service = new UserProducerHistoryService(_repository);
    }

    [Fact]
    public async Task AddAsync_ShouldResetIdentifierAndSave()
    {
        // Arrange
        var history = CreateValidHistory(entity => entity.Id = 99);
        _repository
            .AddAsync(Arg.Any<UserProducerHistory>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<UserProducerHistory>()));

        // Act
        var result = await _service.AddAsync(history, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Id);
        await _repository
            .Received(1)
            .AddAsync(
                Arg.Is<UserProducerHistory>(entity =>
                    entity.Id == 0 && entity.ImageId == history.ImageId
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowWhenValidationFails()
    {
        // Arrange
        var invalidHistory = CreateValidHistory(entity => entity.UserId = string.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.AddAsync(invalidHistory));
        await _repository
            .DidNotReceive()
            .AddAsync(Arg.Any<UserProducerHistory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserHistory_ShouldGroupByHistoryType()
    {
        // Arrange
        var histories = new List<UserProducerHistory>
        {
            CreateValidHistory(entity =>
            {
                entity.UserId = "user-1";
                entity.ImageId = 11;
                entity.ActivityType = ProducerActivityType.ImageUpload;
            }),
            CreateValidHistory(entity =>
            {
                entity.UserId = "user-1";
                entity.ImageId = null;
                entity.CollectionId = 7;
                entity.ActivityType = ProducerActivityType.CollectionCreate;
            }),
            CreateValidHistory(entity =>
            {
                entity.UserId = "user-2";
                entity.ImageId = 22;
            }),
        };

        _repository.Query().Returns(histories.AsQueryable());

        // Act
        var result = await _service.GetUserHistory("user-1", CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(UserHistoryType.Image, result.Keys);
        Assert.Contains(UserHistoryType.Collection, result.Keys);
        Assert.Single(result[UserHistoryType.Image]);
        Assert.Single(result[UserHistoryType.Collection]);
        Assert.Equal(11, result[UserHistoryType.Image].First().HistoryId);
        Assert.Equal(7, result[UserHistoryType.Collection].First().HistoryId);
    }

    [Fact]
    public async Task RemoveAllAsync_ShouldRemovePersistedHistories()
    {
        // Arrange
        var persisted = new List<UserProducerHistory>
        {
            CreateValidHistory(entity => entity.UserId = "removable-user"),
            CreateValidHistory(entity => entity.UserId = "removable-user"),
        };

        _repository.Query().Returns(persisted.AsQueryable());

        // Act
        await _service.RemoveAllAsync("removable-user", CancellationToken.None);

        // Assert
        _repository
            .Received(1)
            .RemoveRange(Arg.Is<IEnumerable<UserProducerHistory>>(list => list.Count() == 2));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UserProducerHistory CreateValidHistory(
        Action<UserProducerHistory>? configure = null
    )
    {
        var history = new UserProducerHistory
        {
            Id = 1,
            UserId = "user-id",
            ActivityType = ProducerActivityType.ImageUpload,
            ActivityDate = DateTime.UtcNow,
            ImageId = 5,
        };

        configure?.Invoke(history);
        return history;
    }
}
