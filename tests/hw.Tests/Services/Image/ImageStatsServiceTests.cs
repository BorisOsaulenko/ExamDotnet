using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using hw.Tests.TestInfrastructure.AsyncEnumerable;
using Models;
using NSubstitute;
using Repositories;
using Services.Image;
using Xunit;

namespace hw.Tests.Services.Image;

public sealed class ImageStatsServiceTests
{
    private readonly IImageStatsRepository _repository = Substitute.For<IImageStatsRepository>();
    private readonly ImageStatsService _service;

    public ImageStatsServiceTests()
    {
        _service = new ImageStatsService(_repository);
    }

    [Fact]
    public async Task ShouldAddStatsWithResetId()
    {
        ImageStats stats = new()
        {
            Id = 99,
            ImageId = 1,
            Views = 10,
        };
        _repository
            .AddAsync(stats, Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                return Task.FromResult(callInfo.Arg<ImageStats>());
            });

        ImageStats added = await _service.AddAsync(stats, CancellationToken.None);

        Assert.Equal(0, added.Id);
        await _repository.Received(1).AddAsync(stats, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldReturnStatsByPredicate()
    {
        var data = new[]
        {
            new ImageStats
            {
                Id = 1,
                ImageId = 1,
                Views = 10,
            },
            new ImageStats
            {
                Id = 2,
                ImageId = 2,
                Views = 20,
            },
        };
        _repository
            .GetByPredicateAsync(
                Arg.Any<Expression<Func<ImageStats, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<Expression<Func<ImageStats, bool>>>();
                var filtered = data.AsQueryable().Where(predicate.Compile()).ToList();
                return Task.FromResult(filtered);
            });

        List<ImageStats> result = await _service.GetByPredicateAsync(
            stats => stats.Views >= 15,
            CancellationToken.None
        );

        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task ShouldUpdateStats()
    {
        ImageStats stats = new()
        {
            Id = 1,
            ImageId = 1,
            Views = 5,
        };

        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), stats.Id)
            .Returns(Task.FromResult<ImageStats?>(stats));

        await _service.UpdateAsync(stats, CancellationToken.None);

        _repository.Received(1).Update(stats);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowOnUpdateWhenStatsMissing()
    {
        ImageStats stats = new()
        {
            Id = 1,
            ImageId = 1,
            Views = 5,
        };

        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), stats.Id)
            .Returns(Task.FromResult<ImageStats?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(stats, CancellationToken.None)
        );

        _repository.DidNotReceive().Update(Arg.Any<ImageStats>());
    }

    [Fact]
    public async Task ShouldRemoveStats()
    {
        ImageStats stats = new() { Id = 1, ImageId = 1 };

        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), stats.Id)
            .Returns(Task.FromResult<ImageStats?>(stats));

        await _service.RemoveAsync(stats, CancellationToken.None);

        _repository.Received(1).Remove(stats);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowOnRemoveWhenStatsMissing()
    {
        ImageStats stats = new() { Id = 1, ImageId = 1 };
        _repository
            .GetByIdAsync(Arg.Any<CancellationToken>(), stats.Id)
            .Returns(Task.FromResult<ImageStats?>(null));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemoveAsync(stats, CancellationToken.None)
        );

        _repository.DidNotReceive().Remove(Arg.Any<ImageStats>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
