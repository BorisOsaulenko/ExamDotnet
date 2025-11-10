using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services.User;

public partial class UserConsumerHistoryService : IUserConsumerHistoryService
{
    public UserConsumerHistoryService(UserConsumerHistoryRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly UserConsumerHistoryRepository _repository;

    public async Task<UserConsumerHistory> AddAsync(
        UserConsumerHistory entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        CreateUserHistoryValidator().ValidateAndThrow(entity);

        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Dictionary<ConsumerActivityType, List<UserConsumerHistory>>> GetUserHistory(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        List<UserConsumerHistory> userHistories = await _repository
            .Query()
            .AsNoTracking()
            .Where(history => history.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var hierarchical = userHistories
            .GroupBy(history => history.ActivityType)
            .ToDictionary(group => group.Key, group => group.ToList());

        return hierarchical;
    }

    public async Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userHistories = await _repository
            .Query()
            .Where(history => history.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _repository.RemoveRange(userHistories);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
