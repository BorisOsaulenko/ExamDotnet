using System.Linq.Expressions;
using Repositories;
using UserModel = Models.User;

namespace Services.User;

public class UserService : IUserService
{
    public UserService(UserRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly UserRepository _repository;

    public Task<List<UserModel>> GetWithPaginationAsync(
        int skip,
        int size,
        CancellationToken cancellationToken = default
    )
    {
        if (skip < 0 || size <= 0)
            throw new ArgumentException("Invalid pagination parameters.");
        return _repository.GetWithPaginationAsync(skip, size, cancellationToken);
    }

    public Task<List<UserModel>> GetByPredicateAsync(
        Expression<Func<UserModel, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return _repository.GetByPredicateAsync(predicate, cancellationToken);
    }

    public Task<UserModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(cancellationToken, id);
    }
}
