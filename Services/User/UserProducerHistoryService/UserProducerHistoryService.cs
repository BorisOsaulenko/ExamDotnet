using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services.User;

public partial class UserProducerHistoryService : IUserProducerHistoryService
{
    public UserProducerHistoryService(IUserProducerHistoryRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly IUserProducerHistoryRepository _repository;

    public async Task<UserProducerHistory> AddAsync(
        UserProducerHistory entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        CreateUserHistoryValidator().ValidateAndThrow(entity);
        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
    }

    public async Task<Dictionary<UserHistoryType, List<UserHistoryKey>>> GetUserHistory(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var userHistoryKeys = await Task.FromResult(
            _repository
                .Query()
                .Where(history =>
                    history.UserId == userId
                    && (history.ImageId != null || history.CollectionId != null)
                )
                .GroupBy(history => new
                {
                    HistoryType = history.ImageId == null
                        ? UserHistoryType.Collection
                        : UserHistoryType.Image,
                    HistoryId = history.ImageId == null ? history.CollectionId : history.ImageId,
                })
                .Select(group => new UserHistoryKey(group.Key.HistoryType, group.Key.HistoryId))
                .ToList()
        );

        var hierarchical = userHistoryKeys
            .GroupBy(key => key.HistoryType)
            .ToDictionary(group => group.Key, group => group.ToList());

        return hierarchical;
    }

    public async Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var userHistories = await Task.FromResult(
            _repository.Query().Where(history => history.UserId == userId).ToList()
        );

        _repository.RemoveRange(userHistories);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
