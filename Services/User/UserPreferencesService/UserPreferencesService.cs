using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services.User;

public class UserPreferencesService : IUserPreferencesService
{
    public UserPreferencesService(UserPreferencesRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly UserPreferencesRepository _repository;

    public async Task<UserPreferences> AddAsync(
        UserPreferences entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserPreferences?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        UserPreferences? preference = await _repository
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(preference => preference.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        return preference;
    }

    public async Task UpdateAsync(
        UserPreferences entity,
        CancellationToken cancellationToken = default
    )
    {
        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        UserPreferences entity,
        CancellationToken cancellationToken = default
    )
    {
        UserPreferences existingEntity =
            await _repository.GetByIdAsync(cancellationToken, entity.Id).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Entity does not exist.");

        _repository.Remove(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
