using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services.User;

public class UserFavoriteTagService : IUserFavoriteTagService
{
    public UserFavoriteTagService(UserFavoriteTagRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly UserFavoriteTagRepository _repository;

    public async Task<UserFavoriteTag> AddAsync(
        UserFavoriteTag entity,
        CancellationToken cancellationToken = default
    )
    {
        UserFavoriteTag? existingEntity = await _repository
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                tag => tag.UserPreferencesId == entity.UserPreferencesId && tag.Tag == entity.Tag,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (existingEntity != null)
        {
            throw new InvalidOperationException("Entity already exists.");
        }

        entity.Id = 0;
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        UserFavoriteTag entity,
        CancellationToken cancellationToken = default
    )
    {
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
        var userFavoriteTags = await _repository
            .Query()
            .Where(tag => tag.UserPreferencesId == userPreferencesId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _repository.RemoveRange(userFavoriteTags);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
