namespace Models;

public class UserFavoriteTag
{
    public int Id { get; set; }

    public required int UserPreferencesId { get; set; }
    public required UserPreferences UserPreferences { get; set; }

    public required string Tag { get; set; }
}
