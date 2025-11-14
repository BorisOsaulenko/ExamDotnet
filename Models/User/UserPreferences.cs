using System.Text.Json.Serialization;

namespace Models;

public enum Theme
{
    Light,
    Dark,
    SystemDefault,
}

public class UserPreferences
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    public bool ReceiveNotifications { get; set; }
    public ICollection<UserFavoriteTag> FavoriteTags { get; set; } = [];
    public ICollection<User> FavoriteAuthors { get; set; } = [];
    public ICollection<ImageCollection> SubscribedCollections { get; set; } = [];
    public ICollection<ImageStats> LikedImages { get; set; } = [];
    public Theme Theme { get; set; } = Theme.SystemDefault;
}
