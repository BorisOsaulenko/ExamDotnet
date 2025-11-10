using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services.User;

public partial class UserProducerHistoryService : IUserProducerHistoryService
{
    public UserProducerHistoryService(UserProducerHistoryRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly UserProducerHistoryRepository _repository;

    public async Task<UserProducerHistory> AddAsync(
        UserProducerHistory entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        CreateUserHistoryValidator().ValidateAndThrow(entity);
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Dictionary<UserHistoryType, List<UserHistoryKey>>> GetUserHistory(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var userHistoryKeys = await _repository
            .Query()
            .AsNoTracking()
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
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var hierarchical = userHistoryKeys
            .GroupBy(key => key.HistoryType)
            .ToDictionary(group => group.Key, group => group.ToList());

        return hierarchical;
    }

    public async Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var userHistories = await _repository
            .Query()
            .Where(history => history.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _repository.RemoveRange(userHistories);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
