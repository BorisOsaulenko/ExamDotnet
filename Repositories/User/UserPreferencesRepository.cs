using Models;

namespace Repositories;

public class UserPreferencesRepository
    : GenericRepository<UserPreferences>,
        IUserPreferencesRepository
{
    public UserPreferencesRepository(ApplicationDbContext context)
        : base(context) { }
}
