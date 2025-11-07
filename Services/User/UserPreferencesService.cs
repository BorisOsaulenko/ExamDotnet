using Models;
using Repositories;

namespace Services.User;

public class UserPreferencesService : GenericService<UserPreferences>, IUserPreferencesService
{
    public UserPreferencesService(UserPreferencesRepository repository)
        : base(repository) { }
}
