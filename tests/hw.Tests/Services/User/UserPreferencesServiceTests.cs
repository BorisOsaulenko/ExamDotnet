using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using NSubstitute;
using Repositories;
using Services.User;

namespace hw.Tests.Services.User;

public sealed class UserPreferencesServiceTests
{
    private readonly IUserPreferencesRepository _repository;
    private readonly UserPreferencesService _service;

    public UserPreferencesServiceTests()
    {
        _repository = Substitute.For<IUserPreferencesRepository>();
        _service = new UserPreferencesService(_repository);
    }

    [Fact]
    public async Task AddAsync_ShouldResetIdAndPersist()
    {
        var preferences = CreatePreferences(id: 7);
        _repository.AddAsync(Arg.Any<UserPreferences>()).Returns(call => call.Arg<UserPreferences>());

        var result = await _service.AddAsync(preferences, CancellationToken.None);

        Assert.Equal(0, result.Id);
        _repository
            .Received(1)
            .AddAsync(Arg.Is<UserPreferences>(p => p.Id == 0 && p.UserId == preferences.UserId));
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnPreferences_WhenExists()
    {
        var existing = CreatePreferences(userId: "user-1", id: 11);
        _repository.Query().Returns(new List<UserPreferences> { existing }.AsQueryable());

        var result = await _service.GetByUserIdAsync("user-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(11, result!.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenMissing()
    {
        _repository.Query().Returns(Enumerable.Empty<UserPreferences>().AsQueryable());

        var result = await _service.GetByUserIdAsync("unknown", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var preferences = CreatePreferences(id: 5);

        await _service.UpdateAsync(preferences, CancellationToken.None);

        _repository.Received(1).Update(Arg.Is<UserPreferences>(p => p == preferences));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteExistingPreferences()
    {
        var existing = CreatePreferences(id: 9);
        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), existing.Id)
            .Returns(Task.FromResult<UserPreferences?>(existing));

        await _service.RemoveAsync(existing, CancellationToken.None);

        _repository.Received(1).Remove(Arg.Is<UserPreferences>(p => p.Id == existing.Id));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrow_WhenEntityMissing()
    {
        var preferences = CreatePreferences(id: 9);
        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), preferences.Id)
            .Returns(Task.FromResult<UserPreferences?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(preferences)
        );
        _repository.DidNotReceive().Remove(Arg.Any<UserPreferences>());
    }

    private static UserPreferences CreatePreferences(int id = 1, string userId = "user")
    {
        return new UserPreferences
        {
            Id = id,
            UserId = userId,
            User = new Models.User(),
        };
    }
}
