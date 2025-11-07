using System.Linq.Expressions;
using Models;

namespace Services.Image;

public interface IImageAllowedUserService
{
    Task<List<ImageAllowedUser>> GetByPredicateAsync(
        Expression<Func<ImageAllowedUser, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<ImageAllowedUser> AddAsync(
        ImageAllowedUser entity,
        CancellationToken cancellationToken = default
    );
    Task Remove(ImageAllowedUser entity, CancellationToken cancellationToken = default);
}
