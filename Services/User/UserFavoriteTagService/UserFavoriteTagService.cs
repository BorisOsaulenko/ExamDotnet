using System.Linq;
using Models;
using Repositories;

namespace Services.User;

public class UserFavoriteTagService : IUserFavoriteTagService
{
    public UserFavoriteTagService(
        IUserFavoriteTagRepository repository,
        IUserPreferencesRepository preferencesRepository
    )
    {
        _repository = repository;
        _preferencesRepository = preferencesRepository;
    }

    private readonly IUserFavoriteTagRepository _repository;
    private readonly IUserPreferencesRepository _preferencesRepository;

    public async Task<UserFavoriteTag> AddAsync(
        string userId,
        UserFavoriteTag entity,
        CancellationToken cancellationToken = default
    )
    {
        await CheckUserPreferencesOwnership(userId, entity.UserPreferencesId).ConfigureAwait(false);

        UserFavoriteTag? existingEntity = await Task.FromResult(
            _repository
                .Query()
                .FirstOrDefault(tag =>
                    tag.UserPreferencesId == entity.UserPreferencesId && tag.Tag == entity.Tag
                )
        );

        if (existingEntity != null)
            throw new InvalidOperationException("Entity already exists.");

        entity.Id = 0;
        return await Task.FromResult(await _repository.AddAsync(entity));
    }

    public async Task RemoveAsync(
        string userId,
        UserFavoriteTag entity,
        CancellationToken cancellationToken = default
    )
    {
        await CheckUserPreferencesOwnership(userId, entity.UserPreferencesId).ConfigureAwait(false);

        UserFavoriteTag existingEntity =
            await _repository.GetByIdAsync(cancellationToken, entity.Id).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Entity does not exist.");

        _repository.Remove(existingEntity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAllAsync(
        int userPreferencesId,
        CancellationToken cancellationToken = default
    )
    {
        var userFavoriteTags = await Task.FromResult(
            _repository.Query().Where(tag => tag.UserPreferencesId == userPreferencesId).ToList()
        );

        _repository.RemoveRange(userFavoriteTags);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task CheckUserPreferencesOwnership(string userId, int preferencesId)
    {
        var preferences =
            await Task.FromResult(
                _preferencesRepository
                    .Query()
                    .FirstOrDefault(pref => pref.UserId == userId && pref.Id == preferencesId)
            ) ?? throw new InvalidOperationException("User preferences not found.");
    }
}
