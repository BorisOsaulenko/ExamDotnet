using FluentValidation;
using System.Linq;
using Models;
using Repositories;

namespace Services.User;

public partial class UserConsumerHistoryService : IUserConsumerHistoryService
{
    public UserConsumerHistoryService(IUserConsumerHistoryRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly IUserConsumerHistoryRepository _repository;

    public async Task<UserConsumerHistory> AddAsync(
        UserConsumerHistory entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        CreateUserHistoryValidator().ValidateAndThrow(entity);

        return await Task.FromResult(await _repository.AddAsync(entity, cancellationToken));
    }

    public async Task<Dictionary<ConsumerActivityType, List<UserConsumerHistory>>> GetUserHistory(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var userHistories = await Task.FromResult(
            _repository.Query().Where(history => history.UserId == userId).ToList()
        );

        var hierarchical = userHistories
            .GroupBy(history => history.ActivityType)
            .ToDictionary(group => group.Key, group => group.ToList());

        return hierarchical;
    }

    public async Task RemoveAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userHistories = await Task.FromResult(
            _repository.Query().Where(history => history.UserId == userId).ToList()
        );

        _repository.RemoveRange(userHistories);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
