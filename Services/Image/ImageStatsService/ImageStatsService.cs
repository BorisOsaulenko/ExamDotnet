using System.Linq.Expressions;
using Models;
using Repositories;
using Services.Identity;

namespace Services.Image;

public class ImageStatsService : IImageStatsService
{
    public ImageStatsService(ImageStatsRepository repository)
    {
        _repository = repository;
    }

    private readonly ImageStatsRepository _repository;

    public async Task<ImageStats> AddAsync(
        ImageStats entity,
        CancellationToken cancellationToken = default
    )
    {
        entity.Id = 0;
        return await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task<List<ImageStats>> GetByPredicateAsync(
        Expression<Func<ImageStats, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return _repository.GetByPredicateAsync(predicate, cancellationToken);
    }

    public async Task Update(ImageStats entity, CancellationToken cancellationToken = default)
    {
        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task Remove(ImageStats entity, CancellationToken cancellationToken = default)
    {
        _repository.Remove(entity);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
