using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using NSubstitute;
using Repositories;
using Services.User;

namespace hw.Tests.Services.User;

public sealed class UserFavoriteTagServiceTests
{
    private readonly IUserFavoriteTagRepository _repository;
    private readonly IUserPreferencesRepository _preferencesRepository;
    private readonly UserFavoriteTagService _service;

    public UserFavoriteTagServiceTests()
    {
        _repository = Substitute.For<IUserFavoriteTagRepository>();
        _preferencesRepository = Substitute.For<IUserPreferencesRepository>();
        _service = new UserFavoriteTagService(_repository, _preferencesRepository);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistWhenUserOwnsPreferences()
    {
        const string userId = "user-1";
        var tag = CreateTag(preferencesId: 10, tagValue: "travel", id: 5);
        _preferencesRepository
            .Query()
            .Returns(new[] { CreatePreferences(10, userId) }.AsQueryable());
        _repository.Query().Returns(Enumerable.Empty<UserFavoriteTag>().AsQueryable());
        _repository
            .AddAsync(Arg.Any<UserFavoriteTag>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<UserFavoriteTag>()));

        var result = await _service.AddAsync(userId, tag, CancellationToken.None);

        Assert.Equal(0, result.Id);
        await _repository
            .Received(1)
            .AddAsync(
                Arg.Is<UserFavoriteTag>(t =>
                    t.Id == 0 && t.UserPreferencesId == 10 && t.Tag == "travel"
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowWhenDuplicateExists()
    {
        const string userId = "user-1";
        var tag = CreateTag(preferencesId: 10, tagValue: "travel", id: 2);
        _preferencesRepository
            .Query()
            .Returns(new[] { CreatePreferences(10, userId) }.AsQueryable());
        _repository
            .Query()
            .Returns(
                new[] { CreateTag(preferencesId: 10, tagValue: "travel", id: 7) }.AsQueryable()
            );

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(userId, tag));
        await _repository
            .DidNotReceive()
            .AddAsync(Arg.Any<UserFavoriteTag>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ShouldThrowWhenUserDoesNotOwnPreferences()
    {
        const string userId = "user-1";
        var tag = CreateTag(preferencesId: 10, tagValue: "macro", id: 2);
        _preferencesRepository
            .Query()
            .Returns(new[] { CreatePreferences(11, userId) }.AsQueryable());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(userId, tag));
        await _repository
            .DidNotReceive()
            .AddAsync(Arg.Any<UserFavoriteTag>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteWhenUserOwnsPreferences()
    {
        const string userId = "user-1";
        var tag = CreateTag(preferencesId: 10, tagValue: "macro", id: 5);
        _preferencesRepository
            .Query()
            .Returns(new[] { CreatePreferences(10, userId) }.AsQueryable());
        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.Id)
            .Returns(Task.FromResult<UserFavoriteTag?>(tag));

        await _service.RemoveAsync(userId, tag, CancellationToken.None);

        _repository.Received(1).Remove(Arg.Is<UserFavoriteTag>(t => t.Id == tag.Id));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowWhenTagMissing()
    {
        const string userId = "user-1";
        var tag = CreateTag(preferencesId: 10, tagValue: "macro", id: 5);
        _preferencesRepository
            .Query()
            .Returns(new[] { CreatePreferences(10, userId) }.AsQueryable());
        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), tag.Id)
            .Returns(Task.FromResult<UserFavoriteTag?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(userId, tag)
        );
        _repository.DidNotReceive().Remove(Arg.Any<UserFavoriteTag>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowWhenUserDoesNotOwnPreferences()
    {
        const string userId = "user-1";
        var tag = CreateTag(preferencesId: 10, tagValue: "macro", id: 5);
        _preferencesRepository
            .Query()
            .Returns(new[] { CreatePreferences(11, userId) }.AsQueryable());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveAsync(userId, tag)
        );
        await _repository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<CancellationToken>(), Arg.Any<int>());
    }

    [Fact]
    public async Task RemoveAllAsync_ShouldDeleteAllPreferencesTags()
    {
        var tags = new[]
        {
            CreateTag(preferencesId: 15, tagValue: "macro", id: 1),
            CreateTag(preferencesId: 15, tagValue: "street", id: 2),
        };

        _repository.Query().Returns(tags.AsQueryable());

        await _service.RemoveAllAsync(15, CancellationToken.None);

        _repository
            .Received(1)
            .RemoveRange(
                Arg.Is<IEnumerable<UserFavoriteTag>>(collection => collection.Count() == 2)
            );
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UserFavoriteTag CreateTag(int preferencesId, string tagValue, int id)
    {
        return new UserFavoriteTag
        {
            Id = id,
            UserPreferencesId = preferencesId,
            UserPreferences = new UserPreferences
            {
                Id = preferencesId,
                UserId = "owner",
                User = new Models.User(),
            },
            Tag = tagValue,
        };
    }

    private static UserPreferences CreatePreferences(int id, string userId)
    {
        return new UserPreferences
        {
            Id = id,
            UserId = userId,
            User = new Models.User(),
        };
    }
}
