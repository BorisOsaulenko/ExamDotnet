using System.Linq.Expressions;
using Models;

namespace Services.Image;

public interface IImageCommentService
{
    Task<ImageComment> AddAsync(ImageComment entity, CancellationToken cancellationToken = default);
    Task<List<ImageComment>> GetByPredicateAsync(
        string currentUserId,
        Expression<Func<ImageComment, bool>> predicate,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(ImageComment entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(ImageComment entity, CancellationToken cancellationToken = default);
}
