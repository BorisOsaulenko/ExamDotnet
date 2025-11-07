using System.Linq.Expressions;
using Models;

namespace Services.Image;

public interface IImageCommentService
{
    Task<ImageComment> AddAsync(ImageComment entity, CancellationToken cancellationToken = default);
    Task<List<ImageComment>> GetByPredicateAsync(
        Expression<Func<ImageComment, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task Update(ImageComment entity, CancellationToken cancellationToken = default);
    Task Remove(ImageComment entity, CancellationToken cancellationToken = default);
}
