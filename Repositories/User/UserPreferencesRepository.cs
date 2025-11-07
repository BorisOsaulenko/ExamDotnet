using Models;

namespace Repositories;

public class UserPreferencesRepository : GenericRepository<UserPreferences>
{
    public UserPreferencesRepository(ApplicationDbContext context)
        : base(context) { }
}
