using System.Linq.Expressions;
using UserModel = Models.User;

namespace Services.User;

public interface IUserService
{
    Task<List<UserModel>> GetWithPaginationAsync(
        int skip,
        int size,
        CancellationToken cancellationToken = default
    );
    Task<List<UserModel>> GetByPredicateAsync(
        Expression<Func<UserModel, bool>> predicate,
        CancellationToken cancellationToken = default
    );
    Task<UserModel?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
}
